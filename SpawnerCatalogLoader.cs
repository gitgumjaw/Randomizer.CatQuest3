// Cat Quest III Randomizer
// File: SpawnerCatalogLoader.cs
// Version: SpawnerCatalogLoader_20261108_v09
// Read-only catalog with fail-closed eligibility and encounter restrictions.
// v08: Optional PATH distinguishes shared spawner IDs without altering legacy entries.
// v09: Opt-in Gentlebros boss slots accept aquatic boss configs.
// 2DOnly requires 2DLocation regardless of MovementRequired.
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
        public string Path; // optional; exact hierarchy path for shared-ID encounters
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

        // Legacy keys are preserved exactly. PATH entries add a suffix only
        // when one is configured, so existing seeds do not change identity.
        public static string KeyFor(SpawnerCatalogEntry entry)
        {
            return Key(entry.Scene, entry.SpawnerId, entry.Path);
        }

        private static string Key(string scene, string id, string path = null)
        {
            string baseKey = scene + "|" + id;
            return string.IsNullOrEmpty(path)
                ? baseKey
                : baseKey + "|PATH|" + path;
        }

        // Match a specific hierarchy first. Fall back ONLY to a legacy
        // unpathed record. Other path-specific siblings cannot match.
        public static SpawnerCatalogEntry Find(string scene, string id, string path)
        {
            EnsureLoaded();
            SpawnerCatalogEntry result;
            if (!string.IsNullOrEmpty(path) &&
                entries.TryGetValue(Key(scene, id, path), out result))
                return result;
            return entries.TryGetValue(Key(scene, id), out result) ? result : null;
        }

        public static string GetHierarchyPath(UnityEngine.Transform transform)
        {
            var names = new List<string>();
            int depth = 0;
            while (transform != null && depth++ < 128)
            {
                names.Add(transform.name);
                transform = transform.parent;
            }
            names.Reverse();
            return string.Join("/", names.ToArray());
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
            return Find(scene, id, null);
        }

        public static bool IsCompatible(SpawnerCatalogEntry spawner, EnemyCatalogEntry enemy)
        {
            if (spawner == null || !spawner.Randomize || enemy == null ||
                !enemy.CanRandomize || enemy.Asset == null)
                return false;

            if (!string.Equals(spawner.PrimaryType, enemy.Type, StringComparison.OrdinalIgnoreCase))
                return false;

            // Only the opt-in Gentlebros experiment may accept aquatic bosses
            // in an otherwise Ground boss slot. All other terrain rules remain intact.
            bool terrainMatches = EnemyCatalogLoader.HasTerrain(enemy, spawner.Terrain);
            bool gentlebrosAquatic =
                string.Equals(spawner.Scene, "Interior_GentlebrosArena", StringComparison.Ordinal) &&
                string.Equals(spawner.PrimaryType, "Boss", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(spawner.Terrain, "Ground", StringComparison.OrdinalIgnoreCase) &&
                HasRestriction(spawner, "AllowAquaticBosses") &&
                EnemyCatalogLoader.HasTerrain(enemy, "Aquatic");

            if (!terrainMatches && !gentlebrosAquatic)
                return false;

            if (EnemyCatalogLoader.HasRestriction(enemy, "NeedsSpikes") &&
                !HasRestriction(spawner, "SpikesAccessible"))
                return false;

            if (EnemyCatalogLoader.HasRestriction(enemy, "Unkillable") &&
                HasRestriction(spawner, "KillRequired"))
                return false;

            // Enemy restriction is location-specific, not a movement test.
            if (EnemyCatalogLoader.HasRestriction(enemy, "2DOnly") &&
                !HasRestriction(spawner, "2DLocation"))
                return false;

            if (EnemyCatalogLoader.HasRestriction(enemy, "CannotMove") &&
                HasRestriction(spawner, "MovementRequired"))
                return false;

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
                    case "PATH": current.Path = value; break;
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
                !entries.ContainsKey(KeyFor(e));
            if (!valid)
            {
                rejected++;
                Plugin.Log.LogWarning("SPAWNER CATALOG | Invalid/duplicate entry: " + e.Scene + "|" + e.SpawnerId);
                return;
            }
            entries.Add(KeyFor(e), e);
            if (e.Randomize) enabled++; else disabled++;
        }
    }
}
