using HarmonyLib;

namespace Randomizer.CatQuest3
{
    // Keep Tavern Tales completion and companion ownership separate.
    // Only applies while the Tavern Tales Parrots randomizer option is enabled.
    internal static class TavernTalesCompanionAccess
    {
        internal static bool Enabled
        {
            get
            {
                RandomizerRunConfiguration configuration =
                    RandomizerRuntime.CurrentConfiguration;

                return configuration != null &&
                    configuration.Enabled &&
                    configuration.TavernTalesParrots;
            }
        }
    }

    [HarmonyPatch(typeof(GameplayHelper), "IsCompanionUnlocked")]
    internal static class TavernTalesCompanionOwnershipPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(
            CompanionDataAsset companionDataAsset,
            ref int companionStarLevel,
            ref bool __result)
        {
            if (!TavernTalesCompanionAccess.Enabled)
                return true; // Vanilla behavior.

            // GetCompanionStarLevel reads the actual unlocked-level save data;
            // unlike IsCompanionUnlocked, it does not fall back to Tavern Tale clears.
            companionStarLevel = companionDataAsset != null
                ? GameplayHelper.GetCompanionStarLevel(companionDataAsset)
                : -1;

            __result = companionStarLevel > 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(TavernTalesCompanionSelectionValidateCondition),
        "ValidateImplementation")]
    internal static class TavernTalesCompanionMenuAccessPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            if (!TavernTalesCompanionAccess.Enabled)
                return true; // Vanilla behavior.

            // Open the Crewmates menu even before the first companion is owned.
            // The grid controller's original ownership validation still handles
            // equip attempts and the usual locked-slot rejection feedback.
            __result = true;
            return false;
        }
    }
}
