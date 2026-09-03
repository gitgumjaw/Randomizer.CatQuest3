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
                    continue;
                }

                if (CatalogScanResults.Get(chestID.Guid) != null)
                {
                    continue;
                }

                CatalogRewardLocation catalogLocation =
                    CatalogScanResults.GetOrCreate(chestID.Guid);

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
                        catalogLocation.AddSlot(
                            new WeightedRewardSlot(options)
                        );
                    }
                }

                if (currentTable?.list != null)
                {
                    foreach (LootTableItem entry in currentTable.list)
                    {
                        Plugin.Log.LogInfo(
                            $"  Loot Entry: {entry.dropType}"
                        );
                    }
                }

                Plugin.Log.LogInfo(
                    $"Scanned Chest: {chestID.Guid} | " +
                    $"Reward Slots: {catalogLocation.RewardSlots.Count} | " +
                    $"Catalog Locations: {CatalogScanResults.Count}"
                );
            }
        }
    }
}