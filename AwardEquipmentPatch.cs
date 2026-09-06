using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardEquipmentToPlayer), "Award")]
    public static class AwardEquipmentPatch
    {
        private static readonly HashSet<AwardEquipmentToPlayer> catalogedActions =
            new HashSet<AwardEquipmentToPlayer>();

        public static void Prefix(AwardEquipmentToPlayer __instance)
        {
            if (__instance.equipmentItem == null)
            {
                return;
            }

            string locationKey =
                PlayMakerLocation.GetKey(__instance.Fsm);

            RewardType rewardType =
                __instance.equipmentItem is ShipBlueprintItemData
                    ? RewardType.Blueprint
                    : RewardType.Equipment;

            Reward reward =
                new Reward(
                    rewardType,
                    __instance.equipmentItem.Guid
                );

            // Existing runtime discovery behavior.
            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    reward
                )
            );

            // Development catalog behavior.
            if (!catalogedActions.Add(__instance))
            {
                return;
            }

            CatalogRewardLocation catalogLocation =
                CatalogScanResults.GetOrCreate(locationKey);

            catalogLocation.AddSlot(
                new WeightedRewardSlot(
                    new[]
                    {
                        new WeightedRewardOption(
                            reward,
                            1
                        )
                    }
                )
            );

            Plugin.Log.LogInfo(
                $"Cataloged scripted {rewardType} location: " +
                $"{locationKey} | " +
                $"{rewardType}: {__instance.equipmentItem.Guid} | " +
                $"Total Slots: {catalogLocation.RewardSlots.Count}"
            );
        }
    }
}