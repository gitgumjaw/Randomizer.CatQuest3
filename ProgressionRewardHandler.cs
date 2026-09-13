using Gentlebros;
using System;
using System.Collections;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class ProgressionRewardHandler
    {
        private const string SquidShipQuestItem =
            "7fe0b53d50a362d42bba16375b91b18d";

        private const string SquidShipLocation =
            "MainOverworld|MainQuest_07_Key_03|FSM|Get Squid Key";

        public static void HandleQuestItem(
            QuestItem questItem,
            Vector3 position,
            Action callback)
        {
            if (questItem == null)
            {
                callback?.Invoke();
                return;
            }

            if (questItem.Guid != SquidShipQuestItem)
            {
                callback?.Invoke();
                return;
            }

            RewardLocation location =
                RewardCatalog.Get(
                    SquidShipLocation
                );

            if (location == null)
            {
                Plugin.Log.LogError(
                    $"Could not find Squid Ship reward location | " +
                    $"Key:{SquidShipLocation}"
                );

                callback?.Invoke();
                return;
            }

            if (location.RandomizedRewards == null ||
                location.RandomizedRewards.Count == 0)
            {
                Plugin.Log.LogError(
                    $"Squid Ship reward location has no randomized rewards | " +
                    $"Location:{location.Label}"
                );

                callback?.Invoke();
                return;
            }

            Reward reward =
                location.RandomizedRewards[0];

            if (reward == null)
            {
                Plugin.Log.LogError(
                    "Squid Ship randomized reward is null."
                );

                callback?.Invoke();
                return;
            }

            if (reward.Type == RewardType.QuestItem &&
                reward.Id == SquidShipQuestItem)
            {
                Plugin.Log.LogWarning(
                    "Squid Ship assigned reward is its own Tentakey. " +
                    "Skipping chained grant."
                );

                callback?.Invoke();
                return;
            }

            SingletonMonoBehaviour<SaveGameManager>
                .Instance
                .StartCoroutine(
                    GrantSquidShipRewardNextFrame(
                        reward,
                        position,
                        callback
                    )
                );
        }

        private static IEnumerator GrantSquidShipRewardNextFrame(
            Reward reward,
            Vector3 position,
            Action callback)
        {
            // Let the first QuestItem popup completely
            // finish closing before opening another award UI.
            yield return null;

            Plugin.Log.LogInfo(
                $"Granting Squid Ship assigned reward early | " +
                $"Reward:{reward.Type}:{reward.Id}"
            );

            RewardGranter.Grant(
                reward,
                -1,
                position,
                callback
            );
        }
    }
}