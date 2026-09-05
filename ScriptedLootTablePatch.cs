using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(SpawnLootTable), "OnEnter")]
    public static class ScriptedLootTablePatch
    {
        // Prevent the same PlayMaker action from adding its catalog
        // slots again if the state executes repeatedly.
        //
        // Different SpawnLootTable actions in the same state are still
        // treated separately, even if they award identical collectibles.
        private static readonly HashSet<SpawnLootTable> catalogedActions =
            new HashSet<SpawnLootTable>();

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

            // Existing runtime discovery behavior.
            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    rewards
                )
            );

            // Catalog-building behavior.
            //
            // We only catalog this particular SpawnLootTable action once.
            // If its PlayMaker state runs again, RewardRegistry can still
            // observe it normally, but we won't duplicate catalog slots.
            if (!catalogedActions.Add(__instance))
            {
                return;
            }

            CatalogRewardLocation catalogLocation =
                CatalogScanResults.GetOrCreate(locationKey);

            foreach (Reward reward in rewards)
            {
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
            }

            Plugin.Log.LogInfo(
                $"Cataloged scripted location: {locationKey} | " +
                $"Added Slots: {rewards.Count} | " +
                $"Total Slots: {catalogLocation.RewardSlots.Count}"
            );

            for (int i = 0; i < catalogLocation.RewardSlots.Count; i++)
            {
                WeightedRewardSlot slot =
                    catalogLocation.RewardSlots[i];

                Plugin.Log.LogInfo(
                    $"  Scripted Slot {i}: " +
                    $"{slot.Options.Count} option(s)"
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
        }
    }
}