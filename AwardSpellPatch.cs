using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardSpellToPlayer), "Award")]
    public static class AwardSpellPatch
    {
        private static readonly HashSet<AwardSpellToPlayer> catalogedActions =
            new HashSet<AwardSpellToPlayer>();

        public static void Prefix(AwardSpellToPlayer __instance)
        {
            if (__instance.spellConfig == null)
            {
                return;
            }

            string locationKey =
                PlayMakerLocation.GetKey(__instance.Fsm);

            Reward reward =
                new Reward(
                    RewardType.Spell,
                    __instance.spellConfig.Guid
                );

            // Existing runtime discovery behavior.
            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    reward
                )
            );

            // Development catalog behavior.
            if (!catalogedActions.Add(__instance))
            {
                return;
            }

            CatalogRewardLocation catalogLocation =
                CatalogScanResults.GetOrCreate(locationKey);

            catalogLocation.AddSlot(
                new WeightedRewardSlot(
                    new[]
                    {
                        new WeightedRewardOption(
                            reward,
                            1
                        )
                    }
                )
            );

            Plugin.Log.LogInfo(
                $"Cataloged scripted Spell location: " +
                $"{locationKey} | " +
                $"Spell: {__instance.spellConfig.Guid} | " +
                $"Total Slots: {catalogLocation.RewardSlots.Count}"
            );
        }
    }
}