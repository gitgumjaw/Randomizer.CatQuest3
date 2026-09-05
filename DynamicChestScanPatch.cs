using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(ChestBehaviour), "PlayAppear")]
    public static class DynamicChestScanPatch
    {
        public static void Postfix(ChestBehaviour __instance)
        {
            Plugin.Log.LogInfo(
                "Dynamic chest appeared. Attempting catalog scan."
            );

            CatalogScanner.ScanChest(__instance);
        }
    }
}