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
            Instance =
                this;

            Log =
                Logger;


            Logger.LogInfo(
                "Cat Quest 3 Randomizer starting."
            );


            Harmony harmony =
                new Harmony(
                    "Randomizer.CatQuest3"
                );


            harmony.PatchAll();


            /*
             * Do not create RandomizerSettings or generate a
             * randomized reward layout here.
             *
             * The active configuration is now chosen after the
             * player commits a new-game save slot, or when an
             * existing save is loaded.
             *
             * RandomizerRuntime translates that save-associated
             * configuration into the existing RandomizerSettings
             * booleans and calls the existing RandomizerGenerator.
             */


            Logger.LogInfo(
                "Harmony patches applied."
            );


            Logger.LogInfo(
                "Playtest Version: 0.1.260921 - Initial Playtest Release"
            );
        }
    }
}
