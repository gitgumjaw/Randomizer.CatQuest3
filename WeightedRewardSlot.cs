using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class WeightedRewardSlot
    {
        public List<WeightedRewardOption> Options { get; }

        // Most slots can receive any enabled reward type.
        // Specific locations can disable collectible rewards
        // when ground-spawned pickups would be unsafe.
        public bool AllowCollectibles { get; }

        public WeightedRewardSlot(
            IEnumerable<WeightedRewardOption> options,
            bool allowCollectibles = true)
        {
            Options = new List<WeightedRewardOption>(options);
            AllowCollectibles = allowCollectibles;
        }
    }
}