using Gentlebros;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class QuestItemTestPatch
    {
        private static bool hasTriggered;

        [HarmonyPatch(
            typeof(ChestBehaviour),
            "GiveChestKeyAfterDelay"
        )]
        [HarmonyPostfix]
        public static void GiveChestKeyAfterDelayPostfix(
            ChestBehaviour __instance,
            ChestData.ChestType ___chestType)
        {
            if (hasTriggered)
            {
                return;
            }

            // Do not trigger from repeatable Bag chests.
            if (___chestType ==
                ChestData.ChestType.Bag)
            {
                return;
            }

            hasTriggered = true;

            Plugin.Log.LogInfo(
                "QUEST ITEM TEST | " +
                "Granting all catalog QuestItems."
            );

            SingletonMonoBehaviour<SaveGameManager>
                .Instance
                .StartCoroutine(
                    GrantAllQuestItems()
                );
        }

        private static IEnumerator GrantAllQuestItems()
        {
            // Give the chest a moment to finish cleaning up.
            yield return new WaitForSecondsRealtime(
                1.0f
            );

            List<string> questItemIds =
                GetQuestItemIds();

            Plugin.Log.LogInfo(
                $"QUEST ITEM TEST | " +
                $"Found {questItemIds.Count} unique QuestItems."
            );

            foreach (string id in questItemIds)
            {
                QuestItem questItem =
                    RewardDataResolver.GetQuestItem(
                        id
                    );

                if (questItem == null)
                {
                    Plugin.Log.LogWarning(
                        $"QUEST ITEM TEST | " +
                        $"Could not resolve:{id}"
                    );

                    continue;
                }

                Plugin.Log.LogInfo(
                    $"QUEST ITEM TEST | " +
                    $"Granting:{id}"
                );

                bool finished = false;

                GameplayHelper.AwardQuestItem(
                    questItem,
                    delegate
                    {
                        finished = true;
                    }
                );

                yield return new WaitUntil(
                    () => finished
                );

                // Small separation so consecutive
                // award UIs do not collide.
                yield return new WaitForSecondsRealtime(
                    0.25f
                );
            }

            Plugin.Log.LogInfo(
                "QUEST ITEM TEST | " +
                "Finished granting all QuestItems."
            );
        }

        private static List<string> GetQuestItemIds()
        {
            HashSet<string> uniqueIds =
                new HashSet<string>();

            foreach (RewardLocation location
                     in RewardCatalog.GetAll())
            {
                foreach (Reward reward
                         in location.VanillaRewards)
                {
                    if (reward.Type !=
                        RewardType.QuestItem)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(
                            reward.Id))
                    {
                        continue;
                    }

                    uniqueIds.Add(
                        reward.Id
                    );
                }
            }

            List<string> ids =
                new List<string>(
                    uniqueIds
                );

            ids.Sort();

            return ids;
        }
    }
}