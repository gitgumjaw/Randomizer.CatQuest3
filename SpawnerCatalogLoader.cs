// Cat Quest III Randomizer
// File: SpawnerCatalogLoader.cs
// Version: SpawnerCatalogLoader_20261108_v06
// Read-only catalog with fail-closed eligibility and encounter restrictions.
// v05: 2DLocation permits XOnly enemies even when MovementRequired is present;
// CannotMove remains excluded from MovementRequired spawners.
// Place SpawnerCatalog.txt in Data beside EnemyCatalog.txt.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Randomizer.CatQuest3
{
    public sealed class SpawnerCatalogEntry
    {
        public string Scene;
        public string SpawnerId;
        public string Name;
        public string Position; // informational, not part of the lookup key
        public string Type; // Raw catalog value: Grunt, Boss, Grunt,Cutscene, Boss,Cutscene
        public string PrimaryType;
        public bool IsCutscene;
        public string Terrain;
        public string Restrictions;
        public bool Randomize;
        public bool HasRandomizeField;
        public string Notes;
    }

    public static class SpawnerCatalogLoader
    {
        private static Dictionary<string, SpawnerCatalogEntry> entries;

        private static string Key(string scene, string id)
        {
            return scene + "|" + id;
        }

        // Read-only snapshot for deterministic planning across ALL enabled spawners.
        public static List<SpawnerCatalogEntry> AllEntries()
        {
            EnsureLoaded();
            return new List<SpawnerCatalogEntry>(entries.Values);
        }

        public static SpawnerCatalogEntry Find(string scene, string id)
        {
            EnsureLoaded();
            SpawnerCatalogEntry result;
            return entries.TryGetValue(Key(scene, id), out result) ? result : null;
        }

        public static bool IsCompatible(SpawnerCatalogEntry spawner, EnemyCatalogEntry enemy)
        {
            if (spawner == null || !spawner.Randomize || enemy == null ||
                !enemy.CanRandomize || enemy.Asset == null)
                return false;

            if (!string.Equals(spawner.PrimaryType, enemy.Type, StringComparison.OrdinalIgnoreCase) ||
                !EnemyCatalogLoader.HasTerrain(enemy, spawner.Terrain))
                return false;

            if (EnemyCatalogLoader.HasRestriction(enemy, "NeedsSpikes") &&
                !HasRestriction(spawner, "SpikesAccessible"))
                return false;

            if (EnemyCatalogLoader.HasRestriction(enemy, "Unkillable") &&
                HasRestriction(spawner, "KillRequired"))
                return false;

            if (HasRestriction(spawner, "MovementRequired"))
            {
                if (EnemyCatalogLoader.HasRestriction(enemy, "CannotMove"))
                    return false;
                if (EnemyCatalogLoader.HasRestriction(enemy, "XOnly") &&
                    !HasRestriction(spawner, "2DLocation"))
                    return false;
            }

            // Old special-case exclusions remain encoded in enemy eligibility.
            return true;
        }

        public static bool HasRestriction(SpawnerCatalogEntry entry, string restriction)
        {
            if (entry == null) return false;
            foreach (string item in (entry.Restrictions ?? "").Split(','))
                if (string.Equals(item.Trim(), restriction, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static void EnsureLoaded()
        {
            if (entries != null) return;
            entries = new Dictionary<string, SpawnerCatalogEntry>(StringComparer.Ordinal);

            string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(folder, "Data", "SpawnerCatalog.txt");
            if (!File.Exists(path))
            {
                Plugin.Log.LogWarning("SPAWNER CATALOG | Missing file. All spawners vanilla: " + path);
                return;
            }

            int rejected = 0;
            int enabled = 0;
            int disabled = 0;
            SpawnerCatalogEntry current = null;

            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                int separator = line.IndexOf('|');
                if (separator < 0) { rejected++; continue; }
                string key = line.Substring(0, separator).Trim().ToUpperInvariant();
                string value = line.Substring(separator + 1).Trim();

                if (key == "SPAWNER")
                {
                    Add(current, ref rejected, ref enabled, ref disabled);
                    // v02: SPAWNER|id, followed by SCENE|scene.
                    // Also accept v01 SPAWNER|scene|id for safe upgrades.
                    int split = value.IndexOf('|');
                    current = split >= 0
                        ? new SpawnerCatalogEntry
                        {
                            Scene = value.Substring(0, split).Trim(),
                            SpawnerId = value.Substring(split + 1).Trim()
                        }
                        : new SpawnerCatalogEntry { SpawnerId = value };
                    continue;
                }

                if (current == null) { rejected++; continue; }
                switch (key)
                {
                    case "NAME": current.Name = value; break;
                    case "SCENE": current.Scene = value; break;
                    case "POSITION": current.Position = value; break;
                    case "TYPE": current.Type = value; break;
                    case "TERRAIN": current.Terrain = value; break;
                    case "RESTRICTIONS": current.Restrictions = value; break;
                    case "NOTES": current.Notes = value; break;
                    case "RANDOMIZE":
                        current.HasRandomizeField = true;
                        current.Randomize = string.Equals(value, "Yes", StringComparison.OrdinalIgnoreCase);
                        if (!current.Randomize && !string.Equals(value, "No", StringComparison.OrdinalIgnoreCase))
                            Plugin.Log.LogWarning("SPAWNER CATALOG | Invalid RANDOMIZE value: " + value);
                        break;
                    default:
                        rejected++;
                        Plugin.Log.LogWarning("SPAWNER CATALOG | Unknown field: " + key);
                        break;
                }
            }
            Add(current, ref rejected, ref enabled, ref disabled);
            Plugin.Log.LogInfo("SPAWNER CATALOG | Entries:" + entries.Count +
                " | Enabled:" + enabled + " | Disabled:" + disabled + " | Rejected:" + rejected);
        }

        private static void Add(SpawnerCatalogEntry e, ref int rejected, ref int enabled, ref int disabled)
        {
            if (e == null) return;
            // TYPE is a primary classification plus an optional Cutscene modifier.
            // Reject unknown modifiers and malformed combinations; never silently
            // randomize a spawner with ambiguous classifications.
            string[] typeParts = (e.Type ?? "").Split(',');
            bool validType = typeParts.Length >= 1 && typeParts.Length <= 2 &&
                (string.Equals(typeParts[0].Trim(), "Grunt", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(typeParts[0].Trim(), "Boss", StringComparison.OrdinalIgnoreCase));
            bool isCutscene = typeParts.Length == 2 &&
                string.Equals(typeParts[1].Trim(), "Cutscene", StringComparison.OrdinalIgnoreCase);
            validType = validType && (typeParts.Length == 1 || isCutscene);
            if (validType)
            {
                e.PrimaryType = typeParts[0].Trim();
                e.IsCutscene = isCutscene;
            }

            bool valid =
                !string.IsNullOrWhiteSpace(e.Scene) &&
                !string.IsNullOrWhiteSpace(e.SpawnerId) &&
                validType &&
                (string.Equals(e.Terrain, "Ground", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(e.Terrain, "Aquatic", StringComparison.OrdinalIgnoreCase)) &&
                e.HasRandomizeField &&
                !entries.ContainsKey(Key(e.Scene, e.SpawnerId));
            if (!valid)
            {
                rejected++;
                Plugin.Log.LogWarning("SPAWNER CATALOG | Invalid/duplicate entry: " + e.Scene + "|" + e.SpawnerId);
                return;
            }
            entries.Add(Key(e.Scene, e.SpawnerId), e);
            if (e.Randomize) enabled++; else disabled++;
        }
    }
}
