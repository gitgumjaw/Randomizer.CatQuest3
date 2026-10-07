using ProjectStar.Data;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class RewardSlot
    {
        private const string
            StartingMeleeLocation =
                "RANDOMIZER|STARTING|MELEE";

        private const string
            StartingBodyLocation =
                "RANDOMIZER|STARTING|BODY";

        private const string
            StartingRangedLocation =
                "RANDOMIZER|STARTING|RANGED";

        private const string
            StartingSpellLocation =
                "RANDOMIZER|STARTING|SPELL";

        private const string
            StartingShipSpellLocation =
                "RANDOMIZER|STARTING|SHIP_SPELL";


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
            Location.VanillaRewards[
                RewardIndex
            ];


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


        public bool IsVanillaRandom =>
            Location.IsVanillaRandomBySlot[
                RewardIndex
            ];


        public bool CanAccept(
            Reward reward)
        {
            if (reward == null)
            {
                return false;
            }


            if (!CanAcceptStartingLoadoutReward(
                    reward))
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


            // The Twin Castle Key may not be placed
            // somewhere that already requires the
            // Twin Castle Key to reach.
            if (
                reward.Type ==
                    RewardType.QuestItem &&
                reward.Id ==
                    SpecialRewards.TwinCastleKeyGuid &&
                Location.LogicRequirements
                    .RequiresTwinCastleKey
            )
            {
                return false;
            }


            return true;
        }


        private bool CanAcceptStartingLoadoutReward(
            Reward reward)
        {
            if (Location == null)
            {
                return true;
            }


            if (Location.Key ==
                    StartingSpellLocation)
            {
                return
                    reward.Type ==
                        RewardType.Spell;
            }


            if (Location.Key ==
                    StartingShipSpellLocation)
            {
                return
                    reward.Type ==
                        RewardType.ShipSpell;
            }


            if (Location.Key !=
                    StartingMeleeLocation &&
                Location.Key !=
                    StartingBodyLocation &&
                Location.Key !=
                    StartingRangedLocation)
            {
                return true;
            }


            if (reward.Type !=
                    RewardType.Equipment ||
                VanillaReward == null ||
                VanillaReward.Type !=
                    RewardType.Equipment)
            {
                return false;
            }


            EquipmentItemData vanillaEquipment =
                RewardDataResolver.GetEquipment(
                    VanillaReward.Id
                );

            EquipmentItemData candidateEquipment =
                RewardDataResolver.GetEquipment(
                    reward.Id
                );


            if (vanillaEquipment == null ||
                candidateEquipment == null)
            {
                return false;
            }


            return
                candidateEquipment.partType ==
                    vanillaEquipment.partType &&
                candidateEquipment.weaponType ==
                    vanillaEquipment.weaponType;
        }
    }
}