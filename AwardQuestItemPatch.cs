using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(AwardQuestItem), "OnEnter")]
    public static class AwardQuestItemPatch
    {
        private const string StarRuneGuid =
            "fd36ba1341aa4d24692cc3eedea20405";

        private static readonly HashSet<AwardQuestItem> catalogedActions =
            new HashSet<AwardQuestItem>();

        public static void Prefix(AwardQuestItem __instance)
        {
            if (__instance.questItem == null)
            {
                return;
            }

            string locationKey =
                PlayMakerLocation.GetKey(__instance.Fsm);

            Reward reward =
                new Reward(
                    RewardType.QuestItem,
                    __instance.questItem.Guid
                );

            // Existing runtime discovery behavior.
            RewardRegistry.Register(
                new RewardLocation(
                    locationKey,
                    reward
                )
            );

            // Development catalog behavior.
            if (catalogedActions.Add(__instance))
            {
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
                    $"Cataloged scripted Quest Item location: " +
                    $"{locationKey} | " +
                    $"Quest Item: {__instance.questItem.Guid} | " +
                    $"Total Slots: {catalogLocation.RewardSlots.Count}"
                );
            }

            // Temporary early-Float proof of concept.
            if (__instance.questItem.Guid == StarRuneGuid)
            {
                Contexts.sharedInstance.game.isFloatBlocked = false;

                Plugin.Log.LogInfo(
                    "Float enabled early with Star Rune."
                );
            }
        }
    }
}