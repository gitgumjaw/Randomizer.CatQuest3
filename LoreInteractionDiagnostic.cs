// LoreInteractionDiagnostic_20261023_2045_EDT.cs
// Targeted, read-only PlayMaker message and character-dialogue diagnostic.
// Replaces the entire previous LoreInteractionDiagnostic.cs.
// Keep ONE line in Plugin.Awake(), immediately after harmony.PatchAll():
//     LoreDiagnosticController.Initialize();
// Press F8 to capture for 20 seconds; press again to stop.
using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    internal sealed class LoreDiagnosticController : MonoBehaviour
    {
        internal static bool Capturing { get; private set; }
        private static float stopAt;
        private static int messageCount;
        private static int dialogueCount;

        internal static void Initialize()
        {
            if (GameObject.Find("Randomizer_LoreDiagnostic") != null) return;
            GameObject go = new GameObject("Randomizer_LoreDiagnostic");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.AddComponent<LoreDiagnosticController>();
        }

        private void Awake()
        {
            Plugin.Log.LogInfo("LORE DIAG | TARGETED READY | ShowMessagePanel.OnEnter + AddDialogueFocus.OnEnter | F8 toggles capture (20 seconds).");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F8))
            {
                if (Capturing) Stop();
                else Start();
            }
            if (Capturing && Time.unscaledTime >= stopAt) Stop();
        }

        private static void Start()
        {
            messageCount = 0;
            dialogueCount = 0;
            stopAt = Time.unscaledTime + 20f;
            Capturing = true;
            Plugin.Log.LogInfo("LORE DIAG | === TARGETED CAPTURE START === | scene=" + SceneManager.GetActiveScene().name);
        }

        private static void Stop()
        {
            Capturing = false;
            Plugin.Log.LogInfo("LORE DIAG | === TARGETED CAPTURE END === | messages=" + messageCount);
        }

        internal static void Record(ShowMessagePanel action)
        {
            if (!Capturing || action == null) return;
            try
            {
                messageCount++;
                Plugin.Log.LogInfo("LORE DIAG | MESSAGE #" + messageCount +
                    " | uniqueID=" + action.uniqueID +
                    " | text=" + Clean(action.displayInfo.contentMessage.content) +
                    " | translateID=" + Clean(action.displayInfo.contentMessage.translateID));

                object fsm = action.Fsm;
                Plugin.Log.LogInfo("LORE DIAG | FSM | " + Describe(fsm));
                LogMember(fsm, "GameObject");
                LogMember(fsm, "Owner");
                LogMember(fsm, "FsmComponent");
                LogMember(fsm, "Name");
                LogMember(fsm, "ActiveStateName");
                LogMember(fsm, "ActiveState");
                LogMember(action, "State");
                LogMember(action, "Owner");
                LogMember(action, "FsmOwner");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("LORE DIAG | Message inspection failed: " + ex);
            }
        }

        internal static void Record(AddDialogueFocus action)
        {
            if (!Capturing || action == null) return;
            try
            {
                dialogueCount++;
                Plugin.Log.LogInfo("LORE DIAG | DIALOGUE #" + dialogueCount +
                    " | action=AddDialogueFocus" +
                    " | dontRaiseCutsceneFlag=" + action.dontRaiseCutsceneFlag +
                    " | choices=" + (!action.HasNoDialogueChoices()));

                object fsm = action.Fsm;
                Plugin.Log.LogInfo("LORE DIAG | DIALOGUE FSM | " + Describe(fsm));
                LogMember(fsm, "GameObject");
                LogMember(fsm, "Owner");
                LogMember(fsm, "Name");
                LogMember(fsm, "ActiveStateName");
                LogMember(action, "Owner");

                // BaseDialogueAction owns dialogueInfo; reflection avoids assuming
                // its field accessibility or serialization details.
                object info = ReadMember(action, "dialogueInfo");
                if (info == null)
                {
                    Plugin.Log.LogInfo("LORE DIAG | DIALOGUE | dialogueInfo unavailable");
                    return;
                }
                LogMember(info, "speakerName");
                LogMember(info, "speakerGO");
                object table = ReadMember(info, "contentTable");
                Array lines = table as Array;
                Plugin.Log.LogInfo("LORE DIAG | DIALOGUE | contentTable length=" +
                    (lines == null ? "unavailable" : lines.Length.ToString()));
                if (lines != null)
                {
                    for (int i = 0; i < lines.Length && i < 8; i++)
                    {
                        object line = lines.GetValue(i);
                        Plugin.Log.LogInfo("LORE DIAG | DIALOGUE LINE #" + (i + 1) +
                            " | content=" + Clean(ReadMember(line, "content") as string) +
                            " | translateID=" + Clean(ReadMember(line, "translateID") as string));
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("LORE DIAG | Dialogue inspection failed: " + ex);
            }
        }

        private static object ReadMember(object obj, string name)
        {
            if (obj == null) return null;
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type type = obj.GetType();
            while (type != null)
            {
                FieldInfo f = type.GetField(name, flags | BindingFlags.DeclaredOnly);
                if (f != null) return f.GetValue(obj);
                PropertyInfo p = type.GetProperty(name, flags | BindingFlags.DeclaredOnly);
                if (p != null && p.GetIndexParameters().Length == 0) return p.GetValue(obj, null);
                type = type.BaseType;
            }
            return null;
        }

        private static void LogMember(object obj, string memberName)
        {
            if (obj == null) return;
            try
            {
                Type t = obj.GetType();
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                PropertyInfo p = t.GetProperty(memberName, flags);
                FieldInfo f = t.GetField(memberName, flags);
                if (p == null && f == null) return;
                object value = p != null ? p.GetValue(obj, null) : f.GetValue(obj);
                Plugin.Log.LogInfo("LORE DIAG | " + t.Name + "." + memberName + " | " + Describe(value));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogInfo("LORE DIAG | " + memberName + " unavailable | " + ex.GetType().Name);
            }
        }

        private static string Describe(object obj)
        {
            if (obj == null) return "<null>";
            GameObject go = obj as GameObject;
            Component component = obj as Component;
            if (component != null) go = component.gameObject;
            if (go != null)
                return "scene=" + go.scene.name + " | path=" + PathOf(go.transform) + " | type=" + obj.GetType().FullName;
            return "type=" + obj.GetType().FullName + " | value=" + Clean(obj.ToString());
        }

        private static string PathOf(Transform transform)
        {
            string result = transform.name;
            int depth = 0;
            while (transform.parent != null && depth++ < 24)
            {
                transform = transform.parent;
                result = transform.name + "/" + result;
            }
            return result;
        }

        private static string Clean(string s)
        {
            if (s == null) return "<null>";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= 1000 ? s : s.Substring(0, 1000) + "...[truncated]";
        }
    }

    [HarmonyPatch(typeof(AddDialogueFocus), "OnEnter")]
    internal static class LoreAddDialogueFocusDiagnosticPatch
    {
        [HarmonyPrefix]
        private static void Prefix(AddDialogueFocus __instance)
        {
            LoreDiagnosticController.Record(__instance);
        }
    }

    [HarmonyPatch(typeof(ShowMessagePanel), "OnEnter")]
    internal static class LoreShowMessagePanelDiagnosticPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ShowMessagePanel __instance)
        {
            LoreDiagnosticController.Record(__instance);
        }
    }
}
