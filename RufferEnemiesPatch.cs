
// RufferEnemiesPatch.cs
// Integration: 2026-10-21
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(GameplayHelper),
        "HasMewGameFlag",
        new[] { typeof(MewGameFlags) }
    )]
    public static class RufferEnemiesPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            MewGameFlags flag,
            ref bool __result)
        {
            if (flag != MewGameFlags.RufferEnemies)
                return;

            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;

            if (configuration == null ||
                !configuration.Enabled ||
                !configuration.RufferEnemies)
            {
                return;
            }

            __result = true;
        }
    }
}
