using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardQuestItem), "OnEnter")]
    public static class StarRuneFloatPatch
    {
        public static void Prefix(AwardQuestItem __instance)
        {
            if (__instance.questItem == null)
            {
                return;
            }

            // Temporary early-Float behavior.
            // This will eventually be replaced by the
            // progression/randomizer logic.
            if (__instance.questItem.Guid ==
                SpecialRewards.StarRuneGuid)
            {
                Contexts.sharedInstance.game.isFloatBlocked = false;
            }
        }
    }
}