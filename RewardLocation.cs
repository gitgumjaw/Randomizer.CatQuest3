using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class RewardLocation
    {
        public string Key { get; }

        public string Label { get; }

        public List<string> Triggers { get; }

        public List<Reward> VanillaRewards { get; }
        public List<Reward> RandomizedRewards { get; }

        public List<bool> AllowCollectiblesBySlot { get; }

        public RewardLocation(
            string key,
            IEnumerable<Reward> vanillaRewards,
            IEnumerable<bool> allowCollectiblesBySlot = null,
            IEnumerable<string> triggers = null,
            string label = null)
        {
            Key = key;

            Label = label;

            Triggers =
                triggers == null
                    ? new List<string>()
                    : new List<string>(triggers);

            VanillaRewards =
                new List<Reward>(vanillaRewards);

            RandomizedRewards =
                new List<Reward>(VanillaRewards);

            AllowCollectiblesBySlot =
                new List<bool>();

            if (allowCollectiblesBySlot == null)
            {
                for (int i = 0; i < VanillaRewards.Count; i++)
                {
                    AllowCollectiblesBySlot.Add(true);
                }
            }
            else
            {
                AllowCollectiblesBySlot.AddRange(
                    allowCollectiblesBySlot
                );

                if (AllowCollectiblesBySlot.Count !=
                    VanillaRewards.Count)
                {
                    throw new System.ArgumentException(
                        "Reward slot restriction count must match reward count."
                    );
                }
            }
        }

        public RewardLocation(
            string key,
            Reward vanillaReward,
            bool allowCollectibles = true)
            : this(
                key,
                new[] { vanillaReward },
                new[] { allowCollectibles })
        {
        }
        public int FindVanillaRewardIndex(
    RewardType type,
    string id)
        {
            int matchIndex = -1;

            for (int i = 0; i < VanillaRewards.Count; i++)
            {
                Reward reward =
                    VanillaRewards[i];

                if (reward.Type != type ||
                    reward.Id != id)
                {
                    continue;
                }

                if (matchIndex != -1)
                {
                    Plugin.Log.LogError(
                        $"Ambiguous vanilla reward match | " +
                        $"Location:{Key} | " +
                        $"Type:{type} | " +
                        $"Id:{id}"
                    );

                    return -1;
                }

                matchIndex = i;
            }

            return matchIndex;
        }
    }
}