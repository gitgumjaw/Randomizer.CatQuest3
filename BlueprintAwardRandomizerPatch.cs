using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardShipBlueprintToPlayer), "Award")]
    public static class BlueprintAwardRandomizerPatch
    {
        public static bool Prefix(
            AwardShipBlueprintToPlayer __instance)
        {
            string key =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );

            RewardLocation location =
                RewardCatalog.Get(key);

            if (location == null ||
                __instance.shipBlueprintItemData == null)
            {
                return true;
            }

            string blueprintGuid =
                __instance.shipBlueprintItemData.Guid;

            int rewardIndex =
                location.FindVanillaRewardIndex(
                    RewardType.Blueprint,
                    blueprintGuid
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

            // If this slot still contains its original Blueprint,
            // let the game's vanilla action handle it.
            if (randomizedReward.Type == RewardType.Blueprint &&
                randomizedReward.Id == blueprintGuid)
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
                __instance.Fsm.GameObject
                    .transform.position;

            Plugin.Log.LogInfo(
                $"Replacing blueprint reward | " +
                $"Location:{location.Label} | " +
                $"Slot:{rewardIndex} | " +
                $"Vanilla:{blueprintGuid} | " +
                $"Randomized:{randomizedReward.Type}:" +
                $"{randomizedReward.Id}"
            );

            RewardGranter.Grant(
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