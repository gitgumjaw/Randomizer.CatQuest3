namespace Randomizer.CatQuest3
{
    public static class RandomizerEligibility
    {
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
                    if (reward.Id == SpecialRewards.BirdPoopGuid)
                    {
                        return settings.RandomizeEquipment &&
                               settings.RandomizeBirdPoop;
                    }

                    return settings.RandomizeEquipment;

                case RewardType.Blueprint:
                    return settings.RandomizeEquipment;

                case RewardType.Spell:
                    return settings.RandomizeSpells;

                case RewardType.QuestItem:
                    if (reward.Id == SpecialRewards.ShipKeyGuid)
                    {
                        return settings.RandomizeQuestItems &&
                               settings.RandomizeShipKey;
                    }

                    if (reward.Id == SpecialRewards.InfinityKeyGuid)
                    {
                        return settings.RandomizeQuestItems &&
                               settings.RandomizeInfinityKey;
                    }

                    if (reward.Id == SpecialRewards.NorthStarEssenceGuid)
                    {
                        return settings.RandomizeQuestItems &&
                               settings.RandomizeNorthStarEssence;
                    }

                    return settings.RandomizeQuestItems;

                case RewardType.ManaCrystal:
                    return settings.RandomizeManaCrystals;

                case RewardType.Collectible:
                    return settings.RandomizeCollectibles;

                case RewardType.KeyItem:
                    return false;

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

            return IsEnabled(
                slot.VanillaReward,
                settings
            );
        }
    }
}