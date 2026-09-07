using HarmonyLib;
using ProjectStar.Data;
using static ChestData;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(ChestBehaviour), "SpawnItemLoot")]
    public static class SpawnItemLootPatch
    {
        public static void Postfix(
            ref EquipmentItemData __result,
            ChestID ___chestID,
            ChestType ___chestType)
        {
            // Repeatable enemy equipment pouches get a completely
            // fresh random equipment item every time.
            //
            // This deliberately does NOT use the randomizer seed.
            if (__result != null &&
                ___chestType == ChestType.Bag)
            {
                EquipmentItemData replacement =
                    RandomEquipmentDropPool.GetRandom();

                if (replacement != null)
                {
                    Plugin.Log.LogInfo(
                        $"Randomized Bag equipment: " +
                        $"{__result.itemName} -> " +
                        $"{replacement.itemName}"
                    );

                    __result = replacement;
                }
            }

            if (__result != null && ___chestID == null)
            {
                Plugin.Log.LogInfo(
                    $"SpawnItemLoot with NO ChestID | " +
                    $"ChestType: {(int)___chestType} ({___chestType}) | " +
                    $"Item: {__result.itemName} | " +
                    $"Guid: {__result.Guid}"
                );
            }

            if (__result == null || ___chestID == null)
            {
                return;
            }

            RewardType rewardType;

            if (__result is ShipBlueprintItemData)
            {
                rewardType = RewardType.Blueprint;
            }
            else
            {
                rewardType = RewardType.Equipment;
            }

            RewardLocation location =
                new RewardLocation(
                    ___chestID.Guid,
                    new Reward(
                        rewardType,
                        __result.Guid
                    )
                );

            RewardRegistry.Register(location);
        }
    }
}