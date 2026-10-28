// Cat Quest III Randomizer | EnemyCatalogLoader.cs
// Generated: 20261028_090955_UTC | Revision: 07
// Compatible patch: EnemyWorldRandomizerPatch_20261028_083757_UTC_v05.cs (unchanged)
// Changes: NeedsSpikes is no longer a special-case restriction; BossShip slots require BossShip-tagged candidates.
// Retains RANDOMIZE|No, Boss categories, Unkillable restrictions, and variation grouping.
// Copy contents into existing EnemyCatalogLoader.cs class in Visual Studio.
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
        public string Environment;
        public string Category;
        public bool CanRandomize;
        public string Restrictions;
        public UnitConfig Asset;
    }

    public static class EnemyCatalogLoader
    {
        private static Dictionary<string, EnemyCatalogEntry> entries;
        private static List<EnemyCatalogEntry> ordinary;
        private static List<EnemyCatalogEntry> miniBosses;
        private static List<EnemyCatalogEntry> bosses;

        public static EnemyCatalogEntry Find(string configName)
        {
            EnsureLoaded();
            EnemyCatalogEntry result;
            return entries.TryGetValue(configName, out result) ? result : null;
        }

        public static List<EnemyCatalogEntry> EligibleFor(string environment, string category = "Ordinary",
            bool originalBossShipSlot = false, bool aquaticWorldSpawn = false)
        {
            EnsureLoaded();
            List<EnemyCatalogEntry> results = new List<EnemyCatalogEntry>();
            List<EnemyCatalogEntry> source;
            if (string.Equals(category, "MiniBoss", StringComparison.OrdinalIgnoreCase))
                source = miniBosses;
            else if (string.Equals(category, "Boss", StringComparison.OrdinalIgnoreCase))
                source = bosses;
            else if (string.Equals(category, "Ordinary", StringComparison.OrdinalIgnoreCase))
                source = ordinary;
            else
                return results;
            foreach (EnemyCatalogEntry entry in source)
            {
                if (entry.Asset == null || !entry.CanRandomize)
                    continue;
                // Original BossShip slots only accept BossShip-tagged candidates.
                // NeedsSpikes has no special behavior: the catalog's Aquatic and
                // BossShip tags now determine eligibility normally.
                if (originalBossShipSlot && !HasRestriction(entry, "BossShip"))
                    continue;
                // Unkillable candidates are safe only in aquatic WORLD_SPAWN slots.
                if (HasRestriction(entry, "Unkillable") &&
                    !(aquaticWorldSpawn && string.Equals(environment, "Aquatic", StringComparison.OrdinalIgnoreCase)))
                    continue;
                // BossShip is permitted, but only in aquatic Ordinary encounters.
                if (HasRestriction(entry, "BossShip") &&
                    (!string.Equals(category, "Ordinary", StringComparison.OrdinalIgnoreCase) ||
                     !string.Equals(environment, "Aquatic", StringComparison.OrdinalIgnoreCase)))
                    continue;
                if (IsCompatible(entry.Environment, environment))
                    results.Add(entry);
            }
            return results;
        }

        public static bool HasRestriction(EnemyCatalogEntry entry, string restriction)
        {
            string[] parts = (entry.Restrictions ?? "").Split(',');
            foreach (string part in parts)
                if (string.Equals(part.Trim(), restriction, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        // A ground enemy may run in a 2D or a 3D land encounter.
        public static bool IsCompatible(string candidateEnvironment, string encounterEnvironment)
        {
            if (string.Equals(candidateEnvironment, "All", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(encounterEnvironment, "Aquatic", StringComparison.OrdinalIgnoreCase))
                return string.Equals(candidateEnvironment, "Aquatic", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(encounterEnvironment, "X_Only", StringComparison.OrdinalIgnoreCase))
                return string.Equals(candidateEnvironment, "Ground", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(candidateEnvironment, "X_Only", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(encounterEnvironment, "Ground", StringComparison.OrdinalIgnoreCase))
                return string.Equals(candidateEnvironment, "Ground", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private static void EnsureLoaded()
        {
            if (entries != null) return;
            entries = new Dictionary<string, EnemyCatalogEntry>(StringComparer.Ordinal);
            ordinary = new List<EnemyCatalogEntry>();
            miniBosses = new List<EnemyCatalogEntry>();
            bosses = new List<EnemyCatalogEntry>();
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
                    case "ENVIRONMENT": current.Environment = value; break;
                    case "CATEGORY": current.Category = value; break;
                    case "RANDOMIZE": current.CanRandomize = string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase); break;
                    case "RESTRICTIONS": current.Restrictions = value; break;
                    case "NOTES": break;
                    default: Plugin.Log.LogWarning("ENEMY CATALOG | Unknown field: " + key); break;
                }
            }
            if (current != null) AddEntry(current, ref rejected);

            Dictionary<string, UnitConfig> assets = new Dictionary<string, UnitConfig>(StringComparer.Ordinal);
            foreach (UnitConfig asset in Resources.FindObjectsOfTypeAll<UnitConfig>())
                if (asset != null && !assets.ContainsKey(asset.name)) assets.Add(asset.name, asset);

            int resolved = 0;
            foreach (EnemyCatalogEntry entry in entries.Values)
            {
                UnitConfig asset;
                if (assets.TryGetValue(entry.UnitConfigName, out asset))
                {
                    entry.Asset = asset;
                    resolved++;
                }
                else Plugin.Log.LogWarning("ENEMY CATALOG | Missing UnitConfig: " + entry.UnitConfigName);
                if (string.Equals(entry.Category, "Ordinary", StringComparison.OrdinalIgnoreCase))
                    ordinary.Add(entry);
                else if (string.Equals(entry.Category, "MiniBoss", StringComparison.OrdinalIgnoreCase))
                    miniBosses.Add(entry);
                else if (string.Equals(entry.Category, "Boss", StringComparison.OrdinalIgnoreCase))
                    bosses.Add(entry);
            }
            Plugin.Log.LogInfo("ENEMY CATALOG | Entries:" + entries.Count +
                " | Resolved:" + resolved + " | Ordinary:" + ordinary.Count +
                " | MiniBoss:" + miniBosses.Count + " | Boss:" + bosses.Count +
                " | Rejected:" + rejected);
            foreach (string environment in new[] { "Ground", "X_Only", "Aquatic" })
            {
                foreach (string category in new[] { "Ordinary", "MiniBoss", "Boss" })
                {
                    List<EnemyCatalogEntry> pool = EligibleFor(environment, category);
                    HashSet<string> groups = new HashSet<string>(StringComparer.Ordinal);
                    foreach (EnemyCatalogEntry entry in pool) groups.Add(entry.Variation);
                    Plugin.Log.LogInfo("ENEMY POOL | Category:" + category +
                        " | Environment:" + environment +
                        " | Variations:" + pool.Count + " | Groups:" + groups.Count);
                }
            }
        }

        private static void AddEntry(EnemyCatalogEntry entry, ref int rejected)
        {
            if (string.IsNullOrWhiteSpace(entry.UnitConfigName) ||
                string.IsNullOrWhiteSpace(entry.Variation) ||
                string.IsNullOrWhiteSpace(entry.Environment) ||
                string.IsNullOrWhiteSpace(entry.Category) ||
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
