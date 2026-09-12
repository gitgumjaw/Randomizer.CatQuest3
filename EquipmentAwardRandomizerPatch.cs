using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardEquipmentToPlayer), "Award")]
    public static class EquipmentAwardRandomizerPatch
    {
        public static bool Prefix(
            AwardEquipmentToPlayer __instance)
        {
            string key =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );

            RewardLocation location =
                RewardCatalog.Get(key);

            if (location == null ||
                __instance.equipmentItem == null)
            {
                return true;
            }

            string equipmentGuid =
                __instance.equipmentItem.Guid;

            int rewardIndex =
                location.FindVanillaRewardIndex(
                    RewardType.Equipment,
                    equipmentGuid
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

            // If this slot still contains its vanilla reward,
            // let the game's original action handle it normally.
            if (randomizedReward.Type == RewardType.Equipment &&
                randomizedReward.Id == equipmentGuid)
            {
                return true;
            }

            // We only intercept reward types that RewardGranter
            // currently knows how to handle.
            if (randomizedReward.Type != RewardType.Equipment &&
                randomizedReward.Type != RewardType.Blueprint &&
                randomizedReward.Type != RewardType.Spell &&
                randomizedReward.Type != RewardType.ShipSpell &&
                randomizedReward.Type != RewardType.QuestItem &&
                randomizedReward.Type != RewardType.ManaCrystal &&
                randomizedReward.Type != RewardType.Collectible)
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

            int vanillaAwardLevel =
                Mathf.CeilToInt(
                    __instance.itemLevel +
                    GameplayHelper.GetRewardBonusLevels() *
                    AddressableSingletonScriptableObject<GameConfig>
                        .Instance
                        .lootDropConfig
                        .ItemDropFromChestMultiplier
                );

            Vector3 position =
                __instance.Fsm.GameObject.transform.position;

            Plugin.Log.LogInfo(
                $"Replacing equipment reward | " +
                $"Location:{location.Label} | " +
                $"Slot:{rewardIndex} | " +
                $"Vanilla:{equipmentGuid} | " +
                $"Randomized:{randomizedReward.Type}:" +
                $"{randomizedReward.Id}"
            );

            RewardGranter.Grant(
                randomizedReward,
                vanillaAwardLevel,
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