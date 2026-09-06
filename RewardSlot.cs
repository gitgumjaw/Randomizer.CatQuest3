namespace Randomizer.CatQuest3
{
    public class RewardSlot
    {
        public RewardLocation Location { get; }
        public int RewardIndex { get; }

        public bool AllowCollectibles { get; }

        public RewardSlot(
            RewardLocation location,
            int rewardIndex,
            bool allowCollectibles = true)
        {
            Location = location;
            RewardIndex = rewardIndex;
            AllowCollectibles = allowCollectibles;
        }

        public Reward VanillaReward =>
            Location.VanillaRewards[RewardIndex];

        public Reward RandomizedReward
        {
            get => Location.RandomizedRewards[RewardIndex];
            set => Location.RandomizedRewards[RewardIndex] = value;
        }

        public bool CanAccept(Reward reward)
        {
            if (reward == null)
            {
                return false;
            }

            if (!AllowCollectibles &&
                reward.Type == RewardType.Collectible)
            {
                return false;
            }

            return true;
        }
    }
}