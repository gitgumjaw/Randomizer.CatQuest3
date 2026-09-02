using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardQuestItem), "OnEnter")]
    public static class AwardQuestItemPatch
    {
        private const string StarRuneGuid =
            "fd36ba1341aa4d24692cc3eedea20405";

        public static void Prefix(AwardQuestItem __instance)
        {
            if (__instance.questItem == null)
            {
                return;
            }

            RewardLocation location = new RewardLocation(
                PlayMakerLocation.GetKey(__instance.Fsm),
                new Reward(
                    RewardType.QuestItem,
                    __instance.questItem.Guid
                )
            );

            RewardRegistry.Register(location);

            if (__instance.questItem.Guid == StarRuneGuid)
            {
                Contexts.sharedInstance.game.isFloatBlocked = false;

                Plugin.Log.LogInfo(
                    "Float enabled early with Star Rune."
                );
            }
        }
    }
}