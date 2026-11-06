// Cat Quest III Randomizer
// File: SpawnerSceneScanner.cs
// Version: SpawnerSceneScanner_20261106_v01
// Read-only scanner. F11 = scan all currently loaded scenes.
// Automatic scans after each Unity sceneLoaded event (immediate, +0.5s, +2s).
// Enumerates inactive UnitSpawner components via Resources.FindObjectsOfTypeAll.
// Writes a cumulative TSV into <plugin dll folder>/Data/SpawnerSceneScan.tsv.
// Does not change spawner state, spawn lists, randomization, or aggro.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    internal sealed class SpawnerSceneScanner : MonoBehaviour
    {
        private readonly HashSet<string> written = new HashSet<string>(StringComparer.Ordinal);
        private string outputPath;
        private bool fileReady;
        private int scanNumber;

        internal static void Initialize()
        {
            if (GameObject.Find("Randomizer_SpawnerSceneScanner") != null)
                return;

            GameObject go = new GameObject("Randomizer_SpawnerSceneScanner");
            DontDestroyOnLoad(go);
            go.AddComponent<SpawnerSceneScanner>();
        }

        private void Awake()
        {
            string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            outputPath = Path.Combine(pluginDirectory, "Data", "SpawnerSceneScan.tsv");
            SceneManager.sceneLoaded += OnSceneLoaded;
            Plugin.Log.LogInfo("SPAWNER SCANNER | Ready. F11 = scan all loaded scenes | Output:" + outputPath);
            StartCoroutine(InitialScan());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
                Scan("F11");
        }

        private IEnumerator InitialScan()
        {
            yield return new WaitForSecondsRealtime(1f);
            Scan("INITIAL");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(ScanAfterSceneLoad(scene.name));
        }

        private IEnumerator ScanAfterSceneLoad(string loadedScene)
        {
            // Spawn objects may be registered by game scripts after sceneLoaded.
            Scan("SCENE_LOADED:" + loadedScene);
            yield return new WaitForSecondsRealtime(0.5f);
            Scan("SCENE_LOADED_0.5s:" + loadedScene);
            yield return new WaitForSecondsRealtime(1.5f);
            Scan("SCENE_LOADED_2s:" + loadedScene);
        }

        private void Scan(string reason)
        {
            int enumerated = 0;
            int added = 0;
            int inactive = 0;
            int skipped = 0;
            int errors = 0;
            int thisScan = ++scanNumber;

            try
            {
                EnsureFile();
                UnitSpawner[] all = Resources.FindObjectsOfTypeAll<UnitSpawner>();
                foreach (UnitSpawner spawner in all)
                {
                    if (spawner == null) continue;
                    try
                    {
                        GameObject go = spawner.gameObject;
                        if (go == null) { skipped++; continue; }
                        Scene scene = go.scene;
                        // Resource assets/prefab templates do not belong to a loaded scene.
                        if (!scene.IsValid() || !scene.isLoaded)
                        {
                            skipped++;
                            continue;
                        }

                        enumerated++;
                        if (!go.activeInHierarchy) inactive++;
                        string id = spawner.spawnerId ?? "";
                        if (id.Length == 0) { skipped++; continue; }
                        string sceneName = scene.name ?? "";
                        // Spawner ID should be stable within each scene; a repeated ID in
                        // different scenes is deliberately recorded separately.
                        string key = sceneName + "|" + id;
                        if (written.Contains(key)) continue;

                        Vector3 pos = go.transform.position;
                        Vector3 local = go.transform.localPosition;
                        string vanilla = GetSpawnListSummary(spawner.spawnList);
                        string ruffer = GetOtherSpawnList(spawner, "spawnListRufferOverride");
                        string hierarchy = GetHierarchyPath(go.transform);
                        string line = JoinTsv(new string[]
                        {
                            sceneName, id, go.name,
                            pos.x.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                            pos.y.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                            pos.z.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                            local.x.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                            local.y.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                            local.z.ToString("F3", System.Globalization.CultureInfo.InvariantCulture),
                            go.activeInHierarchy ? "Yes" : "No",
                            go.activeSelf ? "Yes" : "No",
                            spawner.enabled ? "Yes" : "No",
                            spawner.spawnType.ToString(),
                            vanilla, ruffer, hierarchy, reason
                        });
                        File.AppendAllText(outputPath, line + Environment.NewLine, Encoding.UTF8);
                        written.Add(key);
                        added++;
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        if (errors <= 4)
                            Plugin.Log.LogWarning("SPAWNER SCANNER | Entry error: " + ex.Message);
                    }
                }
                Plugin.Log.LogInfo("SPAWNER SCANNER | Scan:" + thisScan +
                    " | Trigger:" + reason + " | Scene:" + SceneManager.GetActiveScene().name +
                    " | Loaded:" + enumerated + " | Inactive:" + inactive +
                    " | New:" + added + " | TotalUnique:" + written.Count +
                    " | Skipped:" + skipped + " | Errors:" + errors);
                if (added > 0)
                    Plugin.Log.LogInfo("SPAWNER SCANNER | Appended " + added + " rows to " + outputPath);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("SPAWNER SCANNER | Scan failed: " + ex);
            }
        }

        private void EnsureFile()
        {
            if (fileReady) return;
            string dir = Path.GetDirectoryName(outputPath);
            Directory.CreateDirectory(dir);
            // A new game launch starts a fresh report, without touching catalogs.
            File.WriteAllText(outputPath,
                "Scene\tSpawnerId\tGameObjectName\tWorldX\tWorldY\tWorldZ\tLocalX\tLocalY\tLocalZ\tActiveInHierarchy\tActiveSelf\tComponentEnabled\tSpawnType\tVanillaConfigs\tRufferConfigs\tHierarchy\tScanTrigger" +
                Environment.NewLine, Encoding.UTF8);
            fileReady = true;
        }

        private static string GetSpawnListSummary(IEnumerable<SpawnTableItem> list)
        {
            if (list == null) return "";
            List<string> values = new List<string>();
            foreach (SpawnTableItem item in list)
            {
                if (item == null || item.spawnConfig == null) continue;
                values.Add(item.spawnConfig.name + " x" + item.spawnCount);
            }
            return string.Join("; ", values.ToArray());
        }

        private static string GetOtherSpawnList(UnitSpawner spawner, string fieldName)
        {
            // Optional field; avoid coupling scanner build to version-specific APIs.
            try
            {
                Type type = spawner.GetType();
                while (type != null)
                {
                    FieldInfo f = type.GetField(fieldName,
                        BindingFlags.Instance | BindingFlags.Public |
                        BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (f != null)
                    {
                        IEnumerable values = f.GetValue(spawner) as IEnumerable;
                        if (values == null) return "";
                        List<string> names = new List<string>();
                        foreach (object element in values)
                        {
                            if (element == null) continue;
                            // Some builds use SpawnTableItem, others wrap configs.
                            SpawnTableItem item = element as SpawnTableItem;
                            if (item != null && item.spawnConfig != null)
                            {
                                names.Add(item.spawnConfig.name + " x" + item.spawnCount);
                                continue;
                            }
                            object config = ReadMember(element, "spawnConfig");
                            UnityEngine.Object obj = config as UnityEngine.Object;
                            if (obj != null) names.Add(obj.name);
                        }
                        return string.Join("; ", names.ToArray());
                    }
                    type = type.BaseType;
                }
            }
            catch (Exception) { /* Ruffer metadata is optional */ }
            return "";
        }

        private static object ReadMember(object source, string name)
        {
            Type t = source.GetType();
            FieldInfo f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null) return f.GetValue(source);
            PropertyInfo p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return p == null ? null : p.GetValue(source, null);
        }

        private static string GetHierarchyPath(Transform transform)
        {
            List<string> names = new List<string>();
            Transform current = transform;
            int guard = 0;
            while (current != null && guard++ < 128)
            {
                names.Add(current.name);
                current = current.parent;
            }
            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        private static string JoinTsv(string[] fields)
        {
            for (int i = 0; i < fields.Length; ++i)
                fields[i] = (fields[i] ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
            return string.Join("\t", fields);
        }
    }
}
