// Cat Quest III Randomizer | EnemyWorldRandomizerPatch.cs
// Generated: 20261028_092908_UTC | Revision: 09
// Loader baseline: EnemyCatalogLoader_20261028_090955_UTC_v07.cs (unchanged)
// Changes: Macho Dog cannot replace Boar King, Pi-rat King, Meowtallika, or Mister Clean. BossShip weighting preserved.
// Retains MoleHoming restriction, BossShip-slot filtering, Unkillable, all-spawner logic.
// Copy contents into existing EnemyWorldRandomizerPatch.cs class in Visual Studio.
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
                List<EnemyCatalogEntry> pool = EnemyCatalogLoader.EligibleFor(
                    environment, originalEntry.Category, originalBossShipSlot, aquaticWorldSpawn);
                // Mole Squid and both Heartpurroken variants are unsuitable for
                // scripted non-world slots. Only select them for WORLD_SPAWN.
                if (__instance.spawnType != SpawnType.WORLD_SPAWN)
                {
                    pool.RemoveAll(candidate => candidate.UnitConfigName != null &&
                        (candidate.UnitConfigName.IndexOf("MoleHoming", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         string.Equals(candidate.UnitConfigName, "Spherical_Spikeball_MonsterConfig", StringComparison.Ordinal) ||
                         string.Equals(candidate.UnitConfigName, "Spherical_SpikeballBoss_MonsterConfig", StringComparison.Ordinal)));
                }
                // Macho Dog cannot move, so exclude him from encounters that
                // require movement: Boar King, Pi-rat King, Meowtallika, Mr. Clean.
                // Apply BEFORE variation-group selection.
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
                // Only non-BossShip Ordinary aquatic slots discount BossShip candidates.
                // Weight *variation groups*, not individual configs: regular=4 tickets,
                // BossShip=1 ticket. Dedicated BossShip slots retain uniform weights.
                bool discountBossShips =
                    string.Equals(originalEntry.Category, "Ordinary", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(environment, "Aquatic", StringComparison.OrdinalIgnoreCase) &&
                    !originalBossShipSlot;
                string group;
                if (discountBossShips)
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
