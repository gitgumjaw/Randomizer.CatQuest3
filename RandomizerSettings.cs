namespace Randomizer.CatQuest3
{
    public class RandomizerSettings
    {
        // Main reward categories
        public bool RandomizeEquipment { get; set; }
        public bool RandomizeSpells { get; set; }
        public bool RandomizeQuestItems { get; set; }
        public bool RandomizeManaCrystals { get; set; }
        public bool RandomizeCollectibles { get; set; }

        // Equipment options
        public bool MatchEquipmentLevelToPlayer { get; set; }

        // Special-case rewards
        public bool RandomizeShipKey { get; set; }
        public bool RandomizeInfinityKey { get; set; }
        public bool RandomizeNorthStarEssence { get; set; }
        public bool RandomizeBirdPoop { get; set; }
    }
}