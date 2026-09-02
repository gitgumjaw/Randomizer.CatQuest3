using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class RewardLocation
    {
        public string Key { get; }

        public List<Reward> VanillaRewards { get; }
        public List<Reward> RandomizedRewards { get; set; }

        public RewardLocation(
            string key,
            IEnumerable<Reward> vanillaRewards)
        {
            Key = key;

            VanillaRewards =
                new List<Reward>(vanillaRewards);

            RandomizedRewards =
                new List<Reward>(VanillaRewards);
        }

        public RewardLocation(
            string key,
            Reward vanillaReward)
            : this(
                key,
                new[] { vanillaReward })
        {
        }
    }
}