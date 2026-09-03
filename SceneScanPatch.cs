using HarmonyLib;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
    public static class SceneScanPatch
    {
        public static void Postfix(
            Scene scene,
            LoadSceneMode mode)
        {
            Plugin.Log.LogInfo(
                $"Scene loaded: {scene.name}"
            );

            CatalogScanner.ScanLoadedChests();
        }
    }
}