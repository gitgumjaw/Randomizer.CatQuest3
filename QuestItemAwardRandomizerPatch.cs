using HarmonyLib;
using Gentlebros;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(AwardQuestItem),
        "OnEnter"
    )]
    public static class
        QuestItemAwardRandomizerPatch
    {
        public static bool Prefix(
            AwardQuestItem __instance)
        {
            if (__instance.questItem == null)
            {
                Plugin.Log.LogWarning(
                    "QUEST ITEM RANDOMIZER FALLBACK | " +
                    "Reason:QuestItemNull"
                );

                return true;
            }


            string key =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );


            RewardLocation location =
                RewardCatalog.Get(
                    key
                );


            if (location == null)
            {
                Plugin.Log.LogWarning(
                    $"QUEST ITEM RANDOMIZER FALLBACK | " +
                    $"Reason:LocationNotFound | " +
                    $"Key:{key} | " +
                    $"QuestItem:{__instance.questItem.Guid}"
                );

                return true;
            }


            int rewardIndex =
                location.FindVanillaRewardIndex(
                    RewardType.QuestItem,
                    __instance.questItem.Guid
                );


            if (rewardIndex < 0)
            {
                Plugin.Log.LogWarning(
                    $"QUEST ITEM RANDOMIZER FALLBACK | " +
                    $"Reason:VanillaRewardNotFound | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label} | " +
                    $"QuestItem:{__instance.questItem.Guid}"
                );

                return true;
            }


            Reward randomizedReward =
                location.RandomizedRewards[
                    rewardIndex
                ];


            if (randomizedReward == null)
            {
                Plugin.Log.LogWarning(
                    $"QUEST ITEM RANDOMIZER FALLBACK | " +
                    $"Reason:RandomizedRewardNull | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label} | " +
                    $"Slot:{rewardIndex} | " +
                    $"QuestItem:{__instance.questItem.Guid}"
                );

                return true;
            }


            QuestItem vanillaQuestItem =
                __instance.questItem;


            Vector3 position =
                RewardPositionResolver.PlayerOrFallback(
                    __instance.Fsm.GameObject
                        .transform.position
                );


            Plugin.Log.LogInfo(
                $"Queueing quest item reward | " +
                $"Location:{location.Label} | " +
                $"Slot:{rewardIndex} | " +
                $"Vanilla:QuestItem:" +
                $"{vanillaQuestItem.Guid} | " +
                $"Randomized:{randomizedReward.Type}:" +
                $"{randomizedReward.Id}"
            );


            RewardGrantQueue.Enqueue(
                location,
                rewardIndex,
                randomizedReward,
                -1,
                position,
                delegate
                {
                    PreserveLovepurrProgression(
                        vanillaQuestItem
                    );


                    __instance.Finish();


                    PlayMakerUtils.UpdateFsmThisFrame(
                        __instance.Fsm
                    );
                }
            );


            return false;
        }


        private static void
            PreserveLovepurrProgression(
                QuestItem questItem)
        {
            if (!IsLovepurrBook(
                    questItem))
            {
                return;
            }


            if (questItem.key == null)
            {
                Plugin.Log.LogWarning(
                    $"LOVEBOOK COMPATIBILITY | " +
                    $"Reason:KeyNull | " +
                    $"QuestItem:{questItem.Guid}"
                );

                return;
            }


            if (SingletonMonoBehaviour<
                    SaveGameManager>
                .Instance == null ||
                SingletonMonoBehaviour<
                    SaveGameManager>
                .Instance.currSaveSlot == null ||
                SingletonMonoBehaviour<
                    SaveGameManager>
                .Instance.currSaveSlot
                .savedKeyData == null)
            {
                Plugin.Log.LogWarning(
                    $"LOVEBOOK COMPATIBILITY | " +
                    $"Reason:SavedKeyDataUnavailable | " +
                    $"QuestItem:{questItem.Guid} | " +
                    $"Key:{questItem.key.Guid}"
                );

                return;
            }


            // Vanilla GameplayHelper.AwardQuestItem()
            // adds this KeyData after the award UI.
            //
            // The LoveBook FSMs use that persistent key
            // state to determine which chapter comes next.
            //
            // We intentionally do NOT add the vanilla
            // QuestItem to awardedQuestItems. Only its
            // progression key is preserved.
            SingletonMonoBehaviour<
                SaveGameManager>
                .Instance
                .currSaveSlot
                .savedKeyData
                .AddKey(
                    questItem.key
                );


            Plugin.Log.LogInfo(
                $"LOVEBOOK COMPATIBILITY | " +
                $"Added chapter progression key | " +
                $"QuestItem:{questItem.Guid} | " +
                $"Key:{questItem.key.Guid}"
            );
        }


        private static bool IsLovepurrBook(
            QuestItem questItem)
        {
            if (questItem == null)
            {
                return false;
            }


            return
                questItem.Guid ==
                    SpecialRewards
                        .LovepurrBook1Guid ||
                questItem.Guid ==
                    SpecialRewards
                        .LovepurrBook2Guid ||
                questItem.Guid ==
                    SpecialRewards
                        .LovepurrBook3Guid;
        }
    }
}