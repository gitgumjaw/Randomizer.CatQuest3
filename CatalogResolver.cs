using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class CatalogResolver
    {
        public static void ResolveToRewardCatalog(
            int seed)
        {
            List<CatalogRewardLocation>
                catalogLocations =
                    CatalogLoader.Load();

            // Dictionary enumeration order should not
            // determine a seed.
            // Always resolve locations in a stable order.
            catalogLocations.Sort(
                (a, b) =>
                    string.CompareOrdinal(
                        a.Key,
                        b.Key
                    )
            );

            Random random =
                new Random(seed);

            int vanillaRandomSlotCount = 0;


            foreach (
                CatalogRewardLocation catalogLocation
                in catalogLocations)
            {
                List<Reward> resolvedRewards =
                    new List<Reward>();

                List<bool> allowCollectiblesBySlot =
                    new List<bool>();

                List<bool> isVanillaRandomBySlot =
                    new List<bool>();


                foreach (
                    WeightedRewardSlot weightedSlot
                    in catalogLocation.RewardSlots)
                {
                    Reward resolvedReward =
                        ResolveSlot(
                            weightedSlot,
                            random
                        );

                    if (resolvedReward == null)
                    {
                        Plugin.Log.LogError(
                            $"Could not resolve catalog slot at " +
                            $"{catalogLocation.Key}"
                        );

                        continue;
                    }

                    bool isVanillaRandom =
                        weightedSlot.Options != null &&
                        weightedSlot.Options.Count > 1;

                    resolvedRewards.Add(
                        resolvedReward
                    );

                    allowCollectiblesBySlot.Add(
                        weightedSlot.AllowCollectibles
                    );

                    isVanillaRandomBySlot.Add(
                        isVanillaRandom
                    );

                    if (isVanillaRandom)
                    {
                        vanillaRandomSlotCount++;
                    }
                }


                if (resolvedRewards.Count == 0)
                {
                    continue;
                }


                RewardLocation resolvedLocation =
                    new RewardLocation(
                        catalogLocation.Key,
                        resolvedRewards,
                        allowCollectiblesBySlot,
                        catalogLocation.Triggers,
                        catalogLocation.Label,
                        isVanillaRandomBySlot
                    );

                RewardCatalog.Add(
                    resolvedLocation
                );
            }


            Plugin.Log.LogInfo(
                $"Resolved catalog into RewardCatalog | " +
                $"Locations: {RewardCatalog.Count} | " +
                $"Vanilla random slots: " +
                $"{vanillaRandomSlotCount} | " +
                $"Seed: {seed}"
            );
        }


        private static Reward ResolveSlot(
            WeightedRewardSlot slot,
            Random random)
        {
            if (slot == null ||
                slot.Options == null ||
                slot.Options.Count == 0)
            {
                return null;
            }


            // Fixed slot.
            if (slot.Options.Count == 1)
            {
                return slot.Options[0].Reward;
            }


            int totalWeight = 0;

            foreach (
                WeightedRewardOption option
                in slot.Options)
            {
                if (option == null ||
                    option.Reward == null ||
                    option.Weight <= 0)
                {
                    continue;
                }

                totalWeight +=
                    option.Weight;
            }


            if (totalWeight <= 0)
            {
                return null;
            }


            int roll =
                random.Next(
                    totalWeight
                );

            int cumulativeWeight = 0;


            foreach (
                WeightedRewardOption option
                in slot.Options)
            {
                if (option == null ||
                    option.Reward == null ||
                    option.Weight <= 0)
                {
                    continue;
                }

                cumulativeWeight +=
                    option.Weight;

                if (roll < cumulativeWeight)
                {
                    return option.Reward;
                }
            }


            return null;
        }
    }
}