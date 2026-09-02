using HarmonyLib;
using ProjectStar.Data;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(ChestBehaviour), "SpawnItemLoot")]
    public static class SpawnItemLootPatch
    {
        public static void Postfix(
            EquipmentItemData __result,
            ChestID ___chestID)
        {
            if (__result != null && ___chestID == null)
            {
                Plugin.Log.LogInfo(
                    $"SpawnItemLoot with NO ChestID | " +
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

            RewardLocation location = new RewardLocation(
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