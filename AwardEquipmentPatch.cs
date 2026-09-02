using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardEquipmentToPlayer), "Award")]
    public static class AwardEquipmentPatch
    {
        public static void Prefix(AwardEquipmentToPlayer __instance)
        {
            if (__instance.equipmentItem == null)
            {
                return;
            }

            RewardLocation location = new RewardLocation(
                PlayMakerLocation.GetKey(__instance.Fsm),
                new Reward(
                    RewardType.Equipment,
                    __instance.equipmentItem.Guid
                )
            );

            RewardRegistry.Register(location);
        }
    }
}