using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class RewardSlot
    {
        private static readonly HashSet<string>
            ShipKeyExcludedLocations =
                new HashSet<string>
                {
                    // Catuga — North Crate
                    "0294cd0074d1d204db0bac9eab555e55",

                    // Catuga — West Chest
                    "7790326666aad054e8775e3759d4a28a",

                    // Purrmaid Pool Catuga
                    "MainOverworld|WorldQuest_PurrmaidTreasure_Catuga|FSM|Get Reward",

                    // Catuga — East Crate
                    "b3c5d4e55fb2a914b8a547680be68a78",

                    // Defeat Mr Clean 1
                    "MainOverworld|MainQuest_01|FSM|Defeated Mr Clean"
                };

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
            get =>
                Location.RandomizedRewards[
                    RewardIndex
                ];

            set =>
                Location.RandomizedRewards[
                    RewardIndex
                ] = value;
        }

        public bool CanAccept(
            Reward reward)
        {
            if (reward == null)
            {
                return false;
            }

            if (
                !AllowCollectibles &&
                reward.Type ==
                    RewardType.Collectible
            )
            {
                return false;
            }

            // The randomized Ship Key must always be found
            // somewhere beyond Catuga.
            //
            // This deliberately includes its vanilla
            // Mr. Clean reward location.
            if (
                reward.Type ==
                    RewardType.QuestItem &&
                reward.Id ==
                    SpecialRewards.ShipKeyGuid &&
                ShipKeyExcludedLocations.Contains(
                    Location.Key
                )
            )
            {
                return false;
            }

            return true;
        }
    }
}