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
                "Cat Quest 3 Randomizer starting."
            );

            Harmony harmony =
                new Harmony(
                    "Randomizer.CatQuest3"
                );

            harmony.PatchAll();

            RandomizerSettings settings =
                new RandomizerSettings
                {
                    RandomizeEquipment = true,
                    RandomizeSpells = true,
                    RandomizeQuestItems = true,
                    RandomizeManaCrystals = true,
                    RandomizeCollectibles = true,

                    RandomizeShipKey = false,
                    RandomizeInfinityKey = false,
                    RandomizeNorthStarEssence = false,
                    RandomizeBirdPoop = false
                };

            RandomizerState.Settings =
                settings;

            RandomizerGenerator.Generate(
                settings,
                54321
            );

            Logger.LogInfo(
                "Harmony patches applied."
            );

            Logger.LogInfo(
                "Sanity Check: Rando test 1"
            );
        }
    }
}