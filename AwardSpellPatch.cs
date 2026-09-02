using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardSpellToPlayer), "Award")]
    public static class AwardSpellPatch
    {
        public static void Prefix(AwardSpellToPlayer __instance)
        {
            if (__instance.spellConfig == null)
            {
                return;
            }

            RewardLocation location = new RewardLocation(
                PlayMakerLocation.GetKey(__instance.Fsm),
                new Reward(
                    RewardType.Spell,
                    __instance.spellConfig.Guid
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