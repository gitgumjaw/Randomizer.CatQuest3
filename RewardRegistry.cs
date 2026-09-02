using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RewardRegistry
    {
        private static readonly Dictionary<string, RewardLocation> locations =
            new Dictionary<string, RewardLocation>();

        public static void Register(RewardLocation location)
        {
            if (locations.TryGetValue(location.Key, out RewardLocation existing))
            {
                existing.VanillaRewards.AddRange(location.VanillaRewards);
                existing.RandomizedRewards.AddRange(location.VanillaRewards);

                Plugin.Log.LogInfo(
                    $"Merged into: {location.Key} | " +
                    $"Total Rewards At Location: {existing.VanillaRewards.Count}"
                );

                return;
            }

            locations[location.Key] = location;

            string rewardText = "";

            foreach (Reward reward in location.VanillaRewards)
            {
                if (rewardText.Length > 0)
                {
                    rewardText += ", ";
                }

                rewardText += $"{reward.Type} {reward.Id}";
            }

            Plugin.Log.LogInfo(
                $"Registered: {location.Key} | " +
                $"{rewardText} | " +
                $"Total Locations: {locations.Count}"
            );
        }

        public static RewardLocation Get(string key)
        {
            if (locations.TryGetValue(key, out RewardLocation location))
            {
                return location;
            }

            return null;
        }

        public static int Count
        {
            get
            {
                return locations.Count;
            }
        }
    }
}