
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(UnitSpawner),
        nameof(UnitSpawner.PopulateSpawnerEntity)
    )]
    public static class EnemyCatalogDiagnosticPatch
    {
        private static bool hasLoggedCatalog = false;

        [HarmonyPrefix]
        public static void Prefix(UnitSpawner __instance)
        {
            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;

            if (configuration == null ||
                !configuration.Enabled ||
                configuration.EnemyMode !=
                    RandomizerRunConfiguration.EnemySpawnMode.Random)
            {
                return;
            }

            // Run once per game process.
            if (hasLoggedCatalog)
                return;

            hasLoggedCatalog = true;

            UnitConfig[] loadedConfigs =
                Resources.FindObjectsOfTypeAll<UnitConfig>();

            int validCount = 0;

            foreach (UnitConfig config in loadedConfigs)
            {
                if (config != null)
                    validCount++;
            }

            Plugin.Log.LogInfo(
                "ENEMY CATALOG | Random mode active" +
                " | Loaded UnitConfigs:" + validCount +
                " | Scene:" + __instance.gameObject.scene.name
            );

            Plugin.Log.LogInfo(
                "ENEMY CATALOG | Read-only diagnostic complete. " +
                "No enemy spawns modified."
            );
        }
    }
}
