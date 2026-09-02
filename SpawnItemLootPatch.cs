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

            Plugin.Log.LogInfo(
                $"Location: {location.Key} | " +
                $"Vanilla Reward: {location.VanillaReward.Type} " +
                $"{location.VanillaReward.Id}"
            );
        }
    }
}