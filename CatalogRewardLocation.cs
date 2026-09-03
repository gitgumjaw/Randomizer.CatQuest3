using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class CatalogRewardLocation
    {
        public string Key { get; }

        public List<WeightedRewardSlot> RewardSlots { get; }

        public CatalogRewardLocation(
            string key,
            IEnumerable<WeightedRewardSlot> rewardSlots)
        {
            Key = key;

            RewardSlots =
                new List<WeightedRewardSlot>(rewardSlots);
        }

        public CatalogRewardLocation(string key)
        {
            Key = key;

            RewardSlots =
                new List<WeightedRewardSlot>();
        }

        public void AddSlot(WeightedRewardSlot slot)
        {
            RewardSlots.Add(slot);
        }
    }
}