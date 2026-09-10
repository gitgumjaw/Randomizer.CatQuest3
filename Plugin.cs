using BepInEx;
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [BepInPlugin(
        "Randomizer.CatQuest3",
        "Cat Quest 3 Randomizer",
        "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Logger.LogInfo(
                "Hello from Cat Quest 3 Randomizer!"
            );

            Logger.LogInfo(
                "Cat Quest 3 Randomizer DEBUG BUILD: Category Label Maker 1"
            );

            Harmony harmony =
                new Harmony(
                    "Randomizer.CatQuest3"
                );

            harmony.PatchAll();

            var shipBlueprintMethod =
    AccessTools.Method(
        typeof(AwardShipBlueprintToPlayer),
        "OnEnter"
    );

            var shipBlueprintPatchInfo =
                Harmony.GetPatchInfo(
                    shipBlueprintMethod
                );

            bool shipBlueprintPatchFound = false;

            if (shipBlueprintPatchInfo != null)
            {
                foreach (string owner in shipBlueprintPatchInfo.Owners)
                {
                    if (owner == "Randomizer.CatQuest3")
                    {
                        shipBlueprintPatchFound = true;
                        break;
                    }
                }
            }

            Logger.LogInfo(
                $"SHIP BLUEPRINT PATCH INSTALLED: " +
                $"{shipBlueprintPatchFound}"
            );

            Logger.LogInfo(
                "Harmony patches applied."
            );

            CatalogExporter.UpgradeExistingCatalogMetadata();
        }

        private void LateUpdate()
        {
            if (!CatalogScanResults.IsDirty)
            {
                return;
            }

            CatalogExporter.Export();
        }
    }
}