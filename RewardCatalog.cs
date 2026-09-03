using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RewardCatalog
    {
        private static readonly Dictionary<string, RewardLocation> locations =
            new Dictionary<string, RewardLocation>();

        public static void Add(RewardLocation location)
        {
            locations[location.Key] = location;
        }

        public static RewardLocation Get(string key)
        {
            if (locations.TryGetValue(key, out RewardLocation location))
            {
                return location;
            }

            return null;
        }

        public static List<RewardLocation> GetAll()
        {
            return new List<RewardLocation>(locations.Values);
        }

        public static int Count
        {
            get
            {
                return locations.Count;
            }
        }

        public static List<RewardSlot> GetEligibleSlots(
    RandomizerSettings settings)
        {
            List<RewardSlot> eligibleSlots =
                new List<RewardSlot>();

            foreach (RewardLocation location in locations.Values)
            {
                for (int i = 0; i < location.VanillaRewards.Count; i++)
                {
                    RewardSlot slot =
                        new RewardSlot(
                            location,
                            i
                        );

                    if (RandomizerEligibility.IsEnabled(slot, settings))
                    {
                        eligibleSlots.Add(slot);
                    }
                }
            }

            return eligibleSlots;
        }

        public static void AddRange(IEnumerable<RewardLocation> newLocations)
        {
            foreach (RewardLocation location in newLocations)
            {
                Add(location);
            }
        }
    }
}