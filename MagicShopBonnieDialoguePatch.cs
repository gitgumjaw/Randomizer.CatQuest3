using HarmonyLib;

namespace Randomizer.CatQuest3
{
    // Suppress vanilla "maxed spell" shop chatter when the selected
    // spell still has a randomized lockbox available.
    [HarmonyPatch(
        typeof(EquipmentUpgradeShopDialogueReactReactiveHandler),
        "ShowSelectionConfirmationFeedback"
    )]
    public static class MagicShopBonnieDialoguePatch
    {
        public static bool Prefix(UIButton targetUIButton)
        {
            if (!RandomizerRuntime.IsEnabled ||
                RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeSpells ||
                targetUIButton == null)
            {
                return true;
            }

            MagicUpgradeGridCell cell =
                targetUIButton.GetComponent<MagicUpgradeGridCell>();

            if (cell == null ||
                cell.cachedSpell == null ||
                cell.cachedSpell.spellConfig == null)
            {
                return true;
            }

            Spell spell = cell.cachedSpell;

            if (!spell.level.valueIsMaxed ||
                MagicShopProgress.GetNextTier(spell.spellConfig.Guid) == 0)
            {
                return true;
            }

            // Vanilla sees a maxed spell, but the randomizer offers
            // another lockbox. Do not show misleading feedback.
            Plugin.Log.LogInfo(
                "MAGIC SHOP | Suppressed max-level Bonnie dialogue for " +
                spell.GetItemName()
            );

            return false;
        }
    }
}
