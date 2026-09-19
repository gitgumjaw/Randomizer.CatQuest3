using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RandomizerEligibility
    {
        private static readonly HashSet<string>
            GoingPostalMailGuids =
                new HashSet<string>
                {
                    "08952e6278361304ea3d7e988637de6e",
                    "b55e4020c560a5646be96d8fb3ba5568",
                    "7fd62494db0d19948b662959e79ae9bc",
                    "04efe6e991cef4e47a3ee7b4b3cad594",
                    "e26a1f630c0f302429d5aac6e4c7b8ef",
                    "266de2538fe901e40b7bcce7c5f7d4dd",
                    "f14e80a6d251cea459653f49e25477ef",
                    "c2c077f8e3e160f4fa2e9039ddaa1a19",
                    "418b2dc4a64bab64490e2c1512b3b82f"
                };


        public static bool IsEnabled(
            Reward reward,
            RandomizerSettings settings)
        {
            if (reward == null || settings == null)
            {
                return false;
            }

            switch (reward.Type)
            {
                case RewardType.Equipment:
                    if (reward.Id ==
                        SpecialRewards.BirdPoopGuid)
                    {
                        return
                            settings.RandomizeEquipment &&
                            settings.RandomizeBirdPoop;
                    }

                    return settings.RandomizeEquipment;

                case RewardType.Blueprint:
                case RewardType.ShipSpell:
                    return settings.RandomizeEquipment;

                case RewardType.Spell:
                    return settings.RandomizeSpells;

                case RewardType.QuestItem:
                    if (reward.Id ==
                        SpecialRewards.ShipKeyGuid)
                    {
                        return
                            settings.RandomizeQuestItems &&
                            settings.RandomizeShipKey;
                    }

                    if (reward.Id ==
                        SpecialRewards.InfinityKeyGuid)
                    {
                        return
                            settings.RandomizeQuestItems &&
                            settings.RandomizeInfinityKey;
                    }

                    if (reward.Id ==
                        SpecialRewards.NorthStarEssenceGuid)
                    {
                        return
                            settings.RandomizeQuestItems &&
                            settings.RandomizeNorthStarEssence;
                    }

                    return settings.RandomizeQuestItems;

                case RewardType.ManaCrystal:
                    return settings.RandomizeManaCrystals;

                case RewardType.Collectible:
                    return settings.RandomizeCollectibles;

                default:
                    return false;
            }
        }


        public static bool IsEnabled(
            RewardSlot slot,
            RandomizerSettings settings)
        {
            if (slot == null)
            {
                return false;
            }

            Reward vanillaReward =
                slot.VanillaReward;

            if (
                vanillaReward != null &&
                vanillaReward.Type ==
                    RewardType.QuestItem &&
                GoingPostalMailGuids.Contains(
                    vanillaReward.Id
                )
            )
            {
                return false;
            }

            return IsEnabled(
                vanillaReward,
                settings
            );
        }
    }
}