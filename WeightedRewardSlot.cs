using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class WeightedRewardSlot
    {
        public List<WeightedRewardOption> Options { get; }

        public WeightedRewardSlot(
            IEnumerable<WeightedRewardOption> options)
        {
            Options =
                new List<WeightedRewardOption>(options);
        }
    }
}