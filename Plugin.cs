using BepInEx;
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [BepInPlugin(
        PluginId,
        PluginName,
        PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginId =
            "Randomizer.CatQuest3";

        public const string PluginName =
            "Cat Quest 3 Randomizer";

        public const string PluginVersion =
            "0.261004";


        internal static Plugin Instance;

        internal static
            BepInEx.Logging.ManualLogSource Log;


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
                    PluginId
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
                "Logic good"
            );
        }
    }
}