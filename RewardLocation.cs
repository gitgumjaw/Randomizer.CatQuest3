namespace Randomizer.CatQuest3
{
    public class RewardLocation
    {
        public string Key { get; }
        public Reward VanillaReward { get; }

        public RewardLocation(string key, Reward vanillaReward)
        {
            Key = key;
            VanillaReward = vanillaReward;
        }
    }
}