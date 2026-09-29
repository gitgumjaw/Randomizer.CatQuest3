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
            ref bool ___dropItem,
            ref KeyData ___keyDrop,
            ref bool ___hasQuestItem,
            ref bool ___opened)
        {
            if (!CanRandomizeChest(
                    "SpawnLoot",
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


            // Vanilla SpawnLoot() prepares these fields before
            // RandomLootItem() / OnPostChestAwardedUI().
            //
            // We suppress vanilla loot spawning, so preserve the
            // non-randomized chest state ourselves.
            ___hasQuestItem = false;


            foreach (LootTableItem item
                     in ___currentTable.list)
            {
                if (item.dropType ==
                    LootTableItem.DropType.Key)
                {
                    ___keyDrop =
                        item.dropKeyOnOpen;

                    Plugin.Log.LogInfo(
                        $"Preserving chest key | " +
                        $"Location:{location.Label} | " +
                        $"Key:{___keyDrop}"
                    );
                }


                if (item.dropType ==
                    LootTableItem.DropType.QuestItem)
                {
                    ___hasQuestItem = true;
                }
            }


            // Vanilla SpawnLoot() finishes by marking the
            // chest as opened.
            ___opened = true;


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
                    "RandomLootItem",
                    ___chestID,
                    ___currentTable,
                    ___itemLootToBeSpawned,
                    ___chestType,
                    out RewardLocation location))
            {
                Plugin.Log.LogWarning(
                    $"CHEST RANDOMIZER FALLBACK | " +
                    $"Stage:RandomLootItem | " +
                    $"Reason:PreviouslyAcceptedChestFailedSecondValidation | " +
                    $"Guid:{GetChestGuid(___chestID)}"
                );

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
            string stage,
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
                LogFallback(
                    stage,
                    "BagChest",
                    chestID,
                    null
                );

                return false;
            }


            if (chestID == null)
            {
                LogFallback(
                    stage,
                    "ChestIDNull",
                    null,
                    null
                );

                return false;
            }


            if (currentTable == null)
            {
                LogFallback(
                    stage,
                    "CurrentTableNull",
                    chestID,
                    null
                );

                return false;
            }


            location =
                RewardCatalog.Get(
                    chestID.Guid
                );


            if (location == null)
            {
                LogFallback(
                    stage,
                    "LocationNotFound",
                    chestID,
                    null
                );

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
                    RewardType.Blueprint ||
                    reward.Type ==
                    RewardType.QuestItem)
                {
                    continue;
                }


                LogFallback(
                    stage,
                    "UnsupportedCatalogRewardType",
                    chestID,
                    location
                );

                return false;
            }


            if (currentTable.list == null)
            {
                LogFallback(
                    stage,
                    "LootTableListNull",
                    chestID,
                    location
                );

                return false;
            }


            int lootCollectibleCount = 0;


            foreach (LootTableItem item
                     in currentTable.list)
            {
                if (item == null)
                {
                    LogFallback(
                        stage,
                        "LootTableItemNull",
                        chestID,
                        location
                    );

                    return false;
                }


                if (item.dropType ==
                    LootTableItem.DropType.Collectible)
                {
                    lootCollectibleCount++;

                    continue;
                }


                if (item.dropType ==
                    LootTableItem.DropType.QuestItem)
                {
                    continue;
                }


                // Keys are vanilla chest side effects rather
                // than randomized reward slots.
                //
                // SpawnLootPrefix preserves dropKeyOnOpen in
                // the chest's keyDrop field so the normal
                // OnPostChestAwardedUI() flow can award it.
                if (item.dropType ==
                    LootTableItem.DropType.Key)
                {
                    continue;
                }


                Plugin.Log.LogWarning(
                    $"CHEST RANDOMIZER FALLBACK | " +
                    $"Stage:{stage} | " +
                    $"Reason:UnsupportedLootTableDropType | " +
                    $"Guid:{GetChestGuid(chestID)} | " +
                    $"Location:{location.Label} | " +
                    $"DropType:{item.dropType}"
                );

                return false;
            }


            if (lootCollectibleCount !=
                collectibleCount)
            {
                Plugin.Log.LogWarning(
                    $"CHEST RANDOMIZER FALLBACK | " +
                    $"Stage:{stage} | " +
                    $"Reason:CollectibleShapeMismatch | " +
                    $"Guid:{GetChestGuid(chestID)} | " +
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
                    $"CHEST RANDOMIZER FALLBACK | " +
                    $"Stage:{stage} | " +
                    $"Reason:CouldNotMapChestItemSlot | " +
                    $"Guid:{GetChestGuid(chestID)} | " +
                    $"Location:{location.Label} | " +
                    $"Item:{itemLoot.Guid}"
                );

                return false;
            }


            return true;
        }


        private static void LogFallback(
            string stage,
            string reason,
            ChestID chestID,
            RewardLocation location)
        {
            string locationName =
                location != null
                    ? location.Label
                    : "Unknown";


            Plugin.Log.LogWarning(
                $"CHEST RANDOMIZER FALLBACK | " +
                $"Stage:{stage} | " +
                $"Reason:{reason} | " +
                $"Guid:{GetChestGuid(chestID)} | " +
                $"Location:{locationName}"
            );
        }


        private static string GetChestGuid(
            ChestID chestID)
        {
            if (chestID == null)
            {
                return "Unknown";
            }


            if (string.IsNullOrEmpty(
                    chestID.Guid))
            {
                return "Unknown";
            }


            return chestID.Guid;
        }


        private static int FindChestItemSlot(
            RewardLocation location,
            EquipmentItemData itemLoot)
        {
            if (location == null ||
                itemLoot == null)
            {
                return -1;
            }


            RewardType type =
                itemLoot is ShipBlueprintItemData
                    ? RewardType.Blueprint
                    : RewardType.Equipment;


            int exactMatch =
                location.FindVanillaRewardIndex(
                    type,
                    itemLoot.Guid
                );


            if (exactMatch >= 0)
            {
                return exactMatch;
            }


            int randomSlotMatch = -1;


            for (int i = 0;
                 i < location.VanillaRewards.Count;
                 i++)
            {
                if (!location.IsVanillaRandomBySlot[i])
                {
                    continue;
                }


                if (randomSlotMatch != -1)
                {
                    Plugin.Log.LogWarning(
                        $"CHEST RANDOMIZER FALLBACK | " +
                        $"Stage:FindChestItemSlot | " +
                        $"Reason:MultipleVanillaRandomSlots | " +
                        $"Location:{location.Label} | " +
                        $"Item:{itemLoot.Guid}"
                    );

                    return -1;
                }


                randomSlotMatch = i;
            }


            return randomSlotMatch;
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