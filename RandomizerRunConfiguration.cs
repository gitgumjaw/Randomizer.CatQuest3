namespace Randomizer.CatQuest3
{
    public sealed class RandomizerRunConfiguration
    {
        public bool Enabled
        {
            get;
            private set;
        }

        public bool Equipment
        {
            get;
            private set;
        }

        public bool Spells
        {
            get;
            private set;
        }

        public bool QuestItems
        {
            get;
            private set;
        }

        public bool ManaCrystals
        {
            get;
            private set;
        }

        public bool ResourcesCollectibles
        {
            get;
            private set;
        }

        public bool ShipKey
        {
            get;
            private set;
        }

        public bool InfinityKey
        {
            get;
            private set;
        }

        public bool BirdPoop
        {
            get;
            private set;
        }

        public bool MatchPlayerLevel
        {
            get;
            private set;
        }

        public string SeedText
        {
            get;
            private set;
        }

        public int SeedValue
        {
            get;
            private set;
        }


        public RandomizerRunConfiguration(
            bool enabled,
            bool equipment,
            bool spells,
            bool questItems,
            bool manaCrystals,
            bool resourcesCollectibles,
            bool shipKey,
            bool infinityKey,
            bool birdPoop,
            bool matchPlayerLevel,
            string seedText,
            int seedValue)
        {
            Enabled = enabled;
            Equipment = equipment;
            Spells = spells;
            QuestItems = questItems;
            ManaCrystals = manaCrystals;
            ResourcesCollectibles = resourcesCollectibles;
            ShipKey = shipKey;
            InfinityKey = infinityKey;
            BirdPoop = birdPoop;
            MatchPlayerLevel = matchPlayerLevel;
            SeedText = seedText ?? "";
            SeedValue = seedValue;
        }
    }
}
