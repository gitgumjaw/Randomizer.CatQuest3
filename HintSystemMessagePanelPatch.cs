
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // HintSystemMessagePatch.cs
    //
    // Handles message-first sources registered in
    // HintSystemCatalog.txt.
    //
    // Supports both:
    //
    // TRIGGER|Message|StateName
    //
    // and:
    //
    // TRIGGER|Message|StateName|MessageUniqueID
    //
    // The second format allows us to identify just the
    // first message in a multi-message interaction.

    [HarmonyPatch(
        typeof(ShowMessagePanel),
        "OnEnter")]
    internal static class HintSystemMessagePanelPatch
    {

        [HarmonyPrefix]
        private static bool Prefix(
            ShowMessagePanel __instance)
        {
            if (HintSystemPopupController.IsResumingVanilla ||
                HintSystemPopupController.IsPopupActive)
                return true;

            if (!RandomizerRuntime.IsEnabled)
                return true;

            if (__instance == null ||
                __instance.Fsm == null ||
                __instance.Fsm.GameObject == null)
                return true;

            GameObject owner =
                __instance.Fsm.GameObject;

            string key =
                owner.scene.name + "|" +
                GetHierarchyPath(owner.transform);

            string state =
                __instance.Fsm.ActiveStateName;

            // First, try matching an exact message ID.
            // This is useful when multiple messages share
            // the same FSM state.
            HintSystemSource source =
                HintSystemCatalog.Get(
                    key,
                    "Message",
                    state + "|" + __instance.uniqueID);

            // If there is no message-specific entry,
            // continue supporting our existing murals,
            // which are registered by FSM state only.
            if (source == null)
            {
                source = HintSystemCatalog.Get(
                    key,
                    "Message",
                    state);
            }

            if (source == null)
                return true;

            Plugin.Log.LogInfo(
                "HINT SYSTEM | Message source matched: " +
                source.Key +
                " | Message ID: " +
                __instance.uniqueID);

            string hintText =
                HintSystemAssignments.GetText(source.Key);

            bool intercepted =
                HintSystemPopupController.TryShowBeforeMessage(
                    __instance,
                    hintText);


            return !intercepted;
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
