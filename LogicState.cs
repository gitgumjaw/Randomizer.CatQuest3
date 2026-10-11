namespace Randomizer.CatQuest3
{
    public class LogicState
    {
        public bool HasShipKey { get; set; }

        public bool HasTwinCastleKey { get; set; }

        public int TentakeyCount { get; set; }

        public int SeekerKeyCount { get; set; }

        public int LovepurrBookCount { get; set; }

        public bool HasInfinityKey { get; set; }

        public bool HasNorthStarEssence { get; set; }


        public bool HasVoodooDoll { get; set; }

        public bool HasSmithyHammer { get; set; }

        public bool HasJugOfMilk { get; set; }

        public bool HasCathulhuFang { get; set; }

        public bool HasConcertTicket { get; set; }

        public bool HasNecropawmicon { get; set; }

        public bool HasDragonbone { get; set; }

        public int CatfishCount { get; set; }


        public LogicState()
        {
            HasShipKey = false;

            HasTwinCastleKey = false;

            TentakeyCount = 0;

            SeekerKeyCount = 0;

            LovepurrBookCount = 0;

            HasInfinityKey = false;

            HasNorthStarEssence = false;


            HasVoodooDoll = false;

            HasSmithyHammer = false;

            HasJugOfMilk = false;

            HasCathulhuFang = false;

            HasConcertTicket = false;

            HasNecropawmicon = false;

            HasDragonbone = false;

            CatfishCount = 0;
        }
    }
}