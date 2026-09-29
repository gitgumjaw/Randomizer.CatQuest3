using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(SpawnLootTable),
        "OnEnter"
    )]
    public static class
        CollectibleAwardRandomizerPatch
    {
        public static bool Prefix(
            SpawnLootTable __instance)
        {
            string key =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );


            RewardLocation location =
                RewardCatalog.Get(
                    key
                );


            if (location == null)
            {
                Plugin.Log.LogWarning(
                    $"COLLECTIBLE RANDOMIZER FALLBACK | " +
                    $"Reason:LocationNotFound | " +
                    $"Key:{key}"
                );

                return true;
            }


            LootTable lootTable =
                __instance.IsUsingLootTableRef()
                    ? AddressableSingletonScriptableObject<
                        LootTableReference
                    >
                        .Instance
                        .GetLootTable(
                            __instance.lootTableIdentifier
                        )
                    : __instance.scriptableLootTable
                        ?.lootTable;


            if (lootTable == null)
            {
                Plugin.Log.LogWarning(
                    $"COLLECTIBLE RANDOMIZER FALLBACK | " +
                    $"Reason:LootTableNull | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label}"
                );

                return true;
            }


            List<int> collectibleSlots =
                new List<int>();


            for (int i = 0;
                 i < location.VanillaRewards.Count;
                 i++)
            {
                if (location.VanillaRewards[i].Type ==
                    RewardType.Collectible)
                {
                    collectibleSlots.Add(i);
                }
            }


            if (collectibleSlots.Count == 0)
            {
                Plugin.Log.LogWarning(
                    $"COLLECTIBLE RANDOMIZER FALLBACK | " +
                    $"Reason:NoCollectibleSlots | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label}"
                );

                return true;
            }


            if (lootTable.list.Count !=
                collectibleSlots.Count)
            {
                Plugin.Log.LogWarning(
                    $"COLLECTIBLE RANDOMIZER FALLBACK | " +
                    $"Reason:SourceShapeMismatch | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label} | " +
                    $"LootItems:{lootTable.list.Count} | " +
                    $"CatalogCollectibles:" +
                    $"{collectibleSlots.Count}"
                );

                return true;
            }


            Vector3 position =
                __instance.target.Value
                    .transform.position +
                __instance.offset;


            position.y = 0f;


            Plugin.Log.LogInfo(
                $"Queueing collectible source | " +
                $"Location:{location.Label} | " +
                $"Slots:{collectibleSlots.Count}"
            );


            for (int i = 0;
                 i < collectibleSlots.Count;
                 i++)
            {
                int slotIndex =
                    collectibleSlots[i];


                Reward randomizedReward =
                    location.RandomizedRewards[
                        slotIndex
                    ];


                bool isLast =
                    i ==
                    collectibleSlots.Count - 1;


                RewardGrantQueue.Enqueue(
                    location,
                    slotIndex,
                    randomizedReward,
                    -1,
                    position,
                    isLast
                        ? (System.Action)delegate
                        {
                            __instance.Finish();

                            PlayMakerUtils
                                .UpdateFsmThisFrame(
                                    __instance.Fsm
                                );
                        }
                : null
                );


                Plugin.Log.LogInfo(
                    $"Queueing collectible reward | " +
                    $"Location:{location.Label} | " +
                    $"Slot:{slotIndex} | " +
                    $"Vanilla:" +
                    $"{location.VanillaRewards[slotIndex].Type}:" +
                    $"{location.VanillaRewards[slotIndex].Id} | " +
                    $"Randomized:{randomizedReward.Type}:" +
                    $"{randomizedReward.Id}"
                );
            }


            return false;
        }
    }
}