// Cat Quest III Randomizer | EnemyWorldRandomizerPatch.cs
// Generated: 20261102_082520_UTC | Revision: 10 (distribution experiment)
// Baseline: committed v09; EnemyCatalogLoader v07 unchanged.
// Changes: shared ground Boss+MiniBoss non-repeating variation distribution;
//          X_Only bosses allowed into Ground; non-repeating dedicated BossShip slots.
//          Existing restrictions, normal-ship weighting and WORLD-only guards retained.
// Copy contents into existing EnemyWorldRandomizerPatch.cs in Visual Studio.
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(UnitSpawner), nameof(UnitSpawner.PopulateSpawnerEntity))]
    public static class EnemyWorldRandomizerPatch
    {
        // Prevent editing the same spawner's already-randomized list twice.
        private static readonly HashSet<int> processed = new HashSet<int>();
        // The usage counts are shared across all spawners during one loaded run.
        // Variation is the identity: multiple UnitConfigs do not get extra copies.
        private static readonly Dictionary<string, int> landBossUses = new Dictionary<string, int>(StringComparer.Ordinal);
        private static readonly Dictionary<string, int> bossShipUses = new Dictionary<string, int>(StringComparer.Ordinal);
        private static RandomizerRunConfiguration currentRun;
        private static int currentSeed;

        [HarmonyPrefix]
        public static void Prefix(UnitSpawner __instance)
        {
            RandomizerRunConfiguration run = RandomizerRuntime.CurrentConfiguration;
            if (run == null || !run.Enabled ||
                run.EnemyMode != RandomizerRunConfiguration.EnemySpawnMode.Random)
                return;
            if (__instance == null ||
                __instance.spawnList == null || __instance.spawnList.Count == 0)
                return;
            // Reset state when a new profile/configuration becomes active.
            if (!object.ReferenceEquals(currentRun, run) || currentSeed != run.SeedValue)
            {
                processed.Clear();
                landBossUses.Clear();
                bossShipUses.Clear();
                currentRun = run;
                currentSeed = run.SeedValue;
                Plugin.Log.LogInfo("ENEMY DISTRIBUTION | Reset for seed:" + run.SeedValue.ToString("X8"));
            }
            int instanceId = __instance.GetInstanceID();
            if (processed.Contains(instanceId)) return;

            List<SpawnTableItem> updated = new List<SpawnTableItem>(__instance.spawnList);
            int changes = 0;
            string scene = __instance.gameObject.scene.name;
            for (int i = 0; i < updated.Count; i++)
            {
                SpawnTableItem original = updated[i];
                if (original == null || original.spawnConfig == null) continue;
                EnemyCatalogEntry originalEntry = EnemyCatalogLoader.Find(original.spawnConfig.name);
                // Preserve the role: Ordinary -> Ordinary, MiniBoss -> MiniBoss, Boss -> Boss.
                if (originalEntry == null ||
                    (!string.Equals(originalEntry.Category, "Ordinary", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(originalEntry.Category, "MiniBoss", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(originalEntry.Category, "Boss", StringComparison.OrdinalIgnoreCase)))
                    continue;
                string environment = originalEntry.Environment;
                // Explicitly special environments aren't randomized.
                if (string.Equals(environment, "Special", StringComparison.OrdinalIgnoreCase)) continue;
                // Current catalog convention: vanilla All enemies spawn on land,
                // except in Squeaky's Cave, which needs a scene-specific rule.
                if (string.Equals(environment, "All", StringComparison.OrdinalIgnoreCase))
                {
                    // Until that exception is mapped, preserve those spawns.
                    if (scene.IndexOf("Squeaky", StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;
                    environment = "Ground";
                }

                // Restrictions are based on the ORIGINAL spawn slot, not candidate identity.
                bool originalBossShipSlot = EnemyCatalogLoader.HasRestriction(originalEntry, "BossShip");
                bool aquaticWorldSpawn =
                    string.Equals(environment, "Aquatic", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(__instance.spawnType.ToString(), "WORLD_SPAWN", StringComparison.OrdinalIgnoreCase);
                bool isLandBoss =
                    (string.Equals(originalEntry.Category, "Boss", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(originalEntry.Category, "MiniBoss", StringComparison.OrdinalIgnoreCase)) &&
                    (string.Equals(environment, "Ground", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(environment, "X_Only", StringComparison.OrdinalIgnoreCase));
                bool isDedicatedBossShip = originalBossShipSlot &&
                    string.Equals(originalEntry.Category, "Ordinary", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(environment, "Aquatic", StringComparison.OrdinalIgnoreCase);

                List<EnemyCatalogEntry> pool;
                if (isLandBoss)
                {
                    // X_Only lookup contains both Ground and X_Only candidates.
                    // Combine categories, including in 3D Ground boss slots, as a test.
                    pool = EnemyCatalogLoader.EligibleFor("X_Only", "Boss");
                    pool.AddRange(EnemyCatalogLoader.EligibleFor("X_Only", "MiniBoss"));
                }
                else
                {
                    pool = EnemyCatalogLoader.EligibleFor(
                        environment, originalEntry.Category, originalBossShipSlot, aquaticWorldSpawn);
                }
                // Mole Squid and both Heartpurroken variants are unsuitable for
                // scripted non-world slots. Only select them for WORLD_SPAWN.
                if (__instance.spawnType != SpawnType.WORLD_SPAWN)
                {
                    pool.RemoveAll(candidate => candidate.UnitConfigName != null &&
                        (candidate.UnitConfigName.IndexOf("MoleHoming", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         string.Equals(candidate.UnitConfigName, "Spherical_Spikeball_MonsterConfig", StringComparison.Ordinal) ||
                         string.Equals(candidate.UnitConfigName, "Spherical_SpikeballBoss_MonsterConfig", StringComparison.Ordinal)));
                }
                // Macho Dog cannot move; preserve v09's source-specific exclusions.
                string sourceName = original.spawnConfig.name;
                bool requiresMobileBoss =
                    sourceName.StartsWith("Quadruped_BoarBoss_", StringComparison.OrdinalIgnoreCase) ||
                    sourceName.StartsWith("Enemy_HumanoidBoss_PirateKing_", StringComparison.OrdinalIgnoreCase) ||
                    sourceName.StartsWith("Enemy_HumanoidBoss_Mewtallica_", StringComparison.OrdinalIgnoreCase) ||
                    sourceName.StartsWith("HumanoidBoss_Mewtallica_", StringComparison.OrdinalIgnoreCase) ||
                    sourceName.StartsWith("Grunt_ChubbyBoss_Clean_", StringComparison.OrdinalIgnoreCase);
                if (requiresMobileBoss)
                {
                    pool.RemoveAll(candidate => string.Equals(
                        candidate.UnitConfigName, "MachoDog_Boss_MonsterConfig", StringComparison.OrdinalIgnoreCase));
                }
                if (pool.Count == 0) continue;
                Dictionary<string, List<EnemyCatalogEntry>> grouped =
                    new Dictionary<string, List<EnemyCatalogEntry>>(StringComparer.Ordinal);
                foreach (EnemyCatalogEntry candidate in pool)
                {
                    List<EnemyCatalogEntry> variations;
                    if (!grouped.TryGetValue(candidate.Variation, out variations))
                    {
                        variations = new List<EnemyCatalogEntry>();
                        grouped.Add(candidate.Variation, variations);
                    }
                    variations.Add(candidate);
                }
                List<string> groupNames = new List<string>(grouped.Keys);
                groupNames.Sort(StringComparer.Ordinal);
                uint seed = Hash(run.SeedValue.ToString("X8") + "|" + originalEntry.Category + "|" + scene + "|" +
                                 __instance.spawnerId + "|" + i);
                // For the two special pools, pick among the least-used *eligible*
                // identities. With unrestricted slots, every identity gets one turn
                // before any repeat. Restricted slots may need an already-used one.
                Dictionary<string, int> usage = isLandBoss ? landBossUses :
                    (isDedicatedBossShip ? bossShipUses : null);
                string distributionGroup = null;
                if (usage != null)
                {
                    int minimum = int.MaxValue;
                    List<string> leastUsed = new List<string>();
                    foreach (string name in groupNames)
                    {
                        int used;
                        if (!usage.TryGetValue(name, out used)) used = 0;
                        if (used < minimum)
                        {
                            minimum = used;
                            leastUsed.Clear();
                            leastUsed.Add(name);
                        }
                        else if (used == minimum) leastUsed.Add(name);
                    }
                    uint draw = Hash(run.SeedValue.ToString("X8") + "|DISTRIBUTE|" +
                                     (isLandBoss ? "LAND_BOSS" : "BOSS_SHIP") + "|" +
                                     scene + "|" + __instance.spawnerId + "|" + i +
                                     "|" + minimum);
                    distributionGroup = leastUsed[(int)(draw % (uint)leastUsed.Count)];
                }
                // Only non-BossShip Ordinary aquatic slots discount BossShip candidates.
                // Weight *variation groups*, not individual configs: regular=4 tickets,
                // BossShip=1 ticket. Dedicated BossShip slots retain uniform weights.
                bool discountBossShips =
                    string.Equals(originalEntry.Category, "Ordinary", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(environment, "Aquatic", StringComparison.OrdinalIgnoreCase) &&
                    !originalBossShipSlot;
                string group;
                if (distributionGroup != null)
                {
                    group = distributionGroup;
                }
                else if (discountBossShips)
                {
                    int totalTickets = 0;
                    foreach (string name in groupNames)
                    {
                        // Mixed groups are treated as BossShip groups conservatively.
                        bool containsBossShip = grouped[name].Exists(candidate =>
                            EnemyCatalogLoader.HasRestriction(candidate, "BossShip"));
                        totalTickets += containsBossShip ? 1 : 4;
                    }
                    int ticket = (int)(seed % (uint)totalTickets);
                    group = groupNames[groupNames.Count - 1];
                    foreach (string name in groupNames)
                    {
                        bool containsBossShip = grouped[name].Exists(candidate =>
                            EnemyCatalogLoader.HasRestriction(candidate, "BossShip"));
                        int weight = containsBossShip ? 1 : 4;
                        if (ticket < weight)
                        {
                            group = name;
                            break;
                        }
                        ticket -= weight;
                    }
                }
                else
                {
                    group = groupNames[(int)(seed % (uint)groupNames.Count)];
                }
                List<EnemyCatalogEntry> variationsForGroup = grouped[group];
                variationsForGroup.Sort((a, b) => StringComparer.Ordinal.Compare(a.UnitConfigName, b.UnitConfigName));
                uint variantSeed = Hash(run.SeedValue.ToString("X8") + "|VARIANT|" + scene +
                                        "|" + __instance.spawnerId + "|" + i);
                EnemyCatalogEntry selected = variationsForGroup[(int)(variantSeed % (uint)variationsForGroup.Count)];
                if (selected.Asset == null) continue;
                SpawnTableItem replacement = new SpawnTableItem(selected.Asset, original.probability);
                replacement.spawnCount = original.spawnCount;
                updated[i] = replacement;
                changes++;
                if (usage != null)
                {
                    int previous;
                    if (!usage.TryGetValue(group, out previous)) previous = 0;
                    usage[group] = previous + 1;
                    Plugin.Log.LogInfo("ENEMY DISTRIBUTION | Pool:" +
                        (isLandBoss ? "LandBoss" : "BossShip") +
                        " | Identity:" + group + " | Uses:" + usage[group] +
                        " | EligibleIdentities:" + groupNames.Count +
                        " | Scene:" + scene + " | Spawner:" + __instance.spawnerId +
                        " | Entry:" + i);
                }
                Plugin.Log.LogInfo("ENEMY RANDOM | " + scene + " | " + __instance.spawnerId +
                    " | SpawnType:" + __instance.spawnType + " | Category:" + originalEntry.Category + " | Entry:" + i + " | " + original.spawnConfig.name +
                    " -> " + selected.UnitConfigName + " | Count:" + original.spawnCount);
            }
            if (changes > 0) __instance.spawnList = updated;
            processed.Add(instanceId);
        }

        // Stable across separate game launches; unlike string.GetHashCode().
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
    }
}
