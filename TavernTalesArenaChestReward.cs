using System;
using System.Collections.Generic;
using Gentlebros;
using ProjectStar.Data;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // Associates a generated Tavern Tales chest with its boss/difficulty check.
    // No new Harmony hooks; called by the existing ChestAwardRandomizerPatch.
    internal static class TavernTalesArenaChestReward
    {
        private sealed class ChestClaim
        {
            internal string Encounter;
            internal int Difficulty;
        }

        private static readonly Dictionary<int, ChestClaim> registered =
            new Dictionary<int, ChestClaim>();

        internal static bool TryGetLocation(ChestBehaviour chest, ChestID chestID,
            ChestData.ChestType chestType, out RewardLocation location)
        {
            location = null;

            RandomizerRunConfiguration config = RandomizerRuntime.CurrentConfiguration;
            if (config == null || !config.Enabled || !config.TavernTalesParrots)
                return false;

            if (chest == null || chestID != null ||
                chestType != ChestData.ChestType.Normal ||
                chest.gameObject.name != "ChestTrigger(Clone)" ||
                chest.gameObject.scene.name != "DontDestroyOnLoad")
                return false;

            var contexts = Contexts.sharedInstance;
            if (contexts == null || contexts.game == null ||
                contexts.game.selectedTavernTalesData == null ||
                contexts.game.selectedTavernTalesData.encounter == null)
                return false;

            var selection = contexts.game.selectedTavernTalesData;
            string encounter = selection.encounter.name;
            int difficulty = selection.difficultyLevel;
            string key = TavernTalesProgress.LocationKey(encounter, difficulty);
            if (key == null)
                return false;

            // The chest occurs before the return-to-tavern victory bookkeeping.
            // Do NOT require latestTavernTalesEncounterInfo.cleared here.
            if (TavernTalesProgress.IsClaimed(encounter, difficulty))
            {
                Plugin.Log.LogInfo("TAVERN CHEST | Claimed previously; using vanilla loot | Key:" + key);
                return false;
            }

            location = RewardCatalog.Get(key);
            if (location == null || location.RandomizedRewards == null ||
                location.RandomizedRewards.Count != 1 ||
                location.RandomizedRewards[0] == null)
            {
                Plugin.Log.LogWarning("TAVERN CHEST | No single-slot randomized location; using vanilla loot | Key:" + key);
                location = null;
                return false;
            }

            registered[chest.GetInstanceID()] = new ChestClaim
            {
                Encounter = encounter,
                Difficulty = difficulty
            };

            Plugin.Log.LogInfo("TAVERN CHEST | Resolved | Key:" + key +
                " | Instance:" + chest.GetInstanceID());
            return true;
        }

        // Called only by the final queued reward's successful completion callback.
        internal static void CompleteReward(ChestBehaviour chest)
        {
            if (chest == null) return;
            int id = chest.GetInstanceID();
            ChestClaim claim;
            if (!registered.TryGetValue(id, out claim)) return;

            registered.Remove(id);
            bool marked = TavernTalesProgress.MarkClaimed(claim.Encounter, claim.Difficulty);
            Plugin.Log.LogInfo("TAVERN CHEST | Reward completed | Encounter:" +
                claim.Encounter + " | Difficulty:" + claim.Difficulty +
                " | Claimed:" + marked);
        }

        internal static void Forget(ChestBehaviour chest)
        {
            if (chest != null) registered.Remove(chest.GetInstanceID());
        }
    }
}
