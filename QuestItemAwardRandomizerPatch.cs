using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardQuestItem), "OnEnter")]
    public static class QuestItemAwardRandomizerPatch
    {
        public static bool Prefix(
            AwardQuestItem __instance)
        {
            if (__instance.questItem == null)
            {
                return true;
            }

            string key =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );

            RewardLocation location =
                RewardCatalog.Get(key);

            if (location == null)
            {
                return true;
            }

            int rewardIndex =
                location.FindVanillaRewardIndex(
                    RewardType.QuestItem,
                    __instance.questItem.Guid
                );

            if (rewardIndex < 0)
            {
                return true;
            }

            Reward randomizedReward =
                location.RandomizedRewards[
                    rewardIndex
                ];

            if (randomizedReward == null)
            {
                return true;
            }

            Vector3 position =
                RewardPositionResolver.PlayerOrFallback(
                    __instance.Fsm.GameObject
                        .transform.position
                );

            Plugin.Log.LogInfo(
                $"Queueing quest item reward | " +
                $"Location:{location.Label} | " +
                $"Slot:{rewardIndex} | " +
                $"Vanilla:QuestItem:" +
                $"{__instance.questItem.Guid} | " +
                $"Randomized:{randomizedReward.Type}:" +
                $"{randomizedReward.Id}"
            );

            RewardGrantQueue.Enqueue(
                location,
                rewardIndex,
                randomizedReward,
                -1,
                position,
                delegate
                {
                    __instance.Finish();
                }
            );

            return false;
        }
    }
}