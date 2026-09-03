namespace Randomizer.CatQuest3
{
    public class RewardSlot
    {
        public RewardLocation Location { get; }
        public int RewardIndex { get; }

        public RewardSlot(
            RewardLocation location,
            int rewardIndex)
        {
            Location = location;
            RewardIndex = rewardIndex;
        }

        public Reward VanillaReward =>
            Location.VanillaRewards[RewardIndex];

        public Reward RandomizedReward
        {
            get => Location.RandomizedRewards[RewardIndex];
            set => Location.RandomizedRewards[RewardIndex] = value;
        }
    }
}