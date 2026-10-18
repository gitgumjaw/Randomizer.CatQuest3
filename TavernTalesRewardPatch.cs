using System;
using System.Collections.Generic;
using Gentlebros;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // Substitute the randomized location's reward at the vanilla post-arena
    // companion award action. Never display the vanilla companion panel here.
    [HarmonyPatch(typeof(TavernTalesShowCompanionReward), "OnEnter")]
    public static class TavernTalesRewardPatch
    {
        private static readonly HashSet<string> pending =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        [HarmonyPrefix]
        private static bool Prefix(TavernTalesShowCompanionReward __instance)
        {
            RandomizerRunConfiguration config = RandomizerRuntime.CurrentConfiguration;
            if (config == null || !config.Enabled || !config.TavernTalesParrots)
                return true;

            var contexts = Contexts.sharedInstance;
            if (contexts == null || contexts.game == null)
                return true;

            var game = contexts.game;
            if (game.selectedTavernTalesData == null || !game.hasLatestTavernTalesEncounterInfo ||
                game.selectedTavernTalesData.encounter == null)
                return true;

            var selection = game.selectedTavernTalesData;
            var result = game.latestTavernTalesEncounterInfo;
            if (!result.cleared)
                return true;

            string encounter = selection.encounter.name;
            int difficulty = selection.difficultyLevel;
            string key = TavernTalesProgress.LocationKey(encounter, difficulty);
            if (key == null)
                return true;

            if (TavernTalesProgress.IsClaimed(encounter, difficulty) || pending.Contains(key))
            {
                Plugin.Log.LogInfo("TAVERN REWARD | Already claimed or pending | Location:" + key);
                __instance.Finish();
                return false;
            }

            RewardLocation location = RewardCatalog.Get(key);
            if (location == null || location.RandomizedRewards == null ||
                location.RandomizedRewards.Count != 1 || location.RandomizedRewards[0] == null)
            {
                Plugin.Log.LogError("TAVERN REWARD | Missing single-slot randomized location | Key:" + key);
                __instance.Finish();
                return false;
            }

            var manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
            if (manager == null || manager.currSaveSlot == null)
            {
                Plugin.Log.LogError("TAVERN REWARD | Save unavailable | Key:" + key);
                __instance.Finish();
                return false;
            }

            Reward reward = location.RandomizedRewards[0];
            pending.Add(key);
            Plugin.Log.LogInfo("TAVERN REWARD | Queueing at companion award stage | Location:" + location.Label +
                " | Reward:" + reward.Type + ":" + reward.Id);

            try
            {
                RewardGrantQueue.Enqueue(location, 0, reward, -1,
                    RewardPositionResolver.PlayerOrFallback(Vector3.zero),
                    delegate
                    {
                        pending.Remove(key);
                        bool claimed = TavernTalesProgress.MarkClaimed(encounter, difficulty);
                        Plugin.Log.LogInfo("TAVERN REWARD | Completed | Key:" + key +
                            " | Claimed:" + claimed);
                        __instance.Finish();
                    });
            }
            catch (Exception ex)
            {
                pending.Remove(key);
                Plugin.Log.LogError("TAVERN REWARD | Failed to queue | Key:" + key + " | " + ex);
                __instance.Finish();
            }

            return false;
        }
    }
}
