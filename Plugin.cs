using BepInEx;
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [BepInPlugin("Randomizer.CatQuest3", "Cat Quest 3 Randomizer", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;

            Logger.LogInfo("Hello from Cat Quest 3 Randomizer!");

            Harmony harmony = new Harmony("Randomizer.CatQuest3");
            harmony.PatchAll();

            Logger.LogInfo("Harmony patches applied.");

        }
    }
}