using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(ChestBehaviour), "SpawnLoot")]
    public static class ResourceChestPatch
    {
        private static readonly HashSet<string> registeredChests =
            new HashSet<string>();

        public static void Prefix(
            ChestID ___chestID,
            LootTable ___currentTable)
        {
            if (___chestID == null ||
                ___currentTable?.list == null)
            {
                return;
            }

            string locationKey = ___chestID.Guid;

            if (registeredChests.Contains(locationKey))
            {
                return;
            }

            List<Reward> rewards = new List<Reward>();

            foreach (LootTableItem entry in ___currentTable.list)
            {
                if (entry.dropType != LootTableItem.DropType.Collectible)
                {
                    continue;
                }

                LootTableItem collectibleEntry = entry;

                // This mirrors the game's own collectible resolution.
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

            registeredChests.Add(locationKey);

            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    rewards
                )
            );
        }
    }
}