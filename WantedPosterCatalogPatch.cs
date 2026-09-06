using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(SpawnWantedPosterReward), "OnEnter")]
    public static class WantedPosterCatalogPatch
    {
        public static void Postfix(SpawnWantedPosterReward __instance)
        {
            GameObject spawnedChest =
                __instance.storeSpawnedChest?.Value;

            if (spawnedChest == null)
            {
                Plugin.Log.LogWarning(
                    "Wanted Poster catalog rescan: spawned chest not found."
                );

                return;
            }

            ChestBehaviour chest =
                spawnedChest.GetComponent<ChestBehaviour>();

            if (chest == null)
            {
                Plugin.Log.LogWarning(
                    "Wanted Poster catalog rescan: " +
                    "ChestBehaviour not found."
                );

                return;
            }

            Plugin.Log.LogInfo(
                "Wanted Poster chest finished populating. " +
                "Rescanning catalog."
            );

            CatalogScanner.ScanChest(chest);
        }
    }
}