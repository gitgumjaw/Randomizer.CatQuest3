using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RandomizerGenerator
    {
        public static void Generate(
            RandomizerSettings settings,
            int seed)
        {
            List<RewardSlot> eligibleSlots =
                RewardCatalog.GetEligibleSlots(settings);

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