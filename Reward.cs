namespace Randomizer.CatQuest3
{
    public class Reward
    {
        public RewardType Type { get; }
        public string Id { get; }
        public CollectibleRewardData CollectibleData { get; }

        public Reward(
            RewardType type,
            string id,
            CollectibleRewardData collectibleData = null)
        {
            Type = type;
            Id = id;
            CollectibleData = collectibleData;
        }
    }
}