// Cat Quest III Randomizer
// File: LovebookCaveDiagnostic.cs
// Read-only cave Lovebook FSM / ownership diagnostic.
//
// Install: add this file to the C# project, then add
//     LovebookCaveDiagnostic.Initialize();
// to Plugin.Awake(), after Plugin.Log is initialized.
//
// Automatically watches the three Lovebook cave scenes.
// F6 prints an immediate snapshot of every matching PlayMaker FSM.
// Does not alter PlayMaker states, rewards, quest items, or saves.

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    internal sealed class LovebookCaveDiagnostic : MonoBehaviour
    {
        private const float PollInterval = 0.10f;
        private const string Prefix = "LOVEBOOK CAVE DIAG | ";
        private static LovebookCaveDiagnostic instance;
        private readonly Dictionary<int, string> previousStates =
            new Dictionary<int, string>();
        private readonly HashSet<int> documentedFsms = new HashSet<int>();
        private float nextPoll;
        private int snapshotNumber;

        internal static void Initialize()
        {
            if (instance != null) return;
            GameObject go = new GameObject("Randomizer_LovebookCaveDiagnostic");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<LovebookCaveDiagnostic>();
        }

        private void Awake()
        {
            Plugin.Log.LogInfo(Prefix +
                "Ready. Automatically logs cave Lovebook FSM state changes. " +
                "F6 = full current FSM snapshot and action inventory.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F6))
                Snapshot("F6", true);

            if (Time.unscaledTime < nextPoll) return;
            nextPoll = Time.unscaledTime + PollInterval;
            Snapshot("POLL", false);
        }

        private static bool IsCaveScene(string scene)
        {
            return string.Equals(scene, "Cave_HeartpurreakCave", StringComparison.Ordinal) ||
                   string.Equals(scene, "Cave_FurrlornCave", StringComparison.Ordinal) ||
                   string.Equals(scene, "Cave_LovepurrCave", StringComparison.Ordinal);
        }

        private static bool IsLovebookFsm(PlayMakerFSM component)
        {
            if (component == null || component.gameObject == null ||
                !component.gameObject.scene.IsValid() ||
                !component.gameObject.scene.isLoaded ||
                !IsCaveScene(component.gameObject.scene.name)) return false;

            string name = component.gameObject.name ?? "";
            return name.IndexOf("LoveBook", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Love_Book", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Lovepurr", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Snapshot(string reason, bool full)
        {
            try
            {
                int found = 0;
                if (full)
                {
                    snapshotNumber++;
                    Plugin.Log.LogWarning(Prefix + "===== SNAPSHOT " + snapshotNumber +
                        " ===== | Reason:" + reason + " | ActiveScene:" +
                        SceneManager.GetActiveScene().name);
                    LogOwnership();
                }

                foreach (PlayMakerFSM component in
                    Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
                {
                    if (!IsLovebookFsm(component) || component.Fsm == null)
                        continue;
                    found++;
                    int id = component.GetInstanceID();
                    string state = component.Fsm.ActiveStateName ?? "<no state>";
                    string identity = Identity(component);
                    string old;
                    bool changed = !previousStates.TryGetValue(id, out old) ||
                                   !string.Equals(old, state, StringComparison.Ordinal);
                    if (changed || full)
                    {
                        Plugin.Log.LogWarning(Prefix + "STATE | " + identity +
                            " | " + (changed ? (old ?? "<unseen>") + " -> " + state : state) +
                            " | ActiveGO:" + component.gameObject.activeInHierarchy +
                            " | Enabled:" + component.enabled);
                        LogCurrentStateActions(component);
                        previousStates[id] = state;
                    }
                    if (documentedFsms.Add(id))
                    {
                        Plugin.Log.LogInfo(Prefix + "DISCOVERED | " + identity);
                        LogAllStates(component);
                    }
                }
                if (full)
                    Plugin.Log.LogWarning(Prefix + "SNAPSHOT COMPLETE | FSMs:" + found);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(Prefix + "Snapshot error: " + ex);
            }
        }

        private static string Identity(PlayMakerFSM component)
        {
            return "Scene:" + component.gameObject.scene.name +
                " | Object:" + component.gameObject.name +
                " | FSM:" + component.Fsm.Name +
                " | Instance:" + component.GetInstanceID();
        }

        private static void LogOwnership()
        {
            try
            {
                Plugin.Log.LogWarning(Prefix + "BOOK OWNERSHIP" +
                    " | Book1:" + LovepurrCastleBookState.HasBook(SpecialRewards.LovepurrBook1Guid) +
                    " | Book2:" + LovepurrCastleBookState.HasBook(SpecialRewards.LovepurrBook2Guid) +
                    " | Book3:" + LovepurrCastleBookState.HasBook(SpecialRewards.LovepurrBook3Guid));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(Prefix + "Ownership query failed: " + ex.Message);
            }
        }

        private static void LogCurrentStateActions(PlayMakerFSM component)
        {
            try
            {
                FsmState[] states = component.Fsm.States;
                if (states == null) return;
                string active = component.Fsm.ActiveStateName;
                foreach (FsmState state in states)
                {
                    if (state == null || !string.Equals(state.Name, active, StringComparison.Ordinal))
                        continue;
                    LogActions("ACTIVE ACTION", component, state);
                    break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(Prefix + "Current action read failed: " + ex.Message);
            }
        }

        private static void LogAllStates(PlayMakerFSM component)
        {
            try
            {
                FsmState[] states = component.Fsm.States;
                if (states == null) return;
                Plugin.Log.LogInfo(Prefix + "FSM INVENTORY | " + Identity(component) +
                    " | States:" + states.Length);
                foreach (FsmState state in states)
                {
                    if (state == null) continue;
                    LogActions("STATE INVENTORY", component, state);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(Prefix + "Inventory read failed: " + ex.Message);
            }
        }

        private static void LogActions(string kind, PlayMakerFSM component, FsmState state)
        {
            if (state.Actions == null || state.Actions.Length == 0)
            {
                Plugin.Log.LogInfo(Prefix + kind + " | " + component.gameObject.name +
                    " | State:" + state.Name + " | Actions:0");
                return;
            }
            for (int i = 0; i < state.Actions.Length; i++)
            {
                FsmStateAction action = state.Actions[i];
                if (action == null) continue;
                Plugin.Log.LogInfo(Prefix + kind + " | " + component.gameObject.name +
                    " | State:" + state.Name + " | Action:" + i +
                    " | Type:" + action.GetType().FullName +
                    DescribeRelevantFields(action));
            }
        }

        private static string DescribeRelevantFields(object action)
        {
            try
            {
                List<string> details = new List<string>();
                const BindingFlags flags = BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly;
                for (Type type = action.GetType();
                     type != null && type != typeof(FsmStateAction);
                     type = type.BaseType)
                {
                    foreach (FieldInfo field in type.GetFields(flags))
                    {
                        string name = field.Name.ToLowerInvariant();
                        if (!(name.Contains("key") || name.Contains("quest") ||
                              name.Contains("item") || name.Contains("book") ||
                              name.Contains("event") || name.Contains("compare") ||
                              name.Contains("bool") || name.Contains("result")))
                            continue;
                        if (details.Count >= 12) break;
                        try
                        {
                            object value = field.GetValue(action);
                            string formatted;
                            if (value == null) formatted = "null";
                            else if (value is UnityEngine.Object)
                            {
                                UnityEngine.Object obj = (UnityEngine.Object)value;
                                formatted = obj != null ? obj.name : "null";
                            }
                            else formatted = value.ToString();
                            if (formatted != null && formatted.Length > 140)
                                formatted = formatted.Substring(0, 140) + "...";
                            details.Add(field.Name + "=" + formatted);
                        }
                        catch { /* Some PlayMaker fields have inaccessible values. */ }
                    }
                }
                return details.Count == 0 ? "" : " | Fields:" + string.Join(";", details.ToArray());
            }
            catch { return ""; }
        }

        internal static void LogAwardEnter(AwardQuestItem action)
        {
            if (action == null || action.Fsm == null || action.Fsm.GameObject == null ||
                !IsCaveScene(action.Fsm.GameObject.scene.name)) return;
            Plugin.Log.LogWarning(Prefix + "AWARD QUEST ITEM OnEnter | Key:" +
                PlayMakerLocation.GetKey(action.Fsm) +
                " | QuestItem:" +
                (action.questItem != null ? action.questItem.Guid : "null"));
        }
    }

    // Observes the award before the randomizer intercepts it.
    // Returns void and never changes the original method's execution.
    [HarmonyPatch(typeof(AwardQuestItem), "OnEnter")]
    internal static class LovebookCaveAwardDiagnosticPatch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static void Prefix(AwardQuestItem __instance)
        {
            LovebookCaveDiagnostic.LogAwardEnter(__instance);
        }
    }
}
