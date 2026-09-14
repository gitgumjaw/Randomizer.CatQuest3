using HarmonyLib;
using ProjectStar.Data;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class ChestAwardRandomizerPatch
    {
        private static readonly HashSet<int>
            randomizedChestInstances =
                new HashSet<int>();

        private static readonly HashSet<int>
            submittedChestInstances =
                new HashSet<int>();

        private static readonly MethodInfo
            OnPostChestAwardedUIMethod =
                AccessTools.Method(
                    typeof(ChestBehaviour),
                    "OnPostChestAwardedUI"
                );

        private static readonly MethodInfo
            RemoveChestMethod =
                AccessTools.Method(
                    typeof(ChestBehaviour),
                    "RemoveChest"
                );

        [HarmonyPatch(
            typeof(ChestBehaviour),
            "SpawnLoot"
        )]
        [HarmonyPrefix]
        public static bool SpawnLootPrefix(
            ChestBehaviour __instance,
            ChestID ___chestID,
            LootTable ___currentTable,
            EquipmentItemData ___itemLootToBeSpawned,
            ChestData.ChestType ___chestType,
            ref bool ___dropItem)
        {
            if (!CanRandomizeChest(
                    ___chestID,
                    ___currentTable,
                    ___itemLootToBeSpawned,
                    ___chestType,
                    out RewardLocation location))
            {
                return true;
            }

            int instanceId =
                __instance.GetInstanceID();

            randomizedChestInstances.Add(
                instanceId
            );

            // Prevent ChestBehaviour.Update() from
            // removing the chest before RandomLootItem()
            // gets a chance to submit the rewards.
            ___dropItem = true;

            Plugin.Log.LogInfo(
                $"Suppressing vanilla chest loot | " +
                $"Location:{location.Label} | " +
                $"Slots:{location.RandomizedRewards.Count}"
            );

            return false;
        }

        [HarmonyPatch(
            typeof(ChestBehaviour),
            "RandomLootItem"
        )]
        [HarmonyPrefix]
        public static bool RandomLootItemPrefix(
            ChestBehaviour __instance,
            ChestID ___chestID,
            LootTable ___currentTable,
            EquipmentItemData ___itemLootToBeSpawned,
            ChestData.ChestType ___chestType,
            int ___level,
            bool ___overrideItemLevel,
            int ___itemLevel,
            ref bool ___dropItem)
        {
            int instanceId =
                __instance.GetInstanceID();

            if (!randomizedChestInstances.Contains(
                    instanceId))
            {
                return true;
            }

            if (submittedChestInstances.Contains(
                    instanceId))
            {
                return false;
            }

            if (!CanRandomizeChest(
                    ___chestID,
                    ___currentTable,
                    ___itemLootToBeSpawned,
                    ___chestType,
                    out RewardLocation location))
            {
                return true;
            }

            submittedChestInstances.Add(
                instanceId
            );

            ___dropItem = true;

            Vector3 position =
                __instance.transform.position;

            int equipmentSlot =
                FindChestItemSlot(
                    location,
                    ___itemLootToBeSpawned
                );

            int vanillaAwardLevel =
                GetVanillaAwardLevel(
                    ___chestType,
                    ___level,
                    ___overrideItemLevel,
                    ___itemLevel
                );

            int finalSlot =
                location.RandomizedRewards.Count - 1;

            Plugin.Log.LogInfo(
                $"Queueing chest rewards | " +
                $"Location:{location.Label} | " +
                $"Slots:{location.RandomizedRewards.Count}"
            );

            for (int slotIndex = 0;
                 slotIndex <
                    location.RandomizedRewards.Count;
                 slotIndex++)
            {
                Reward randomizedReward =
                    location.RandomizedRewards[
                        slotIndex
                    ];

                int awardLevel =
                    slotIndex == equipmentSlot
                        ? vanillaAwardLevel
                        : -1;

                System.Action completionCallback =
                    slotIndex == finalSlot
                        ? (System.Action)delegate
                        {
                            CompleteChest(
                                __instance,
                                ___chestID,
                                instanceId
                            );
                        }
                : null;

                Plugin.Log.LogInfo(
                    $"Queueing chest reward | " +
                    $"Location:{location.Label} | " +
                    $"Slot:{slotIndex} | " +
                    $"Vanilla:" +
                    $"{location.VanillaRewards[slotIndex].Type}:" +
                    $"{location.VanillaRewards[slotIndex].Id} | " +
                    $"Randomized:{randomizedReward.Type}:" +
                    $"{randomizedReward.Id}"
                );

                RewardGrantQueue.Enqueue(
                    location,
                    slotIndex,
                    randomizedReward,
                    awardLevel,
                    position,
                    completionCallback
                );
            }

            return false;
        }

        private static bool CanRandomizeChest(
            ChestID chestID,
            LootTable currentTable,
            EquipmentItemData itemLoot,
            ChestData.ChestType chestType,
            out RewardLocation location)
        {
            location = null;

            if (chestType ==
                ChestData.ChestType.Bag)
            {
                return false;
            }

            if (chestID == null ||
                currentTable == null)
            {
                return false;
            }

            location =
                RewardCatalog.Get(
                    chestID.Guid
                );

            if (location == null)
            {
                return false;
            }

            int collectibleCount = 0;

            foreach (Reward reward
                     in location.VanillaRewards)
            {
                if (reward.Type ==
                    RewardType.Collectible)
                {
                    collectibleCount++;
                    continue;
                }

                if (reward.Type ==
                        RewardType.Equipment ||
                    reward.Type ==
                        RewardType.Blueprint)
                {
                    continue;
                }

                // QuestItems, Keys, or other special
                // vanilla chest rewards stay untouched
                // until progression handling is ready.
                return false;
            }

            if (currentTable.list == null)
            {
                return false;
            }

            int lootCollectibleCount = 0;

            foreach (LootTableItem item
                     in currentTable.list)
            {
                if (item == null ||
                    item.dropType !=
                        LootTableItem.DropType.Collectible)
                {
                    return false;
                }

                lootCollectibleCount++;
            }

            if (lootCollectibleCount !=
                collectibleCount)
            {
                Plugin.Log.LogWarning(
                    $"Chest collectible shape mismatch | " +
                    $"Location:{location.Label} | " +
                    $"LootCollectibles:{lootCollectibleCount} | " +
                    $"CatalogCollectibles:{collectibleCount}"
                );

                return false;
            }

            if (itemLoot != null &&
                FindChestItemSlot(
                    location,
                    itemLoot
                ) < 0)
            {
                Plugin.Log.LogWarning(
                    $"Could not map chest item slot | " +
                    $"Location:{location.Label} | " +
                    $"Item:{itemLoot.Guid}"
                );

                return false;
            }

            return true;
        }

        private static int FindChestItemSlot(
            RewardLocation location,
            EquipmentItemData itemLoot)
        {
            if (itemLoot == null)
            {
                return -1;
            }

            RewardType type =
                itemLoot is ShipBlueprintItemData
                    ? RewardType.Blueprint
                    : RewardType.Equipment;

            return location.FindVanillaRewardIndex(
                type,
                itemLoot.Guid
            );
        }

        private static int GetVanillaAwardLevel(
            ChestData.ChestType chestType,
            int level,
            bool overrideItemLevel,
            int itemLevel)
        {
            if (overrideItemLevel)
            {
                return itemLevel;
            }

            float multiplier =
                AddressableSingletonScriptableObject<GameConfig>
                    .Instance
                    .lootDropConfig
                    .ItemDropFromChestMultiplier;

            if (chestType ==
                ChestData.ChestType.Bag)
            {
                multiplier =
                    AddressableSingletonScriptableObject<GameConfig>
                        .Instance
                        .lootDropConfig
                        .ItemDropFromEnemyMultiplier;
            }

            return Mathf.CeilToInt(
                level * multiplier
            );
        }

        private static void CompleteChest(
            ChestBehaviour chest,
            ChestID chestID,
            int instanceId)
        {
            randomizedChestInstances.Remove(
                instanceId
            );

            submittedChestInstances.Remove(
                instanceId
            );

            string sceneName =
                chest.gameObject.scene.name;

            Plugin.Log.LogInfo(
                $"Completed randomized chest | " +
                $"Guid:{chestID.Guid}"
            );

            OnPostChestAwardedUIMethod.Invoke(
                chest,
                new object[]
                {
                    chestID,
                    sceneName
                }
            );

            RemoveChestMethod.Invoke(
                chest,
                new object[]
                {
                    null
                }
            );
        }
    }
}