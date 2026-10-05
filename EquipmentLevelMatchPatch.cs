using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(GameContextExtensions),
        "CreateAddEquipmentCommand"
    )]
    public static class EquipmentLevelMatchPatch
    {
        public static void Prefix(
            ref int level)
        {
            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings
                    .MatchEquipmentLevelToPlayer)
            {
                return;
            }


            int originalLevel =
                level;


            int playerLevel =
                Contexts.sharedInstance
                    .gameState
                    .level
                    .value;


            level =
                playerLevel;


            Plugin.Log.LogInfo(
                "EQUIPMENT LEVEL MATCH | " +
                "Original:" +
                originalLevel +
                " | Player:" +
                playerLevel
            );
        }
    }
}