using ProjectStar.Data;
using System;
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


        private const string
            MagicShopPrefix =
                "RANDOMIZER|MAGIC_SHOP|";

        private const string
            TavernTalesPrefix =
                "RANDOMIZER|TAVERN_TALES|";

        private const string
            InteriorMageShopPrefix =
                "Interior_MageShop|";

        private const string
            InteriorTavernPrefix =
                "Interior_Tavern|";

        private const string
            WantedPosterLabelPrefix =
                "Wanted Poster";


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


            // Acquiring the Voodoo Doll while already
            // inside the Magic Shop can immediately
            // trigger Bonnie's quest progression and
            // hijack control from the reward flow.
            //
            // Keep it out of both Bonnie's normal
            // Magic Shop rewards and all synthetic
            // Magic Shop lockbox locations.
            if (
                reward.Type ==
                    RewardType.QuestItem &&
                reward.Id ==
                    SpecialRewards.VoodooDollGuid &&
                IsMagicShopLocation()
            )
            {
                return false;
            }


            // Acquiring the Jug of Milk while interacting
            // with Tavern content can immediately advance
            // Mama Milka's progression.
            //
            // Keep it out of all Interior Tavern rewards,
            // Wanted Poster rewards, and future Tavern
            // Tales randomized locations.
            if (
                reward.Type ==
                    RewardType.QuestItem &&
                reward.Id ==
                    SpecialRewards.JugOfMilkGuid &&
                IsJugOfMilkExcludedLocation()
            )
            {
                return false;
            }


            return true;
        }


        private bool IsMagicShopLocation()
        {
            if (Location == null ||
                string.IsNullOrEmpty(
                    Location.Key))
            {
                return false;
            }


            return
                Location.Key.StartsWith(
                    InteriorMageShopPrefix,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                Location.Key.StartsWith(
                    MagicShopPrefix,
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private bool IsJugOfMilkExcludedLocation()
        {
            if (Location == null)
            {
                return false;
            }


            if (!string.IsNullOrEmpty(
                    Location.Key))
            {
                if (Location.Key.StartsWith(
                        InteriorTavernPrefix,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }


                if (Location.Key.StartsWith(
                        TavernTalesPrefix,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }


            if (!string.IsNullOrEmpty(
                    Location.Label) &&
                Location.Label.StartsWith(
                    WantedPosterLabelPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }


            return false;
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