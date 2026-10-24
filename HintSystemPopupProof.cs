
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // Temporary catalog-driven test adapter.
    // Currently supports dialogue-first hint sources.

    [HarmonyPatch(
        typeof(AddDialogueFocus),
        "StartDialogue")]
    internal static class HintSystemPopupProof
    {
        private const string SpoilerText =
            "<b>You found a</b>\n" +
            "<b>Secret Spoiler!</b>\n\n" +
            "Tentakey\n" +
            "is in\n" +
            "Eight Bit Castle\n";

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
                __instance.Fsm.GameObject == null)
                return true;

            GameObject owner =
                __instance.Fsm.GameObject;

            string key =
                owner.scene.name + "|" +
                GetHierarchyPath(owner.transform);

            HintSystemSource source =
                HintSystemCatalog.Get(
                    key,
                    "Dialogue",
                    __instance.Fsm.ActiveStateName);

            if (source == null)
                return true;

            Plugin.Log.LogInfo(
                "HINT SYSTEM PROOF | Catalog matched: " +
                source.Key);

            bool intercepted =
                HintSystemPopupController.TryShowBeforeDialogue(
                    __instance,
                    SpoilerText);

            return !intercepted;
        }

        private static string GetHierarchyPath(
            Transform transform)
        {
            string path = transform.name;

            while (transform.parent != null)
            {
                transform = transform.parent;

                path =
                    transform.name + "/" + path;
            }

            return path;
        }
    }
}
