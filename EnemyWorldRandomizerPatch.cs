// Cat Quest III Randomizer
// File: EnemyWorldRandomizerPatch.cs
// Version: EnemyWorldRandomizerPatch_20261108_v17
// v17: Arena-specific isolated 30/30/0/1 aggro clones for all five Tavern Tales arenas;
// Gentlebros aquatic bosses additionally receive agentNavtype = 2.
// All other v14 selections and deterministic boss-distribution planning retained.
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
            // Match individual phase spawners that share scene + spawnerId.
            string hierarchyPath = SpawnerCatalogLoader.GetHierarchyPath(__instance.transform);
            SpawnerCatalogEntry spawner = SpawnerCatalogLoader.Find(
                scene, __instance.spawnerId, hierarchyPath);
            if (spawner == null || !spawner.Randomize) return;

            // Plan depends ONLY on seed + immutable catalog data and enemy pool.
            // This must run before any per-instance selection, including boss spawns.
            if (!bossPlanReady || bossPlanSeed != run.SeedValue)
                BuildBossPlan(run.SeedValue);

            bool isBoss = string.Equals(spawner.PrimaryType, "Boss", StringComparison.OrdinalIgnoreCase);
            bool aquatic = string.Equals(spawner.Terrain, "Aquatic", StringComparison.OrdinalIgnoreCase);
            bool gentlebrosExperiment = IsGentlebrosAquaticExperiment(spawner);
            bool selectedFromAquaticPool = false;
            List<EnemyCatalogEntry> compatible = GetCompatible(
                spawner, __instance.spawnType, false);

            // Keep all ordinary boss assignments exactly as in v14. The
            // Gentlebros experiment alone can choose the aquatic boss pool.
            if (gentlebrosExperiment)
            {
                List<EnemyCatalogEntry> aquaticCandidates =
                    GetGentlebrosAquaticBosses(spawner, __instance.spawnType);

                if (aquaticCandidates.Count > 0 && compatible.Count > 0)
                {
                    uint poolDraw = Hash(SeedKey(run.SeedValue) +
                        "|GENTLEBROS_POOL|" + SlotKey(spawner, 0));
                    selectedFromAquaticPool = (poolDraw % 2u) == 0u;
                }
                else
                {
                    selectedFromAquaticPool = aquaticCandidates.Count > 0;
                }

                if (selectedFromAquaticPool)
                    compatible = aquaticCandidates;
            }

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
            string key = SlotKey(spawner, 0);
            string group = null;
            // Experimental Gentlebros draws do not consume or depend on the
            // normal ground/aquatic boss distribution plan.
            if (isBoss && !spawner.IsCutscene && !gentlebrosExperiment)
                bossPlan.TryGetValue(key, out group);
            if (group == null || !grouped.ContainsKey(group))
            {
                string salt = gentlebrosExperiment
                    ? (selectedFromAquaticPool ? "GENTLEBROS_AQUATIC" : "GENTLEBROS_GROUND")
                    : spawner.IsCutscene ? "CUTSCENE" :
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

            // All five Tavern Tales arenas receive isolated 30/30/0/1 aggro
            // clones. Only experimental Gentlebros aquatic bosses get nav 2.
            UnitConfig assignedAsset = selected.Asset;
            if (IsTavernTalesArena(scene))
            {
                assignedAsset = GetTavernTalesArenaClone(
                    selected.Asset, gentlebrosExperiment && selectedFromAquaticPool);
                if (assignedAsset == null)
                {
                    Plugin.Log.LogWarning("TAVERN TALES AGGRO | Clone failed; original spawn table retained | " + key);
                    return;
                }
            }

            List<SpawnTableItem> updated = new List<SpawnTableItem>(__instance.spawnList);
            int changes = 0;
            for (int i = 0; i < updated.Count; i++)
            {
                SpawnTableItem original = updated[i];
                if (original == null || original.spawnConfig == null) continue;
                if (ReferenceEquals(original.spawnConfig, assignedAsset))
                    continue; // Already assigned; safe on repeated Populate calls.

                SpawnTableItem replacement = new SpawnTableItem(assignedAsset, original.probability);
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

        private static bool IsGentlebrosAquaticExperiment(SpawnerCatalogEntry spawner)
        {
            return spawner != null &&
                string.Equals(spawner.Scene, "Interior_GentlebrosArena", StringComparison.Ordinal) &&
                string.Equals(spawner.PrimaryType, "Boss", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(spawner.Terrain, "Ground", StringComparison.OrdinalIgnoreCase) &&
                SpawnerCatalogLoader.HasRestriction(spawner, "AllowAquaticBosses");
        }

        private static List<EnemyCatalogEntry> GetGentlebrosAquaticBosses(
            SpawnerCatalogEntry spawner, SpawnType spawnType)
        {
            List<EnemyCatalogEntry> result = EnemyCatalogLoader.EligibleFor(
                "Aquatic", "Ordinary", true, false);
            result.RemoveAll(candidate => !SpawnerCatalogLoader.IsCompatible(spawner, candidate));

            // Same non-WORLD encounter safeguards as the normal randomizer.
            if (spawnType != SpawnType.WORLD_SPAWN)
            {
                result.RemoveAll(candidate => candidate.UnitConfigName != null &&
                    (candidate.UnitConfigName.IndexOf("MoleHoming", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     string.Equals(candidate.UnitConfigName, "Spherical_Spikeball_MonsterConfig", StringComparison.Ordinal) ||
                     string.Equals(candidate.UnitConfigName, "Spherical_SpikeballBoss_MonsterConfig", StringComparison.Ordinal)));
            }
            return result;
        }

        private static bool IsTavernTalesArena(string scene)
        {
            return string.Equals(scene, "Interior_GentlebrosArena", StringComparison.Ordinal) ||
                string.Equals(scene, "TavernTalesArena_BoarBoss", StringComparison.Ordinal) ||
                string.Equals(scene, "TavernTalesArena_MewtallicaBoss", StringComparison.Ordinal) ||
                string.Equals(scene, "TavernTalesArena_PirateKing", StringComparison.Ordinal) ||
                string.Equals(scene, "TavernTalesArena_SpicySquidTakomeowkiBoss", StringComparison.Ordinal);
        }

        // One isolated configuration per source asset AND navigation mode.
        // The 30/30/0/1 clones can be reused across Tavern Tales arenas,
        // while Gentlebros aquatic navigation remains an independent variant.
        private static readonly Dictionary<string, UnitConfig> tavernTalesArenaClones =
            new Dictionary<string, UnitConfig>(StringComparer.Ordinal);

        private static UnitConfig GetTavernTalesArenaClone(UnitConfig original, bool aquaticNav)
        {
            if (original == null) return null;
            string cacheKey = original.GetInstanceID().ToString() + "|" + (aquaticNav ? "NAV2" : "DEFAULT");
            UnitConfig clone;
            if (tavernTalesArenaClones.TryGetValue(cacheKey, out clone) && clone != null)
                return clone;

            try
            {
                clone = UnityEngine.Object.Instantiate(original);
                if (clone == null) return null;
                clone.name = original.name;

                // All four values must be written successfully or this
                // experimental spawner remains vanilla (fail closed).
                bool sensor = TrySetAggroField(clone, "sensorRadius", 30f);
                bool aggro = TrySetAggroField(clone, "aggroRadius", 30f);
                bool dontCheckLos = TrySetAggroField(clone, "dontCheckForLineOfSightAggro", 0);
                bool overrideLos = TrySetAggroField(clone, "overrideAggroLineOfSight", 1);
                bool nav = !aquaticNav || TrySetAggroField(clone, "agentNavtype", 2);

                if (!sensor || !aggro || !dontCheckLos || !overrideLos || !nav)
                {
                    UnityEngine.Object.Destroy(clone);
                    Plugin.Log.LogWarning("TAVERN TALES AGGRO | Clone setup failed | Config:" +
                        original.name + " | Sensor:" + sensor + " | Aggro:" + aggro +
                        " | DontCheckLOS:" + dontCheckLos + " | OverrideLOS:" + overrideLos +
                        " | Nav:" + nav);
                    return null;
                }

                tavernTalesArenaClones[cacheKey] = clone;
                Plugin.Log.LogInfo("TAVERN TALES AGGRO | Cloned " + original.name +
                    " | sensorRadius:30 | aggroRadius:30" +
                    " | dontCheckForLineOfSightAggro:0 | overrideAggroLineOfSight:1" +
                    (aquaticNav ? " | agentNavtype:2" : " | agentNavtype:unchanged") +
                    " | Original unchanged");
                return clone;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("TAVERN TALES AGGRO | Clone error: " + ex);
                return null;
            }
        }

        private static void BuildBossPlan(int seed)
        {
            Dictionary<string, string> next = new Dictionary<string, string>(StringComparer.Ordinal);
            Dictionary<string, int> groundUses = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> aquaticUses = new Dictionary<string, int>(StringComparer.Ordinal);

            List<SpawnerCatalogEntry> all = SpawnerCatalogLoader.AllEntries();
            all.Sort((a, b) => StringComparer.Ordinal.Compare(
                SlotKey(a, 0), SlotKey(b, 0)));

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

                string key = SlotKey(spawner, 0);
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

        private static string SlotKey(SpawnerCatalogEntry spawner, int slot)
        {
            // Preserve the exact original key for every legacy entry.
            // Only PATH-based spawners get a distinct seeded identity.
            return SpawnerCatalogLoader.KeyFor(spawner) + "|" + slot;
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
