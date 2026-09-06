using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class RewardLocation
    {
        public string Key { get; }

        public List<Reward> VanillaRewards { get; }
        public List<Reward> RandomizedRewards { get; }

        // One entry for each reward slot.
        // Existing locations default to allowing collectibles.
        public List<bool> AllowCollectiblesBySlot { get; }

        public RewardLocation(
            string key,
            IEnumerable<Reward> vanillaRewards,
            IEnumerable<bool> allowCollectiblesBySlot = null)
        {
            Key = key;

            VanillaRewards =
                new List<Reward>(vanillaRewards);

            RandomizedRewards =
                new List<Reward>(VanillaRewards);

            AllowCollectiblesBySlot =
                new List<bool>();

            if (allowCollectiblesBySlot == null)
            {
                // Normal/default behavior.
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

                // A location must have exactly one restriction
                // value for every reward slot.
                if (AllowCollectiblesBySlot.Count != VanillaRewards.Count)
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
    }
}