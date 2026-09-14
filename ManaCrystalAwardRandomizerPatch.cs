using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(GivePlayersManaCrystal), "OnEnter")]
    public static class ManaCrystalAwardRandomizerPatch
    {
        public static bool Prefix(
            GivePlayersManaCrystal __instance)
        {
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
                    RewardType.ManaCrystal,
                    ""
                );

            if (rewardIndex < 0)
            {
                return true;
            }

            Reward randomizedReward =
                location.RandomizedRewards[rewardIndex];

            if (randomizedReward == null)
            {
                return true;
            }

            Vector3 position =
                __instance.Fsm.GameObject
                    .transform.position;

            Plugin.Log.LogInfo(
                $"Queueing mana crystal reward | " +
                $"Location:{location.Label} | " +
                $"Slot:{rewardIndex} | " +
                $"Vanilla:ManaCrystal | " +
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