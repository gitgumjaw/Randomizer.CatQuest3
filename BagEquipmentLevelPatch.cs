using HarmonyLib;
using static ChestData;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(ChestBehaviour),
        "RandomLootItem"
    )]
    public static class BagEquipmentLevelPatch
    {
        public static void Prefix(
            ChestType ___chestType,
            ref bool ___overrideItemLevel,
            ref int ___itemLevel)
        {
            if (___chestType !=
                    ChestType.Bag ||
                RandomizerState.Settings == null ||
                !RandomizerState.Settings
                    .MatchEquipmentLevelToPlayer)
            {
                return;
            }


            ___overrideItemLevel =
                true;


            ___itemLevel =
                Contexts.sharedInstance
                    .gameState
                    .level
                    .value;
        }
    }
}