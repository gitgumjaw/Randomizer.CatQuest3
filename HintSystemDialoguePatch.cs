
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // HintSystemPopupProof.cs
    //
    // Catalog-driven dialogue-first spoiler patch.
    //
    // Supports:
    // TRIGGER|Dialogue|StateName
    // TRIGGER|Dialogue|*
    //
    // Wildcard triggers only intercept the first
    // AddDialogueFocus action in the current FSM state.

    [HarmonyPatch(
        typeof(AddDialogueFocus),
        "StartDialogue")]
    internal static class HintSystemDialoguePatch
    {

        [HarmonyPrefix]
        private static bool Prefix(
            AddDialogueFocus __instance)
        {
            if (HintSystemPopupController.IsResumingVanilla ||
                HintSystemPopupController.IsPopupActive)
                return true;

            if (!RandomizerRuntime.IsEnabled)
                return true;

            if (__instance == null ||
                __instance.Fsm == null ||
                __instance.Fsm.GameObject == null ||
                __instance.Fsm.ActiveState == null)
                return true;

            GameObject owner =
                __instance.Fsm.GameObject;

            string key =
                owner.scene.name + "|" +
                GetHierarchyPath(owner.transform);

            string state =
                __instance.Fsm.ActiveStateName;

            // First try an exact-state catalog trigger.
            // This preserves all our previously tested
            // dialogue-first sources.

            HintSystemSource source =
                HintSystemCatalog.Get(
                    key,
                    "Dialogue",
                    state);

            // If no exact match exists, look for a
            // wildcard trigger registered for this source.

            if (source == null)
            {
                source = HintSystemCatalog.Get(
                    key,
                    "Dialogue",
                    "*");

                if (source == null)
                    return true;

                // Wildcard sources should only show a
                // spoiler before the first dialogue action
                // in the currently active FSM state.

                if (!IsFirstDialogueAction(__instance))
                    return true;
            }

            Plugin.Log.LogInfo(
                "HINT SYSTEM | Dialogue source matched: " +
                source.Key +
                " | State: " + state);

            string hintText =
                HintSystemAssignments.GetText(source.Key);

            bool intercepted =
                HintSystemPopupController.TryShowBeforeDialogue(
                    __instance,
                    hintText);


            return !intercepted;
        }

        private static bool IsFirstDialogueAction(
            AddDialogueFocus dialogueAction)
        {
            FsmState state =
                dialogueAction.Fsm.ActiveState;

            if (state == null || state.Actions == null)
                return false;

            foreach (FsmStateAction action in state.Actions)
            {
                if (action is AddDialogueFocus)
                {
                    return object.ReferenceEquals(
                        action,
                        dialogueAction);
                }
            }

            return false;
        }

        private static string GetHierarchyPath(
            Transform transform)
        {
            string path = transform.name;

            while (transform.parent != null)
            {
                transform = transform.parent;

                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
