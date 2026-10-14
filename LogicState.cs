using System;
using System.Collections.Generic;

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


        private readonly Dictionary<string, int>
            spellCounts =
                new Dictionary<string, int>(
                    StringComparer.Ordinal
                );

        private readonly HashSet<string>
            completedLocations =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


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


        public int GetSpellCount(
            string spellGuid)
        {
            if (string.IsNullOrEmpty(
                    spellGuid))
            {
                return 0;
            }

            if (spellCounts.TryGetValue(
                    spellGuid,
                    out int count))
            {
                return count;
            }

            return 0;
        }


        public bool HasSpell(
            string spellGuid)
        {
            return
                GetSpellCount(
                    spellGuid
                ) > 0;
        }


        public bool AddSpell(
            string spellGuid)
        {
            if (string.IsNullOrEmpty(
                    spellGuid))
            {
                return false;
            }

            int currentCount =
                GetSpellCount(
                    spellGuid
                );

            spellCounts[
                spellGuid
            ] =
                currentCount + 1;

            return true;
        }


        public bool HasCompletedLocation(
            string locationKey)
        {
            if (string.IsNullOrEmpty(
                    locationKey))
            {
                return false;
            }

            return completedLocations.Contains(
                locationKey
            );
        }


        public bool AddCompletedLocation(
            string locationKey)
        {
            if (string.IsNullOrEmpty(
                    locationKey))
            {
                return false;
            }

            return completedLocations.Add(
                locationKey
            );
        }
    }
}