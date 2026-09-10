using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardShipSpecialAmmoToPlayer), "Award")]
    public static class AwardShipSpecialAmmoPatch
    {
        private static readonly HashSet<AwardShipSpecialAmmoToPlayer> catalogedActions =
            new HashSet<AwardShipSpecialAmmoToPlayer>();

        public static void Prefix(
            AwardShipSpecialAmmoToPlayer __instance)
        {
            if (__instance.specialAmmo == null)
            {
                return;
            }

            string locationKey =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );

            Reward reward =
                new Reward(
                    RewardType.ShipSpell,
                    __instance.specialAmmo.Guid
                );

            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    reward
                )
            );

            if (!catalogedActions.Add(__instance))
            {
                return;
            }

            CatalogRewardLocation catalogLocation =
                CatalogScanResults.GetOrCreate(
                    locationKey
                );

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
                $"Cataloged scripted ShipSpell location: " +
                $"{locationKey} | " +
                $"ShipSpell: {__instance.specialAmmo.Guid} | " +
                $"Total Slots: {catalogLocation.RewardSlots.Count}"
            );
        }
    }
}