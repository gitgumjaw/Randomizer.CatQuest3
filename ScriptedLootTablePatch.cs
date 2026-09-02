using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(SpawnLootTable), "OnEnter")]
    public static class ScriptedLootTablePatch
    {
        public static void Prefix(SpawnLootTable __instance)
        {
            LootTable lootTable;

            if (__instance.IsUsingLootTableRef())
            {
                lootTable =
                    AddressableSingletonScriptableObject<LootTableReference>
                        .Instance
                        .GetLootTable(__instance.lootTableIdentifier);
            }
            else
            {
                lootTable = __instance.scriptableLootTable.lootTable;
            }

            if (lootTable?.list == null)
            {
                return;
            }

            List<Reward> rewards = new List<Reward>();

            foreach (LootTableItem entry in lootTable.list)
            {
                if (entry.dropType != LootTableItem.DropType.Collectible)
                {
                    continue;
                }

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

                int quantityMultiplier =
    __instance.quantityMultiplier;

                int valueMultiplier =
                    __instance.overrideMultipliers
                        ? __instance.valueMultiplier
                        : 1;

                if (__instance.Level > 1)
                {
                    valueMultiplier *= UnityEngine.Mathf.CeilToInt(
                        __instance.Level *
                        AddressableSingletonScriptableObject<GameConfig>
                            .Instance
                            .lootDropConfig
                            .CollectibleDropMultiplier
                    );
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
                        collectibleEntry.ignoreCollectibleMultiplier,
                        quantityMultiplier,
                        valueMultiplier
                    );

                rewards.Add(
                    new Reward(
                        RewardType.Collectible,
                        collectibleEntry.prefab.name,
                        data
                    )
                );
            }

            if (rewards.Count == 0)
            {
                return;
            }

            string locationKey =
                PlayMakerLocation.GetKey(__instance.Fsm);

            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    rewards
                )
            );
        }
    }
}