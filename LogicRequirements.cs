namespace Randomizer.CatQuest3
{
    public class LogicRequirements
    {
        public bool RequiresShipKey { get; }

        public bool RequiresTwinCastleKey { get; }

        public int RequiredTentakeys { get; }

        public int RequiredSeekerKeys { get; }

        public bool RequiresInfinityKey { get; }

        public bool RequiresNorthStarEssence { get; }


        public LogicRequirements(
            bool requiresShipKey = false,
            bool requiresTwinCastleKey = false,
            int requiredTentakeys = 0,
            int requiredSeekerKeys = 0,
            bool requiresInfinityKey = false,
            bool requiresNorthStarEssence = false)
        {
            RequiresShipKey =
                requiresShipKey;

            RequiresTwinCastleKey =
                requiresTwinCastleKey;

            RequiredTentakeys =
                requiredTentakeys;

            RequiredSeekerKeys =
                requiredSeekerKeys;

            RequiresInfinityKey =
                requiresInfinityKey;

            RequiresNorthStarEssence =
                requiresNorthStarEssence;
        }


        public bool IsSatisfiedBy(
            LogicState state)
        {
            if (state == null)
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


            return true;
        }
    }
}