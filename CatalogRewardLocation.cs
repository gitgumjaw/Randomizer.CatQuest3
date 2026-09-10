using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class CatalogRewardLocation
    {
        public string Key { get; }

        public string Label { get; set; }

        public List<string> Triggers { get; }

        public List<WeightedRewardSlot> RewardSlots { get; }

        public CatalogRewardLocation(
            string key,
            IEnumerable<WeightedRewardSlot> rewardSlots)
        {
            Key = key;

            Label = null;

            Triggers =
                new List<string>();

            RewardSlots =
                new List<WeightedRewardSlot>(rewardSlots);
        }

        public CatalogRewardLocation(string key)
        {
            Key = key;

            Label = null;

            Triggers =
                new List<string>();

            RewardSlots =
                new List<WeightedRewardSlot>();
        }

        public void AddTrigger(
            string trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger))
            {
                return;
            }

            Triggers.Add(trigger);
        }

        public void AddSlot(
            WeightedRewardSlot slot)
        {
            RewardSlots.Add(slot);
        }
    }
}