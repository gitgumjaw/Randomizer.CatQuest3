namespace Randomizer.CatQuest3
{
    public class Reward
    {
        public RewardType Type { get; }
        public string Id { get; }

        public Reward(RewardType type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}