using HarmonyLib;
using ProjectStar.Data;
using static ChestData;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(ChestBehaviour), "SpawnItemLoot")]
    public static class BagDropRandomizerPatch
    {
        public static void Postfix(
            ref EquipmentItemData __result,
            ChestType ___chestType)
        {
            // Repeatable enemy equipment bags are randomized
            // only when Equipment & Blueprints randomization
            // is enabled.
            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeEquipment)
            {
                return;
            }

            if (__result == null ||
                ___chestType != ChestType.Bag)
            {
                return;
            }

            EquipmentItemData replacement =
                RandomEquipmentDropPool.GetRandom();

            if (replacement != null)
            {
                __result = replacement;
            }
        }
    }
}