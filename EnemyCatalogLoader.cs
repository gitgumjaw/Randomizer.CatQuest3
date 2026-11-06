// Cat Quest III Randomizer | EnemyCatalogLoader.cs
// Version: EnemyCatalogLoader_20261105_2301_EST_v09
// Supports comma-separated TERRAIN values (Ground,Aquatic).
// Compatible with EnemyWorldRandomizerPatch v12 and SpawnerCatalogLoader v04.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public sealed class EnemyCatalogEntry
    {
        public string UnitConfigName;
        public string Name;
        public string Variation;
        public string Type;
        public string Terrain;
        public string Restrictions;
        public bool CanRandomize;
        public UnitConfig Asset;

        // Transitional legacy accessors retained for other patches.
        public string Category
        {
            get
            {
                if (string.Equals(Type, "Boss", StringComparison.OrdinalIgnoreCase))
                    return EnemyCatalogLoader.HasTerrain(this, "Aquatic") &&
                           !EnemyCatalogLoader.HasTerrain(this, "Ground")
                        ? "Ordinary" : "Boss";
                return "Ordinary";
            }
        }

        public string Environment
        {
            get
            {
                if (EnemyCatalogLoader.HasRestriction(this, "Special")) return "Special";
                return Terrain;
            }
        }
    }

    public static class EnemyCatalogLoader
    {
        private static Dictionary<string, EnemyCatalogEntry> entries;
        private static List<EnemyCatalogEntry> grunts;
        private static List<EnemyCatalogEntry> groundBosses;
        private static List<EnemyCatalogEntry> aquaticBosses;

        public static EnemyCatalogEntry Find(string configName)
        {
            EnsureLoaded();
            EnemyCatalogEntry found;
            return configName != null && entries.TryGetValue(configName, out found) ? found : null;
        }

        // This is a terrain-membership test, not a string equality test.
        // TERRAIN|Ground,Aquatic participates in both pools.
        public static bool HasTerrain(EnemyCatalogEntry entry, string terrain)
        {
            return entry != null && ContainsToken(entry.Terrain, terrain);
        }

        private static bool ContainsToken(string csv, string token)
        {
            if (string.IsNullOrEmpty(csv)) return false;
            foreach (string value in csv.Split(','))
                if (string.Equals(value.Trim(), token, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        public static bool HasRestriction(EnemyCatalogEntry entry, string restriction)
        {
            if (entry == null) return false;

            // Compatibility with older code; BossShip is no longer a catalog flag.
            if (string.Equals(restriction, "BossShip", StringComparison.OrdinalIgnoreCase))
                return string.Equals(entry.Type, "Boss", StringComparison.OrdinalIgnoreCase) &&
                       HasTerrain(entry, "Aquatic");

            return ContainsToken(entry.Restrictions, restriction);
        }

        // Kept for compatibility with the current enemy randomizer patch.
        public static List<EnemyCatalogEntry> EligibleFor(
            string environment, string category = "Ordinary",
            bool originalBossShipSlot = false, bool aquaticWorldSpawn = false)
        {
            EnsureLoaded();
            List<EnemyCatalogEntry> result = new List<EnemyCatalogEntry>();
            List<EnemyCatalogEntry> source;

            if (originalBossShipSlot)
                source = aquaticBosses;
            else if (string.Equals(category, "Boss", StringComparison.OrdinalIgnoreCase))
                source = groundBosses;
            else if (string.Equals(category, "MiniBoss", StringComparison.OrdinalIgnoreCase))
                source = new List<EnemyCatalogEntry>();
            else if (string.Equals(category, "Ordinary", StringComparison.OrdinalIgnoreCase))
                source = grunts;
            else
                return result;

            foreach (EnemyCatalogEntry entry in source)
            {
                if (entry.Asset == null || !entry.CanRandomize) continue;
                if (!IsCompatible(entry.Terrain, environment)) continue;
                // Preserve the v08 guard for unkillable enemies.
                if (HasRestriction(entry, "Unkillable") &&
                    !(aquaticWorldSpawn &&
                      string.Equals(environment, "Aquatic", StringComparison.OrdinalIgnoreCase)))
                    continue;
                result.Add(entry);
            }
            return result;
        }

        // Compatible with the legacy X_Only lookup for Ground spawner pools.
        public static bool IsCompatible(string candidateTerrain, string encounterTerrain)
        {
            if (string.Equals(encounterTerrain, "Aquatic", StringComparison.OrdinalIgnoreCase))
                return ContainsToken(candidateTerrain, "Aquatic");
            if (string.Equals(encounterTerrain, "Ground", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(encounterTerrain, "X_Only", StringComparison.OrdinalIgnoreCase))
                return ContainsToken(candidateTerrain, "Ground");
            return false;
        }

        private static void EnsureLoaded()
        {
            if (entries != null) return;
            entries = new Dictionary<string, EnemyCatalogEntry>(StringComparer.Ordinal);
            grunts = new List<EnemyCatalogEntry>();
            groundBosses = new List<EnemyCatalogEntry>();
            aquaticBosses = new List<EnemyCatalogEntry>();

            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(folder, "Data", "EnemyCatalog.txt");
            if (!File.Exists(path))
            {
                Plugin.Log.LogError("ENEMY CATALOG | Missing file: " + path);
                return;
            }

            EnemyCatalogEntry current = null;
            int rejected = 0;
            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                int separator = line.IndexOf('|');
                if (separator < 0) { rejected++; continue; }
                string key = line.Substring(0, separator).Trim().ToUpperInvariant();
                string value = line.Substring(separator + 1).Trim();
                if (key == "UNITCONFIG")
                {
                    if (current != null) AddEntry(current, ref rejected);
                    current = new EnemyCatalogEntry { UnitConfigName = value, Restrictions = "None" };
                    continue;
                }
                if (current == null) { rejected++; continue; }
                switch (key)
                {
                    case "NAME": current.Name = value; break;
                    case "VARIATION": current.Variation = value; break;
                    case "TYPE": current.Type = value; break;
                    case "TERRAIN": current.Terrain = value; break;
                    case "RESTRICTIONS": current.Restrictions = value; break;
                    case "RANDOMIZE":
                        current.CanRandomize =
                        string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase); break;
                    case "NOTES": break;
                    default:
                        Plugin.Log.LogWarning("ENEMY CATALOG | Unknown field: " + key);
                        break;
                }
            }
            if (current != null) AddEntry(current, ref rejected);

            Dictionary<string, UnitConfig> assets =
                new Dictionary<string, UnitConfig>(StringComparer.Ordinal);
            foreach (UnitConfig asset in Resources.FindObjectsOfTypeAll<UnitConfig>())
                if (asset != null && !assets.ContainsKey(asset.name))
                    assets.Add(asset.name, asset);

            int resolved = 0;
            foreach (EnemyCatalogEntry entry in entries.Values)
            {
                UnitConfig asset;
                if (assets.TryGetValue(entry.UnitConfigName, out asset))
                {
                    entry.Asset = asset;
                    resolved++;
                }
                else
                    Plugin.Log.LogWarning("ENEMY CATALOG | Missing UnitConfig: " + entry.UnitConfigName);

                if (!entry.CanRandomize || HasRestriction(entry, "Special")) continue;
                if (string.Equals(entry.Type, "Grunt", StringComparison.OrdinalIgnoreCase))
                {
                    grunts.Add(entry); // Eligibility's terrain filter handles both Ground/Aquatic.
                }
                else
                {
                    if (HasTerrain(entry, "Ground")) groundBosses.Add(entry);
                    if (HasTerrain(entry, "Aquatic")) aquaticBosses.Add(entry);
                }
            }
            Plugin.Log.LogInfo("ENEMY CATALOG v09 | Entries:" + entries.Count +
                " | Resolved:" + resolved + " | Grunts:" + grunts.Count +
                " | GroundBosses:" + groundBosses.Count +
                " | AquaticBosses:" + aquaticBosses.Count +
                " | Rejected:" + rejected);

            foreach (string terrain in new[] { "Ground", "Aquatic" })
                foreach (string type in new[] { "Grunt", "Boss" })
                {
                    int count = 0;
                    HashSet<string> groups = new HashSet<string>(StringComparer.Ordinal);
                    foreach (EnemyCatalogEntry entry in entries.Values)
                    {
                        if (!entry.CanRandomize || entry.Asset == null ||
                            HasRestriction(entry, "Special") ||
                            !string.Equals(entry.Type, type, StringComparison.OrdinalIgnoreCase) ||
                            !HasTerrain(entry, terrain)) continue;
                        count++;
                        groups.Add(entry.Variation);
                    }
                    Plugin.Log.LogInfo("ENEMY POOL v09 | Type:" + type +
                        " | Terrain:" + terrain + " | Entries:" + count +
                        " | ResolvedGroups:" + groups.Count);
                }
        }

        private static bool ValidTerrainList(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return false;
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in csv.Split(','))
            {
                string token = raw.Trim();
                if ((!string.Equals(token, "Ground", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(token, "Aquatic", StringComparison.OrdinalIgnoreCase)) ||
                    !seen.Add(token)) return false;
            }
            return seen.Count > 0;
        }

        private static void AddEntry(EnemyCatalogEntry entry, ref int rejected)
        {
            if (string.IsNullOrWhiteSpace(entry.UnitConfigName) ||
                string.IsNullOrWhiteSpace(entry.Variation) ||
                !(string.Equals(entry.Type, "Grunt", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(entry.Type, "Boss", StringComparison.OrdinalIgnoreCase)) ||
                !ValidTerrainList(entry.Terrain) ||
                entries.ContainsKey(entry.UnitConfigName))
            {
                rejected++;
                Plugin.Log.LogWarning("ENEMY CATALOG | Invalid/duplicate entry: " + entry.UnitConfigName);
                return;
            }
            entries.Add(entry.UnitConfigName, entry);
        }
    }
}
