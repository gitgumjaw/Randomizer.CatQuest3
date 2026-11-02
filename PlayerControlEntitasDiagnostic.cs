// Temporary read-only Cat Quest III control-loss diagnostic.
// F9 toggles a 25-second capture. No Harmony patches required.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    internal sealed class PlayerControlEntitasDiagnostic : MonoBehaviour
    {
        private const float Duration = 25f;
        private const float Interval = 0.25f;
        private const float NearbyRadius = 18f;
        private const int NearbyLimit = 12;
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private bool capturing;
        private float until;
        private float next;
        private int sample;
        private readonly Dictionary<string, string> prior = new Dictionary<string, string>();

        internal static void Initialize()
        {
            if (GameObject.Find("Randomizer_EntitasControlDiagnostic") != null) return;
            GameObject go = new GameObject("Randomizer_EntitasControlDiagnostic");
            DontDestroyOnLoad(go);
            go.AddComponent<PlayerControlEntitasDiagnostic>();
        }

        private void Awake()
        {
            Plugin.Log.LogInfo("CONTROL2 | Ready. F9 toggles a 25s control and entity capture.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (capturing) StopCapture();
                else StartCapture();
            }
            if (!capturing) return;
            if (Time.unscaledTime >= until) { StopCapture(); return; }
            if (Time.unscaledTime < next) return;
            next = Time.unscaledTime + Interval;
            Snapshot();
        }

        private void StartCapture()
        {
            capturing = true;
            until = Time.unscaledTime + Duration;
            next = Time.unscaledTime;
            sample = 0;
            prior.Clear();
            Plugin.Log.LogWarning("CONTROL2 | ===== START ===== | scene=" + SceneManager.GetActiveScene().name);
        }

        private void StopCapture()
        {
            capturing = false;
            Plugin.Log.LogWarning("CONTROL2 | ===== END ===== | samples=" + sample);
        }

        private void Snapshot()
        {
            sample++;
            try
            {
                MonoBehaviour[] behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
                List<Component> links = new List<Component>();
                foreach (MonoBehaviour behaviour in behaviours)
                {
                    if (behaviour != null && behaviour.GetType().FullName == "Entitas.Unity.EntityLink")
                        links.Add(behaviour);
                }
                List<Component> players = new List<Component>();
                foreach (var link in links)
                {
                    if (link == null || !link.gameObject.scene.IsValid() || !link.gameObject.activeInHierarchy) continue;
                    string name = link.gameObject.name;
                    if (name == "Player_Ship" || name.StartsWith("Player_Float", StringComparison.OrdinalIgnoreCase))
                        players.Add(link);
                }
                Plugin.Log.LogInfo("CONTROL2 | TICK " + sample + " | t=" + Time.unscaledTime.ToString("F2") +
                    " | scale=" + Time.timeScale + " | scene=" + SceneManager.GetActiveScene().name +
                    " | playerLinks=" + players.Count);

                Vector3 anchor = Vector3.zero;
                bool foundAnchor = false;
                foreach (var link in players)
                {
                    if (!foundAnchor || link.gameObject.name.StartsWith("Player_Float", StringComparison.OrdinalIgnoreCase))
                    {
                        anchor = link.transform.position;
                        foundAnchor = true;
                    }
                    Inspect(link, true);
                }
                if (!foundAnchor) return;
                List<Component> nearby = new List<Component>();
                foreach (var link in links)
                {
                    if (link == null || !link.gameObject.scene.IsValid() || !link.gameObject.activeInHierarchy || players.Contains(link)) continue;
                    if (Vector3.Distance(link.transform.position, anchor) <= NearbyRadius)
                        nearby.Add(link);
                }
                nearby.Sort((a, b) => Vector3.SqrMagnitude(a.transform.position - anchor)
                    .CompareTo(Vector3.SqrMagnitude(b.transform.position - anchor)));
                Plugin.Log.LogInfo("CONTROL2 | NEARBY | within=" + NearbyRadius + " | total=" + nearby.Count);
                for (int i = 0; i < Math.Min(NearbyLimit, nearby.Count); i++) Inspect(nearby[i], false);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("CONTROL2 | Snapshot error: " + ex.GetType().Name + ":" + ex.Message);
            }
        }

        private void Inspect(Component link, bool player)
        {
            string id = link.GetInstanceID().ToString();
            string label = (player ? "PLAYER" : "NEAR") + " | " + link.gameObject.name + " | id=" + id;
            Transform tr = link.transform;
            string spatial = "pos=" + tr.position.ToString("F2") + " rotY=" + tr.eulerAngles.y.ToString("F1") +
                " forward=" + tr.forward.ToString("F2");
            EmitChange("spatial:" + id, spatial, label);
            object entity = Read(link, "entity") ?? Read(link, "Entity");
            if (entity == null)
            {
                EmitChange("entity:" + id, "NO ENTITY accessible", label);
                return;
            }
            MethodInfo getComponents = entity.GetType().GetMethod("GetComponents", Members, null, Type.EmptyTypes, null);
            if (getComponents == null)
            {
                EmitChange("entity:" + id, "GetComponents unavailable | " + entity.GetType().FullName, label);
                return;
            }
            object[] components = getComponents.Invoke(entity, null) as object[];
            if (components == null) return;
            List<string> names = new List<string>();
            foreach (object component in components)
                if (component != null) names.Add(component.GetType().Name);
            names.Sort(StringComparer.Ordinal);
            EmitChange("types:" + id, string.Join(",", names.ToArray()), label + " | COMPONENTS");

            foreach (object component in components)
            {
                if (component == null) continue;
                Type type = component.GetType();
                string name = type.Name;
                bool priority = player || Interesting(name);
                if (!priority) continue;
                List<string> values = new List<string>();
                foreach (FieldInfo field in type.GetFields(Members))
                {
                    if (field.IsStatic || !Simple(field.FieldType)) continue;
                    try { values.Add(field.Name + "=" + Short(field.GetValue(component))); }
                    catch { }
                    if (values.Count >= 20) break;
                }
                foreach (PropertyInfo prop in type.GetProperties(Members))
                {
                    if (values.Count >= 25) break;
                    if (!prop.CanRead || prop.GetIndexParameters().Length != 0 || !Simple(prop.PropertyType)) continue;
                    try { values.Add(prop.Name + "=" + Short(prop.GetValue(component, null))); }
                    catch { }
                }
                values.Sort(StringComparer.Ordinal);
                EmitChange("component:" + id + ":" + name,
                    values.Count == 0 ? "(no scalar members)" : string.Join("; ", values.ToArray()),
                    label + " | " + name);
            }
        }

        private void EmitChange(string key, string value, string label)
        {
            string before;
            if (prior.TryGetValue(key, out before) && before == value) return;
            prior[key] = value;
            Plugin.Log.LogWarning("CONTROL2 | " + label + " | " + value);
        }

        private static bool Interesting(string name)
        {
            string n = name.ToLowerInvariant();
            return n.Contains("attack") || n.Contains("target") || n.Contains("aggro") ||
                   n.Contains("look") || n.Contains("face") || n.Contains("stun") ||
                   n.Contains("move") || n.Contains("control") || n.Contains("input") ||
                   n.Contains("focus") || n.Contains("lock") || n.Contains("cutscene") ||
                   n.Contains("state") || n.Contains("ai") || n.Contains("ship");
        }

        private static bool Simple(Type t)
        {
            return t.IsPrimitive || t.IsEnum || t == typeof(string) ||
                   t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Quaternion);
        }

        private static string Short(object value)
        {
            string s = value == null ? "null" : value.ToString();
            return s.Length <= 110 ? s : s.Substring(0, 110);
        }

        private static object Read(object source, string name)
        {
            for (Type t = source.GetType(); t != null; t = t.BaseType)
            {
                FieldInfo f = t.GetField(name, Members | BindingFlags.DeclaredOnly);
                if (f != null) { try { return f.GetValue(source); } catch { } }
                PropertyInfo p = t.GetProperty(name, Members | BindingFlags.DeclaredOnly);
                if (p != null && p.GetIndexParameters().Length == 0)
                {
                    try { return p.GetValue(source, null); } catch { }
                }
            }
            return null;
        }
    }
}
