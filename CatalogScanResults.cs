using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class CatalogScanResults
    {
        private static readonly Dictionary<string, CatalogRewardLocation> locations =
            new Dictionary<string, CatalogRewardLocation>();

        private static bool dirty;

        public static CatalogRewardLocation GetOrCreate(string key)
        {
            if (locations.TryGetValue(
                key,
                out CatalogRewardLocation location))
            {
                return location;
            }

            location =
                new CatalogRewardLocation(key);

            locations[key] = location;

            MarkDirty();

            return location;
        }

        public static void Set(CatalogRewardLocation location)
        {
            if (location == null)
            {
                return;
            }

            locations[location.Key] = location;

            MarkDirty();
        }

        public static CatalogRewardLocation Get(string key)
        {
            if (locations.TryGetValue(
                key,
                out CatalogRewardLocation location))
            {
                return location;
            }

            return null;
        }

        public static List<CatalogRewardLocation> GetAll()
        {
            return new List<CatalogRewardLocation>(
                locations.Values
            );
        }

        public static int Count
        {
            get
            {
                return locations.Count;
            }
        }

        public static bool IsDirty
        {
            get
            {
                return dirty;
            }
        }

        public static void MarkDirty()
        {
            dirty = true;
        }

        public static void MarkClean()
        {
            dirty = false;
        }
    }
}