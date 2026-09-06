using HarmonyLib;
using ProjectStar.Data;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class CatalogScanner
    {
        public static void ScanLoadedChests()
        {
            ChestBehaviour[] chests =
                Object.FindObjectsOfType<ChestBehaviour>(true);

            Plugin.Log.LogInfo(
                $"Catalog Scanner found {chests.Length} loaded chests."
            );

            foreach (ChestBehaviour chest in chests)
            {
                ScanChest(chest);
            }
        }

        public static void ScanChest(ChestBehaviour chest)
        {
            if (chest == null)
            {
                return;
            }

            Traverse traverse = Traverse.Create(chest);

            ChestID chestID =
                traverse
                    .Field("chestID")
                    .GetValue<ChestID>();

            EquipmentLootTableItem[] itemsDrops =
                traverse
                    .Field("itemsDrops")
                    .GetValue<EquipmentLootTableItem[]>();

            LootTable currentTable =
                traverse
                    .Field("currentTable")
                    .GetValue<LootTable>();

            if (chestID == null)
            {
                return;
            }

            // Always rebuild a fresh version from the chest's
            // CURRENT state so dynamically populated chests can
            // be rescanned later.
            CatalogRewardLocation scannedLocation =
                new CatalogRewardLocation(chestID.Guid);

            // Equipment / Blueprint slot
            if (itemsDrops != null && itemsDrops.Length > 0)
            {
                var options =
                    new System.Collections.Generic.List<WeightedRewardOption>();

                foreach (EquipmentLootTableItem entry in itemsDrops)
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
                    scannedLocation.AddSlot(
                        new WeightedRewardSlot(options)
                    );
                }
            }

            // LootTable slots
            if (currentTable?.list != null)
            {
                foreach (LootTableItem entry in currentTable.list)
                {
                    // Collectibles / resources
                    if (entry.dropType == LootTableItem.DropType.Collectible)
                    {
                        LootTableItem collectibleEntry = entry;

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

                        CollectibleRewardData collectibleData =
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

                        Reward collectibleReward =
                            new Reward(
                                RewardType.Collectible,
                                collectibleEntry.prefab.name,
                                collectibleData
                            );

                        scannedLocation.AddSlot(
                            new WeightedRewardSlot(
                                new[]
                                {
                                    new WeightedRewardOption(
                                        collectibleReward,
                                        1
                                    )
                                }
                            )
                        );
                    }

                    // Quest Items
                    else if (entry.dropType == LootTableItem.DropType.QuestItem)
                    {
                        if (entry.dropQuestItem == null)
                        {
                            continue;
                        }

                        Reward questItemReward =
                            new Reward(
                                RewardType.QuestItem,
                                entry.dropQuestItem.Guid
                            );

                        scannedLocation.AddSlot(
                            new WeightedRewardSlot(
                                new[]
                                {
                                    new WeightedRewardOption(
                                        questItemReward,
                                        1
                                    )
                                }
                            )
                        );
                    }

                    else
                    {
                        Plugin.Log.LogInfo(
                            $"  Unprocessed Loot Entry: {entry.dropType}"
                        );
                    }
                }
            }

            CatalogRewardLocation existingLocation =
                CatalogScanResults.Get(chestID.Guid);

            // Never let an earlier/partial state overwrite a version
            // that already contains more complete reward information.
            if (existingLocation != null &&
                scannedLocation.RewardSlots.Count <
                existingLocation.RewardSlots.Count)
            {
                Plugin.Log.LogInfo(
                    $"Chest rescan ignored: {chestID.Guid} | " +
                    $"Existing Slots: {existingLocation.RewardSlots.Count} | " +
                    $"Scanned Slots: {scannedLocation.RewardSlots.Count}"
                );

                return;
            }

            CatalogScanResults.Set(scannedLocation);

            string scanType =
                existingLocation == null
                    ? "Scanned Chest"
                    : "Rescanned Chest";

            Plugin.Log.LogInfo(
                $"{scanType}: {chestID.Guid} | " +
                $"Reward Slots: {scannedLocation.RewardSlots.Count}"
            );

            for (int i = 0; i < scannedLocation.RewardSlots.Count; i++)
            {
                WeightedRewardSlot slot =
                    scannedLocation.RewardSlots[i];

                Plugin.Log.LogInfo(
                    $"  Slot {i}: {slot.Options.Count} option(s)"
                );

                foreach (WeightedRewardOption option in slot.Options)
                {
                    Plugin.Log.LogInfo(
                        $"    {option.Reward.Type} " +
                        $"{option.Reward.Id} | " +
                        $"Weight: {option.Weight}"
                    );
                }
            }

            Plugin.Log.LogInfo(
                $"Catalog Locations: {CatalogScanResults.Count}"
            );
        }
    }
}