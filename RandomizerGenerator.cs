using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RandomizerGenerator
    {
        public static void Generate(
            RandomizerSettings settings,
            int seed)
        {
            // Start every generation from a clean resolved catalog.
            RewardCatalog.Clear();

            // Resolve vanilla weighted/random slots into one fixed
            // reward per slot for this seed.
            CatalogResolver.ResolveToRewardCatalog(seed);

            // Only enabled rewards enter the global shuffle pool.
            List<RewardSlot> eligibleSlots =
                RewardCatalog.GetEligibleSlots(settings);

            // Shuffle while respecting destination compatibility
            // rules such as restricted locations rejecting Collectibles.
            RewardShuffler.Shuffle(
                eligibleSlots,
                seed
            );

            Plugin.Log.LogInfo(
                $"Generated randomizer with " +
                $"{eligibleSlots.Count} eligible reward slots."
            );
        }
    }
}