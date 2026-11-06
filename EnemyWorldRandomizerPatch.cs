// Cat Quest III Randomizer
// File: EnemyWorldRandomizerPatch.cs
// Version: EnemyWorldRandomizerPatch_20261105_2253_EST_v12
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
        private static readonly HashSet<int> processed = new HashSet<int>();
        private static readonly Dictionary<string, int> groundBossUses =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private static readonly Dictionary<string, int> aquaticBossUses =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private static RandomizerRunConfiguration currentRun;
        private static int currentSeed;

        [HarmonyPrefix]
        public static void Prefix(UnitSpawner __instance)
        {
            RandomizerRunConfiguration run = RandomizerRuntime.CurrentConfiguration;
            if (run == null || !run.Enabled ||
                run.EnemyMode != RandomizerRunConfiguration.EnemySpawnMode.Random ||
                __instance == null || __instance.spawnList == null ||
                __instance.spawnList.Count == 0)
                return;

            if (!ReferenceEquals(currentRun, run) || currentSeed != run.SeedValue)
            {
                processed.Clear();
                groundBossUses.Clear();
                aquaticBossUses.Clear();
                currentRun = run;
                currentSeed = run.SeedValue;
                Plugin.Log.LogInfo("ENEMY DISTRIBUTION | Reset for seed:" + run.SeedValue.ToString("X8"));
            }

            int instanceId = __instance.GetInstanceID();
            if (processed.Contains(instanceId)) return;

            string scene = __instance.gameObject.scene.name;
            SpawnerCatalogEntry spawner = SpawnerCatalogLoader.Find(scene, __instance.spawnerId);
            // Unknown or explicitly disabled spawners are left completely vanilla.
            if (spawner == null || !spawner.Randomize) return;

            // The v08 Enemy Catalog loader still accepts these legacy lookup
            // arguments. Convert our new two-axis classification only here.
            bool isBoss = string.Equals(spawner.PrimaryType, "Boss", StringComparison.OrdinalIgnoreCase);
            bool aquatic = string.Equals(spawner.Terrain, "Aquatic", StringComparison.OrdinalIgnoreCase);
            string lookupTerrain = aquatic ? "Aquatic" : "X_Only";
            string lookupCategory = isBoss && !aquatic ? "Boss" : "Ordinary";
            bool aquaticBoss = isBoss && aquatic;
            bool aquaticWorld = aquatic && __instance.spawnType == SpawnType.WORLD_SPAWN;

            List<EnemyCatalogEntry> compatible = EnemyCatalogLoader.EligibleFor(
                lookupTerrain, lookupCategory, aquaticBoss, aquaticWorld);
            compatible.RemoveAll(candidate =>
                !SpawnerCatalogLoader.IsCompatible(spawner, candidate));

            // Preserve the previous WORLD_SPAWN-only safeguard until these
            // enemies' scripted encounter behavior has been reviewed.
            if (__instance.spawnType != SpawnType.WORLD_SPAWN)
            {
                compatible.RemoveAll(candidate => candidate.UnitConfigName != null &&
                    (candidate.UnitConfigName.IndexOf("MoleHoming", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     string.Equals(candidate.UnitConfigName, "Spherical_Spikeball_MonsterConfig", StringComparison.Ordinal) ||
                     string.Equals(candidate.UnitConfigName, "Spherical_SpikeballBoss_MonsterConfig", StringComparison.Ordinal)));
            }

            if (compatible.Count == 0)
            {
                Plugin.Log.LogWarning("ENEMY RANDOM | No compatible candidates; vanilla | Scene:" + scene +
                    " | Spawner:" + __instance.spawnerId + " | Type:" + spawner.Type +
                    " | Terrain:" + spawner.Terrain);
                processed.Add(instanceId);
                return;
            }

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
            List<string> groups = new List<string>(grouped.Keys);
            groups.Sort(StringComparer.Ordinal);

            List<SpawnTableItem> updated = new List<SpawnTableItem>(__instance.spawnList);
            int changes = 0;
            for (int i = 0; i < updated.Count; i++)
            {
                SpawnTableItem original = updated[i];
                if (original == null || original.spawnConfig == null) continue;

                uint seed = Hash(run.SeedValue.ToString("X8") + "|" + spawner.PrimaryType + "|" +
                    spawner.Terrain + "|" + scene + "|" + __instance.spawnerId + "|" + i);

                Dictionary<string, int> usage = (!isBoss || spawner.IsCutscene) ? null :
                    (aquatic ? aquaticBossUses : groundBossUses);
                string group;
                if (usage == null)
                {
                    // Cutscene draws are independent from combat distribution.
                    // Include a stable salt so their results remain distinct from
                    // ordinary Grunt draws without using distribution counters.
                    uint independentSeed = spawner.IsCutscene
                        ? Hash(run.SeedValue.ToString("X8") + "|CUTSCENE|" +
                               spawner.PrimaryType + "|" + spawner.Terrain + "|" +
                               scene + "|" + __instance.spawnerId + "|" + i)
                        : seed;
                    group = groups[(int)(independentSeed % (uint)groups.Count)];
                }
                else
                {
                    int minimum = int.MaxValue;
                    List<string> leastUsed = new List<string>();
                    foreach (string name in groups)
                    {
                        int count;
                        if (!usage.TryGetValue(name, out count)) count = 0;
                        if (count < minimum)
                        {
                            minimum = count;
                            leastUsed.Clear();
                            leastUsed.Add(name);
                        }
                        else if (count == minimum)
                        {
                            leastUsed.Add(name);
                        }
                    }
                    uint draw = Hash(run.SeedValue.ToString("X8") + "|DISTRIBUTE|" +
                        spawner.Terrain + "|" + scene + "|" + __instance.spawnerId +
                        "|" + i + "|" + minimum);
                    group = leastUsed[(int)(draw % (uint)leastUsed.Count)];
                }

                List<EnemyCatalogEntry> variantsForGroup = grouped[group];
                variantsForGroup.Sort((a, b) =>
                    StringComparer.Ordinal.Compare(a.UnitConfigName, b.UnitConfigName));
                uint variantSeed = Hash(run.SeedValue.ToString("X8") + "|VARIANT|" +
                    scene + "|" + __instance.spawnerId + "|" + i);
                EnemyCatalogEntry selected = variantsForGroup[
                    (int)(variantSeed % (uint)variantsForGroup.Count)];
                if (selected.Asset == null) continue;

                NormalizeAggroForTest(selected.Asset);

                SpawnTableItem replacement = new SpawnTableItem(selected.Asset, original.probability);
                replacement.spawnCount = original.spawnCount;
                updated[i] = replacement;
                changes++;

                if (usage != null)
                {
                    int count;
                    if (!usage.TryGetValue(group, out count)) count = 0;
                    usage[group] = count + 1;
                    Plugin.Log.LogInfo("ENEMY DISTRIBUTION | Pool:" + spawner.Terrain + "Boss" +
                        " | Identity:" + group + " | Uses:" + usage[group] +
                        " | EligibleIdentities:" + groups.Count +
                        " | Scene:" + scene + " | Spawner:" + __instance.spawnerId + " | Entry:" + i);
                }
                Plugin.Log.LogInfo("ENEMY RANDOM | " + scene + " | " + __instance.spawnerId +
                    " | SpawnType:" + __instance.spawnType + " | Type:" + spawner.Type +
                    " | Terrain:" + spawner.Terrain +
                    " | Cutscene:" + spawner.IsCutscene + " | Entry:" + i + " | " +
                    original.spawnConfig.name + " -> " + selected.UnitConfigName +
                    " | Count:" + original.spawnCount);
            }

            if (changes > 0) __instance.spawnList = updated;
            processed.Add(instanceId);
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
