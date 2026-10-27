
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(UnitSpawner),
        nameof(UnitSpawner.PopulateSpawnerEntity)
    )]
    public static class EnemySpawnerReplacementTestPatch
    {
        private const string TestScene = "Cave_FurstCave";

        private const string TestSpawnerId =
            "1803977248-59de0216614ad9041a06a15ad0cf3851";

        private const string ReplacementName =
            "Flying_Parrot_MonsterConfig";


        public static void Prefix(UnitSpawner __instance)
        {
            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;

            if (configuration == null ||
                !configuration.Enabled ||
                configuration.EnemyMode !=
                    RandomizerRunConfiguration.EnemySpawnMode.Ruffer)
            {
                return;
            }

            if (__instance.gameObject.scene.name != TestScene ||
                __instance.spawnerId != TestSpawnerId)
            {
                return;
            }

            if (!__instance.enableOverrides ||
                __instance.rufferSpawnList == null ||
                __instance.rufferSpawnList.Count == 0)
            {
                Plugin.Log.LogWarning(
                    "ENEMY TEST | Ruffer table unavailable."
                );
                return;
            }

            // Search the UnitConfig assets currently loaded
            // into Unity's memory.
            UnitConfig replacementConfig = null;

            UnitConfig[] loadedConfigs =
                Resources.FindObjectsOfTypeAll<UnitConfig>();

            foreach (UnitConfig config in loadedConfigs)
            {
                if (config != null &&
                    config.name == ReplacementName)
                {
                    replacementConfig = config;
                    break;
                }
            }

            Plugin.Log.LogInfo(
                "ENEMY TEST | Loaded UnitConfigs: " +
                loadedConfigs.Length
            );

            if (replacementConfig == null)
            {
                Plugin.Log.LogWarning(
                    "ENEMY TEST | Could not find " +
                    ReplacementName +
                    " in loaded assets."
                );
                return;
            }

            SpawnTableItem original =
                __instance.rufferSpawnList[0];

            if (original == null ||
                original.spawnConfig == null)
            {
                Plugin.Log.LogWarning(
                    "ENEMY TEST | Original entry unavailable."
                );
                return;
            }

            SpawnTableItem replacement =
                new SpawnTableItem(
                    replacementConfig,
                    original.probability
                );

            replacement.spawnCount =
                original.spawnCount;

            // Preserve the original list and table item.
            List<SpawnTableItem> replacementList =
                new List<SpawnTableItem>(
                    __instance.rufferSpawnList
                );

            replacementList[0] = replacement;

            __instance.rufferSpawnList =
                replacementList;

            Plugin.Log.LogInfo(
                "ENEMY TEST | CROSS-SPAWNER REPLACEMENT" +
                " | Scene: " + TestScene +
                " | Original: " +
                    original.spawnConfig.name +
                " | Replacement: " +
                    replacementConfig.name +
                " | Count: " +
                    replacement.spawnCount
            );
        }
    }
}
