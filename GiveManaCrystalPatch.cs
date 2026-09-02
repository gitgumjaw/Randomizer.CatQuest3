using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(GivePlayersManaCrystal), "OnEnter")]
    public static class GiveManaCrystalPatch
    {
        public static void Prefix(GivePlayersManaCrystal __instance)
        {
            RewardLocation location = new RewardLocation(
                PlayMakerLocation.GetKey(__instance.Fsm),
                new Reward(
                    RewardType.ManaCrystal,
                    null
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