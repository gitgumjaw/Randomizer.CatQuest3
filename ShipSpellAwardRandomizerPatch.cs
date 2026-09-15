using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardShipSpecialAmmoToPlayer), "Award")]
    public static class ShipSpellAwardRandomizerPatch
    {
        public static bool Prefix(
            AwardShipSpecialAmmoToPlayer __instance)
        {
            string key =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );

            RewardLocation location =
                RewardCatalog.Get(key);

            if (location == null ||
                __instance.specialAmmo == null)
            {
                return true;
            }

            string shipSpellGuid =
                __instance.specialAmmo.Guid;

            int rewardIndex =
                location.FindVanillaRewardIndex(
                    RewardType.ShipSpell,
                    shipSpellGuid
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

            if (!__instance.dontRaiseCutsceneFlag)
            {
                Contexts.sharedInstance.game
                    .isInCutscene = true;

                Contexts.sharedInstance.game
                    .cutsceneOwner.value =
                        __instance.Fsm.GameObject;
            }

            Vector3 position =
                RewardPositionResolver.PlayerOrFallback(
                    __instance.Fsm.GameObject
                        .transform.position
                );

            Plugin.Log.LogInfo(
                $"Queueing ship spell reward | " +
                $"Location:{location.Label} | " +
                $"Slot:{rewardIndex} | " +
                $"Vanilla:ShipSpell:{shipSpellGuid} | " +
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