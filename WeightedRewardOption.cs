namespace Randomizer.CatQuest3
{
    public class WeightedRewardOption
    {
        public Reward Reward { get; }
        public int Weight { get; }

        public WeightedRewardOption(
            Reward reward,
            int weight)
        {
            Reward = reward;
            Weight = weight;
        }
    }
}