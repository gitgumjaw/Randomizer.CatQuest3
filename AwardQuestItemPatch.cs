using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardQuestItem), "OnEnter")]
    public static class AwardQuestItemPatch
    {
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

            Plugin.Log.LogInfo(
                $"Location: {location.Key} | " +
                $"Vanilla Reward: {location.VanillaReward.Type} " +
                $"{location.VanillaReward.Id}"
            );
        }
    }
}