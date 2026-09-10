using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardShipBlueprintToPlayer), "OnEnter")]
    public static class AwardShipBlueprintPatch
    {
        private static readonly HashSet<AwardShipBlueprintToPlayer> catalogedActions =
            new HashSet<AwardShipBlueprintToPlayer>();

        public static void Prefix(
            AwardShipBlueprintToPlayer __instance)
        {
            if (__instance.shipBlueprintItemData == null)
            {
                return;
            }

            string locationKey =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );

            Reward reward =
                new Reward(
                    RewardType.Blueprint,
                    __instance.shipBlueprintItemData.Guid
                );

            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    reward
                )
            );

            if (!catalogedActions.Add(__instance))
            {
                return;
            }

            CatalogRewardLocation catalogLocation =
                CatalogScanResults.GetOrCreate(
                    locationKey
                );

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
                $"Cataloged scripted Blueprint location: " +
                $"{locationKey} | " +
                $"Blueprint: {__instance.shipBlueprintItemData.Guid} | " +
                $"Total Slots: {catalogLocation.RewardSlots.Count}"
            );
        }
    }
}