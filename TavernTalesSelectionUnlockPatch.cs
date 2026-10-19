using HarmonyLib;
using UnityEngine.UI;

namespace Randomizer.CatQuest3
{
    // Tavern Tales selection uses the randomizer's independent claim state.
    // Does not alter vanilla progress, fight rewards, or save files.
    [HarmonyPatch(typeof(GameplayHelper), "IsTavernTalesBossUnlocked")]
    internal static class TavernTalesBossUnlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            if (!TavernTalesCompanionAccess.Enabled)
                return true;

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(UITavernTalesDetailsSubPanel), "RefreshContent")]
    internal static class TavernTalesDifficultySelectionPatch
    {
        // Vanilla uses this number to lock higher difficulty buttons.
        // Two unlocks difficulty three without altering actual save records.
        [HarmonyPrefix]
        private static void Prefix(ref int highestExaggerationValue)
        {
            if (TavernTalesCompanionAccess.Enabled)
                highestExaggerationValue = 2;
        }

        // Vanilla's fake completion checks are replaced with independent
        // randomizer claims for this boss and difficulty.
        [HarmonyPostfix]
        private static void Postfix(
            UITavernTalesDetailsSubPanel __instance,
            TavernTalesEncounterDataAsset encounterData)
        {
            if (!TavernTalesCompanionAccess.Enabled ||
                __instance == null || encounterData == null)
                return;

            // These fields are part of the game's decompiled panel class.
            // Reflection avoids relying on their access modifiers.
            var panel = Traverse.Create(__instance);
            int difficulty = panel.Field<int>("difficultyLevel").Value;
            Image completionIcon = panel.Field<Image>("completionIcon").Value;
            if (completionIcon == null)
                return;

            bool claimed = TavernTalesProgress.IsClaimed(
                encounterData.name, difficulty);
            completionIcon.gameObject.SetActive(claimed);
        }
    }
}
