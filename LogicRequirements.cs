using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class LogicRequirements
    {
        public bool RequiresShipKey { get; }

        public bool RequiresTwinCastleKey { get; }

        public int RequiredTentakeys { get; }

        public int RequiredSeekerKeys { get; }

        public int RequiredLovepurrBooks { get; }

        public bool RequiresInfinityKey { get; }

        public bool RequiresNorthStarEssence { get; }

        public bool RequiresShellPhone { get; }

        public bool RequiresVoodooDoll { get; }

        public bool RequiresSmithyHammer { get; }

        public bool RequiresJugOfMilk { get; }

        public bool RequiresCathulhuFang { get; }

        public bool RequiresConcertTicket { get; }

        public bool RequiresNecropawmicon { get; }

        public bool RequiresDragonbone { get; }

        public int RequiredCatfish { get; }


        public IReadOnlyList<string>
            RequiredSpells =>
                requiredSpells;

        public IReadOnlyList<string>
            RequiredLocations =>
                requiredLocations;


        private readonly List<string>
            requiredSpells;

        private readonly List<string>
            requiredLocations;


        public LogicRequirements(
            bool requiresShipKey = false,
            bool requiresTwinCastleKey = false,
            int requiredTentakeys = 0,
            int requiredSeekerKeys = 0,
            int requiredLovepurrBooks = 0,
            bool requiresInfinityKey = false,
            bool requiresNorthStarEssence = false,
            bool requiresVoodooDoll = false,
            bool requiresSmithyHammer = false,
            bool requiresJugOfMilk = false,
            bool requiresCathulhuFang = false,
            bool requiresConcertTicket = false,
            bool requiresNecropawmicon = false,
            bool requiresDragonbone = false,
            int requiredCatfish = 0,
            IEnumerable<string> requiredSpells = null,
            IEnumerable<string> requiredLocations = null,
            bool requiresShellPhone = false)
        {
            RequiresShipKey =
                requiresShipKey;

            RequiresTwinCastleKey =
                requiresTwinCastleKey;

            RequiredTentakeys =
                requiredTentakeys;

            RequiredSeekerKeys =
                requiredSeekerKeys;

            RequiredLovepurrBooks =
                requiredLovepurrBooks;

            RequiresInfinityKey =
                requiresInfinityKey;

            RequiresNorthStarEssence =
                requiresNorthStarEssence;

            RequiresShellPhone =
                requiresShellPhone;

            RequiresVoodooDoll =
                requiresVoodooDoll;

            RequiresSmithyHammer =
                requiresSmithyHammer;

            RequiresJugOfMilk =
                requiresJugOfMilk;

            RequiresCathulhuFang =
                requiresCathulhuFang;

            RequiresConcertTicket =
                requiresConcertTicket;

            RequiresNecropawmicon =
                requiresNecropawmicon;

            RequiresDragonbone =
                requiresDragonbone;

            RequiredCatfish =
                requiredCatfish;


            this.requiredSpells =
                requiredSpells == null
                    ? new List<string>()
                    : new List<string>(
                        requiredSpells
                    );

            this.requiredLocations =
                requiredLocations == null
                    ? new List<string>()
                    : new List<string>(
                        requiredLocations
                    );
        }


        public bool IsSatisfiedBy(
            LogicState state)
        {
            if (state == null)
            {
                return false;
            }

            if (RequiresShellPhone &&
                !state.HasShellPhone)
            {
                return false;
            }

            if (
                RequiresShipKey &&
                !state.HasShipKey
            )
            {
                return false;
            }


            if (
                RequiresTwinCastleKey &&
                !state.HasTwinCastleKey
            )
            {
                return false;
            }


            if (
                state.TentakeyCount <
                RequiredTentakeys
            )
            {
                return false;
            }


            if (
                state.SeekerKeyCount <
                RequiredSeekerKeys
            )
            {
                return false;
            }


            if (
                state.LovepurrBookCount <
                RequiredLovepurrBooks
            )
            {
                return false;
            }


            if (
                RequiresInfinityKey &&
                !state.HasInfinityKey
            )
            {
                return false;
            }


            if (
                RequiresNorthStarEssence &&
                !state.HasNorthStarEssence
            )
            {
                return false;
            }


            if (
                RequiresVoodooDoll &&
                !state.HasVoodooDoll
            )
            {
                return false;
            }


            if (
                RequiresSmithyHammer &&
                !state.HasSmithyHammer
            )
            {
                return false;
            }


            if (
                RequiresJugOfMilk &&
                !state.HasJugOfMilk
            )
            {
                return false;
            }


            if (
                RequiresCathulhuFang &&
                !state.HasCathulhuFang
            )
            {
                return false;
            }


            if (
                RequiresConcertTicket &&
                !state.HasConcertTicket
            )
            {
                return false;
            }


            if (
                RequiresNecropawmicon &&
                !state.HasNecropawmicon
            )
            {
                return false;
            }


            if (
                RequiresDragonbone &&
                !state.HasDragonbone
            )
            {
                return false;
            }


            if (
                state.CatfishCount <
                RequiredCatfish
            )
            {
                return false;
            }


            for (int i = 0;
                 i < requiredSpells.Count;
                 i++)
            {
                if (!state.HasSpell(
                        requiredSpells[i]))
                {
                    return false;
                }
            }


            for (int i = 0;
                 i < requiredLocations.Count;
                 i++)
            {
                if (!state.HasCompletedLocation(
                        requiredLocations[i]))
                {
                    return false;
                }
            }


            return true;
        }
    }
}