// Cat Quest III Randomizer
// File: EnemyWorldRandomizerPatch.cs
// Version: EnemyWorldRandomizerPatch_20261108_v13
// Cutscene TYPE modifier: independent deterministic selection without reading
// or updating boss distribution counters; normal spawners retain v11 behavior.
// Installation: replace the complete existing EnemyWorldRandomizerPatch.cs.
using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(UnitSpawner), nameof(UnitSpawner.PopulateSpawnerEntity))]
    public static class EnemyWorldRandomizerPatch
    {
        // The boss assignment plan is constructed in canonical catalog order.
        // It is not influenced by which spawners Unity happens to load first.
        private static Dictionary<string, string> bossPlan;
        private static int bossPlanSeed;
        private static bool bossPlanReady;

        [HarmonyPrefix]
        public static void Prefix(UnitSpawner __instance)
        {
            RandomizerRunConfiguration run = RandomizerRuntime.CurrentConfiguration;
            if (run == null || !run.Enabled ||
                run.EnemyMode != RandomizerRunConfiguration.EnemySpawnMode.Random ||
                __instance == null || __instance.spawnList == null ||
                __instance.spawnList.Count == 0)
                return;

            string scene = __instance.gameObject.scene.name;
            SpawnerCatalogEntry spawner = SpawnerCatalogLoader.Find(scene, __instance.spawnerId);
            if (spawner == null || !spawner.Randomize) return;

            // Plan depends ONLY on seed + immutable catalog data and enemy pool.
            // This must run before any per-instance selection, including boss spawns.
            if (!bossPlanReady || bossPlanSeed != run.SeedValue)
                BuildBossPlan(run.SeedValue);

            bool isBoss = string.Equals(spawner.PrimaryType, "Boss", StringComparison.OrdinalIgnoreCase);
            bool aquatic = string.Equals(spawner.Terrain, "Aquatic", StringComparison.OrdinalIgnoreCase);
            List<EnemyCatalogEntry> compatible = GetCompatible(
                spawner, __instance.spawnType, false);

            if (compatible.Count == 0)
            {
                Plugin.Log.LogWarning("ENEMY RANDOM | No compatible candidates; vanilla | Scene:" + scene +
                    " | Spawner:" + __instance.spawnerId + " | Type:" + spawner.Type +
                    " | Terrain:" + spawner.Terrain);
                return;
            }

            Dictionary<string, List<EnemyCatalogEntry>> grouped = Group(compatible);
            List<string> groups = new List<string>(grouped.Keys);
            groups.Sort(StringComparer.Ordinal);

            // Choose ONE identity per spawner, not one per table row.
            // The game can roll among spawnList rows on respawn. Assigning
            // every row the same UnitConfig prevents a different enemy from
            // appearing after death even when the game's table draw changes.
            string key = SlotKey(scene, __instance.spawnerId, 0);
            string group = null;
            if (isBoss && !spawner.IsCutscene)
                bossPlan.TryGetValue(key, out group);
            if (group == null || !grouped.ContainsKey(group))
            {
                string salt = spawner.IsCutscene ? "CUTSCENE" :
                    isBoss ? "BOSS_FALLBACK" : "GRUNT";
                uint draw = Hash(SeedKey(run.SeedValue) + "|" + salt + "|" +
                    spawner.PrimaryType + "|" + spawner.Terrain + "|" + key);
                group = groups[(int)(draw % (uint)groups.Count)];
            }
            List<EnemyCatalogEntry> variants = grouped[group];
            variants.Sort((a, b) => StringComparer.Ordinal.Compare(
                a.UnitConfigName, b.UnitConfigName));
            uint variantDraw = Hash(SeedKey(run.SeedValue) + "|VARIANT|" + key);
            EnemyCatalogEntry selected = variants[(int)(variantDraw % (uint)variants.Count)];
            if (selected.Asset == null) return;
            NormalizeAggroForTest(selected.Asset);

            List<SpawnTableItem> updated = new List<SpawnTableItem>(__instance.spawnList);
            int changes = 0;
            for (int i = 0; i < updated.Count; i++)
            {
                SpawnTableItem original = updated[i];
                if (original == null || original.spawnConfig == null) continue;
                if (ReferenceEquals(original.spawnConfig, selected.Asset))
                    continue; // Already assigned; safe on repeated Populate calls.

                SpawnTableItem replacement = new SpawnTableItem(selected.Asset, original.probability);
                replacement.spawnCount = original.spawnCount;
                updated[i] = replacement;
                changes++;
                Plugin.Log.LogInfo("ENEMY RANDOM | " + scene + " | " + __instance.spawnerId +
                    " | SpawnType:" + __instance.spawnType + " | Type:" + spawner.Type +
                    " | Terrain:" + spawner.Terrain +
                    " | Cutscene:" + spawner.IsCutscene + " | Entry:" + i + " | " +
                    original.spawnConfig.name + " -> " + selected.UnitConfigName +
                    " | Count:" + original.spawnCount);
            }
            if (changes > 0) __instance.spawnList = updated;
        }

        private static void BuildBossPlan(int seed)
        {
            Dictionary<string, string> next = new Dictionary<string, string>(StringComparer.Ordinal);
            Dictionary<string, int> groundUses = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> aquaticUses = new Dictionary<string, int>(StringComparer.Ordinal);

            List<SpawnerCatalogEntry> all = SpawnerCatalogLoader.AllEntries();
            all.Sort((a, b) => StringComparer.Ordinal.Compare(
                SlotKey(a.Scene, a.SpawnerId, 0), SlotKey(b.Scene, b.SpawnerId, 0)));

            int eligibleBossSpawners = 0;
            foreach (SpawnerCatalogEntry spawner in all)
            {
                if (!spawner.Randomize || spawner.IsCutscene ||
                    !string.Equals(spawner.PrimaryType, "Boss", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Conservative pool: don't plan with WORLD_SPAWN-only configs,
                // because the actual spawner's SpawnType isn't in the catalog.
                // Every planned identity will therefore remain eligible when
                // the actual spawner is populated.
                List<EnemyCatalogEntry> candidates = GetCompatible(
                    spawner, SpawnType.QUEST_SPAWN, true);
                if (candidates.Count == 0) continue;

                List<string> groups = new List<string>(Group(candidates).Keys);
                groups.Sort(StringComparer.Ordinal);
                Dictionary<string, int> uses =
                    string.Equals(spawner.Terrain, "Aquatic", StringComparison.OrdinalIgnoreCase)
                    ? aquaticUses : groundUses;

                int min = int.MaxValue;
                List<string> ties = new List<string>();
                foreach (string candidate in groups)
                {
                    int count;
                    if (!uses.TryGetValue(candidate, out count)) count = 0;
                    if (count < min)
                    {
                        ties.Clear();
                        min = count;
                        ties.Add(candidate);
                    }
                    else if (count == min) ties.Add(candidate);
                }

                string key = SlotKey(spawner.Scene, spawner.SpawnerId, 0);
                uint draw = Hash(SeedKey(seed) + "|BALANCE|" + spawner.Terrain + "|" + key);
                string chosen = ties[(int)(draw % (uint)ties.Count)];
                next.Add(key, chosen);
                int previous;
                if (!uses.TryGetValue(chosen, out previous)) previous = 0;
                uses[chosen] = previous + 1;
                eligibleBossSpawners++;
            }

            bossPlan = next;
            bossPlanSeed = seed;
            bossPlanReady = true;
            Plugin.Log.LogInfo("ENEMY DISTRIBUTION | Deterministic plan ready | Seed:" +
                SeedKey(seed) + " | BossSpawners:" + eligibleBossSpawners +
                " | Method:CanonicalCatalogOrder");
        }

        private static List<EnemyCatalogEntry> GetCompatible(
            SpawnerCatalogEntry spawner, SpawnType spawnType, bool conservative)
        {
            bool boss = string.Equals(spawner.PrimaryType, "Boss", StringComparison.OrdinalIgnoreCase);
            bool aquatic = string.Equals(spawner.Terrain, "Aquatic", StringComparison.OrdinalIgnoreCase);
            string lookupTerrain = aquatic ? "Aquatic" : "X_Only";
            string category = boss && !aquatic ? "Boss" : "Ordinary";
            bool aquaticBoss = boss && aquatic;
            // Treat unknown catalog planning spawn types conservatively.
            bool aquaticWorld = aquatic && !conservative && spawnType == SpawnType.WORLD_SPAWN;

            List<EnemyCatalogEntry> result = EnemyCatalogLoader.EligibleFor(
                lookupTerrain, category, aquaticBoss, aquaticWorld);
            result.RemoveAll(candidate => !SpawnerCatalogLoader.IsCompatible(spawner, candidate));

            if (conservative || spawnType != SpawnType.WORLD_SPAWN)
            {
                result.RemoveAll(candidate => candidate.UnitConfigName != null &&
                    (candidate.UnitConfigName.IndexOf("MoleHoming", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     string.Equals(candidate.UnitConfigName, "Spherical_Spikeball_MonsterConfig", StringComparison.Ordinal) ||
                     string.Equals(candidate.UnitConfigName, "Spherical_SpikeballBoss_MonsterConfig", StringComparison.Ordinal)));
            }
            return result;
        }

        private static Dictionary<string, List<EnemyCatalogEntry>> Group(List<EnemyCatalogEntry> compatible)
        {
            Dictionary<string, List<EnemyCatalogEntry>> grouped =
                new Dictionary<string, List<EnemyCatalogEntry>>(StringComparer.Ordinal);
            foreach (EnemyCatalogEntry candidate in compatible)
            {
                List<EnemyCatalogEntry> variants;
                if (!grouped.TryGetValue(candidate.Variation, out variants))
                {
                    variants = new List<EnemyCatalogEntry>();
                    grouped.Add(candidate.Variation, variants);
                }
                variants.Add(candidate);
            }
            return grouped;
        }

        private static string SeedKey(int seed)
        {
            return seed.ToString("X8");
        }

        private static string SlotKey(string scene, string spawnerId, int slot)
        {
            return scene + "|" + spawnerId + "|" + slot;
        }

        private static uint Hash(string input)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in input)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return hash;
            }
        }

        // Retain the tested universal aggro normalization from v10.
        private static readonly HashSet<int> AggroTestProcessed = new HashSet<int>();

        private static void NormalizeAggroForTest(UnitConfig config)
        {
            if (config == null || !AggroTestProcessed.Add(config.GetInstanceID())) return;
            bool sensor = TrySetAggroField(config, "sensorRadius", 15f);
            bool aggro = TrySetAggroField(config, "aggroRadius", 10f);
            bool los = TrySetAggroField(config, "overrideAggroLineOfSight", 0);
            bool dontCheckLos = TrySetAggroField(config, "dontCheckForLineOfSightAggro", 0);
            Plugin.Log.LogWarning("UNIVERSAL AGGRO TEST | Config:" + config.name +
                " | Sensor:" + sensor + " | Aggro:" + aggro +
                " | OverrideLOS:" + los + " | DontCheckLOS:" + dontCheckLos);
        }

        private static bool TrySetAggroField(UnitConfig config, string name, object value)
        {
            try
            {
                Type type = config.GetType();
                while (type != null)
                {
                    System.Reflection.FieldInfo field = type.GetField(name,
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.DeclaredOnly);
                    if (field != null)
                    {
                        object converted = field.FieldType.IsEnum
                            ? Enum.ToObject(field.FieldType, value)
                            : Convert.ChangeType(value, field.FieldType);
                        field.SetValue(config, converted);
                        return true;
                    }
                    type = type.BaseType;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("AGGRO TEST ERROR | Field:" + name + " | Error:" + ex.Message);
            }
            return false;
        }
    }
}
