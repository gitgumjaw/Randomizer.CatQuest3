using HarmonyLib;
using ProjectStar.Data;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(InfinityTowerSpawnBatchReward), "OnEnter")]
    public static class InfinityTowerFiniteCatalogPatch
    {
        public static void Prefix(
            InfinityTowerSpawnBatchReward __instance)
        {
            // Only include the one-time storyline ascent.
            // The repeatable downward Infinity Tower stays vanilla.
            if (__instance.Fsm.GameObject.name !=
                "InfinityTowerFiniteManager")
            {
                return;
            }

            InfinityTowerWaveBatch currentBatch =
                __instance.spawnerManager.GetCurrentBatch();

            if (currentBatch == null)
            {
                return;
            }

            int wave =
                __instance.spawnerManager.currWave;

            string locationKey =
                $"{PlayMakerLocation.GetKey(__instance.Fsm)}|" +
                $"Wave:{wave}";

            EventRewardDataInstance batchReward =
                currentBatch.batchReward;

            if (batchReward?.data == null)
            {
                return;
            }

            CatalogRewardLocation location =
                new CatalogRewardLocation(locationKey);

            // Weighted Equipment / Blueprint slot.
            if (batchReward.data.equipLootTable != null &&
                batchReward.data.equipLootTable.Length > 0)
            {
                List<WeightedRewardOption> options =
                    new List<WeightedRewardOption>();

                foreach (EquipmentLootTableItem entry
                         in batchReward.data.equipLootTable)
                {
                    if (entry?.item == null)
                    {
                        continue;
                    }

                    RewardType rewardType =
                        entry.item is ShipBlueprintItemData
                            ? RewardType.Blueprint
                            : RewardType.Equipment;

                    Reward reward =
                        new Reward(
                            rewardType,
                            entry.item.Guid
                        );

                    options.Add(
                        new WeightedRewardOption(
                            reward,
                            entry.weight
                        )
                    );
                }

                if (options.Count > 0)
                {
                    location.AddSlot(
                        new WeightedRewardSlot(options)
                    );
                }
            }

            // Collectible slots.
            //
            // Each loot-table entry is its own slot, so the final
            // Wave 8 chest correctly keeps its three separate Gold
            // rewards.
            LootTable lootTable =
                batchReward.data.lootTable;

            if (lootTable?.list != null)
            {
                foreach (LootTableItem entry in lootTable.list)
                {
                    if (entry.dropType !=
                        LootTableItem.DropType.Collectible)
                    {
                        continue;
                    }

                    LootTableItem collectibleEntry =
                        entry;

                    if (entry.useDataFile &&
                        entry.lootTable?.lootTable?.list != null &&
                        entry.lootTable.lootTable.list.Count > 0)
                    {
                        collectibleEntry =
                            entry.lootTable.lootTable.list[0];
                    }

                    if (collectibleEntry.prefab == null)
                    {
                        continue;
                    }

                    CollectibleRewardData data =
                        new CollectibleRewardData(
                            collectibleEntry.prefab.name,
                            collectibleEntry.quantity,
                            collectibleEntry.value,
                            collectibleEntry.randomQuantity,
                            collectibleEntry.quantityMin,
                            collectibleEntry.quantityMax,
                            collectibleEntry.randomValue,
                            collectibleEntry.valueMin,
                            collectibleEntry.valueMax,
                            collectibleEntry.ignoreCollectibleMultiplier
                        );

                    Reward reward =
                        new Reward(
                            RewardType.Collectible,
                            collectibleEntry.prefab.name,
                            data
                        );

                    location.AddSlot(
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
                }
            }

            if (location.RewardSlots.Count == 0)
            {
                return;
            }

            // Set rather than append. This lets the same PlayMaker
            // action execute on multiple runs without duplicating slots,
            // while Wave:3 / Wave:5 / Wave:8 remain separate locations.
            CatalogScanResults.Set(location);

            Plugin.Log.LogInfo(
                $"Cataloged finite Infinity Tower reward: " +
                $"{locationKey} | " +
                $"Slots: {location.RewardSlots.Count}"
            );

            for (int i = 0;
                 i < location.RewardSlots.Count;
                 i++)
            {
                WeightedRewardSlot slot =
                    location.RewardSlots[i];

                Plugin.Log.LogInfo(
                    $"  Tower Slot {i}: " +
                    $"{slot.Options.Count} option(s)"
                );

                foreach (WeightedRewardOption option
                         in slot.Options)
                {
                    Plugin.Log.LogInfo(
                        $"    {option.Reward.Type} " +
                        $"{option.Reward.Id} | " +
                        $"Weight: {option.Weight}"
                    );
                }
            }
        }
    }
}