using HarmonyLib;
using ProjectStar.Data;
using static ChestData;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(ChestBehaviour),
        "SpawnItemLoot"
    )]
    public static class BagDropRandomizerPatch
    {
        private const string BirdPoopGuid =
            "321ebd701eff1264eb7f7296d033831c";


        public static void Postfix(
            ref EquipmentItemData __result,
            ChestType ___chestType)
        {
            if (__result == null ||
                ___chestType != ChestType.Bag)
            {
                return;
            }

            if (RandomizerState.Settings == null)
            {
                return;
            }


            bool isBirdPoop =
                __result.Guid ==
                BirdPoopGuid;


            // Bird Poop has its own toggle.
            //
            // OFF:
            // Leave the Bird Poop bag completely vanilla.
            //
            // ON:
            // Treat it like a repeatable randomized bag drop.
            if (isBirdPoop)
            {
                if (!RandomizerState.Settings
                    .RandomizeBirdPoop)
                {
                    return;
                }
            }

            // Ordinary equipment bags follow the normal
            // Equipment & Blueprints setting.
            else if (!RandomizerState.Settings
                .RandomizeEquipment)
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