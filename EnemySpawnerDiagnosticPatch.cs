
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(UnitSpawner),
        nameof(UnitSpawner.PopulateSpawnerEntity)
    )]
    public static class EnemySpawnerDiagnosticPatch
    {
        [HarmonyPrefix]
        public static void Prefix(UnitSpawner __instance)
        {
            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;

            if (configuration == null ||
                !configuration.Enabled ||
                !configuration.RufferEnemies)
            {
                return;
            }

            // Limit the first test to ordinary world spawners.
            if (__instance.spawnType != SpawnType.WORLD_SPAWN)
                return;

            string sceneName =
                __instance.gameObject.scene.name;

            bool usesRuffer =
                __instance.enableOverrides &&
                __instance.rufferSpawnList != null &&
                __instance.rufferSpawnList.Count > 0;

            Plugin.Log.LogInfo(
                "ENEMY SPAWNER | Scene: " + sceneName +
                " | ID: " + __instance.spawnerId +
                " | Ruffer Override: " + usesRuffer +
                " | Vanilla Entries: " +
                    (__instance.spawnList?.Count ?? 0) +
                " | Ruffer Entries: " +
                    (__instance.rufferSpawnList?.Count ?? 0)
            );

            LogEntries("VANILLA", __instance.spawnList);

            if (usesRuffer)
            {
                LogEntries(
                    "RUFFER",
                    __instance.rufferSpawnList
                );
            }
        }

        private static void LogEntries(
            string listName,
            List<SpawnTableItem> entries)
        {
            if (entries == null)
                return;

            for (int i = 0; i < entries.Count; i++)
            {
                SpawnTableItem entry = entries[i];

                string enemyName =
                    entry?.spawnConfig != null
                        ? entry.spawnConfig.name
                        : "(null)";

                Plugin.Log.LogInfo(
                    "ENEMY ENTRY | " + listName +
                    " | Index: " + i +
                    " | Enemy: " + enemyName +
                    " | Probability: " +
                        (entry != null ? entry.probability : 0f) +
                    " | Count: " +
                        (entry != null ? entry.spawnCount : 0)
                );
            }
        }
    }
}
