namespace Randomizer.CatQuest3
{
    public class LogicState
    {
        public bool HasShipKey { get; set; }

        public bool HasTwinCastleKey { get; set; }

        public int TentakeyCount { get; set; }

        public int SeekerKeyCount { get; set; }

        public bool HasInfinityKey { get; set; }

        public bool HasNorthStarEssence { get; set; }


        public LogicState()
        {
            HasShipKey = false;

            HasTwinCastleKey = false;

            TentakeyCount = 0;

            SeekerKeyCount = 0;

            HasInfinityKey = false;

            HasNorthStarEssence = false;
        }
    }
}