using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(GivePlayersManaCrystal), "OnEnter")]
    public static class GiveManaCrystalPatch
    {
        private static readonly HashSet<GivePlayersManaCrystal> catalogedActions =
            new HashSet<GivePlayersManaCrystal>();

        public static void Prefix(GivePlayersManaCrystal __instance)
        {
            string locationKey =
                PlayMakerLocation.GetKey(__instance.Fsm);

            Reward reward =
                new Reward(
                    RewardType.ManaCrystal,
                    null
                );

            // Keep existing runtime discovery behavior.
            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    reward
                )
            );

            // Do not add the same PlayMaker action to the
            // development catalog more than once.
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
                    },

                    // Mana increases happen during cinematics.
                    // A ground-spawned collectible could be unreachable here.
                    allowCollectibles: false
                )
            );

            Plugin.Log.LogInfo(
                $"Cataloged Mana Crystal location: {locationKey} | " +
                $"Allow Collectibles: False | " +
                $"Total Slots: {catalogLocation.RewardSlots.Count}"
            );
        }
    }
}