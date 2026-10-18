using System;
using System.Collections.Generic;
using Gentlebros;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // TEMPORARY CONTROLLED TEST ONLY.
    // Replace slot 0 of Catuga North Crate, West Chest, and East Crate with Beakon at grant time.
    // This does not alter the generated reward catalog or the other chest slot.
    // Remove this entire file after the companion grant test.
    [HarmonyPatch]
    public static class TavernTalesBeakonChestTestPatch
    {
        private const string NorthCrateGuid = "0294cd0074d1d204db0bac9eab555e55";
        private const string WestChestGuid = "7790326666aad054e8775e3759d4a28a";
        private const string EastCrateGuid = "b3c5d4e55fb2a914b8a547680be68a78";
        private const string BeakonGuid = "5a855371377284baa94f29916ca93f61";
        private const string TestRewardId = "RANDOMIZER|TEST|BEAKON_COMPANION";

        private static bool Enabled
        {
            get
            {
                RandomizerRunConfiguration config = RandomizerRuntime.CurrentConfiguration;
                return config != null && config.Enabled && config.TavernTalesParrots;
            }
        }

        // Use an existing RewardType only as a temporary test transport marker.
        // The proper Companion RewardType will be introduced with pool integration.
        [HarmonyPatch(typeof(RewardGrantQueue), "Enqueue")]
        [HarmonyPrefix]
        private static void ReplaceWestChestSlot(
            RewardLocation location,
            int slotIndex,
            ref Reward reward)
        {
            if (!Enabled || location == null || slotIndex != 0 ||
                !(string.Equals(location.Key, NorthCrateGuid, StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(location.Key, WestChestGuid, StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(location.Key, EastCrateGuid, StringComparison.OrdinalIgnoreCase)))
                return;

            Plugin.Log.LogInfo(
                "BEAKON CHEST TEST | Replacing " + location.Label + " slot 0 | Original:" +
                (reward == null ? "NULL" : reward.Type + ":" + reward.Id));

            reward = new Reward(RewardType.Equipment, TestRewardId);
        }

        [HarmonyPatch(typeof(RewardGranter), "Grant")]
        [HarmonyPrefix]
        private static bool GrantBeakonTestReward(Reward reward, Action callback)
        {
            if (reward == null || reward.Id != TestRewardId ||
                reward.Type != RewardType.Equipment)
                return true;

            // The queue must continue exactly once, after the award panel closes.
            bool finished = false;
            Action finish = delegate
            {
                if (finished) return;
                finished = true;
                callback?.Invoke();
            };

            if (!Enabled)
            {
                Plugin.Log.LogWarning("BEAKON CHEST TEST | Test reward encountered while disabled; skipping.");
                finish();
                return false;
            }

            try
            {
                GrantBeakon(finish);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("BEAKON CHEST TEST | Grant failed: " + ex);
                finish();
            }

            return false;
        }

        private static void GrantBeakon(Action callback)
        {
            SaveGameManager manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
            SaveGameData save = manager == null ? null : manager.currSaveSlot;
            if (save == null || save.saveGameCompanionLevelData == null ||
                save.saveGameCompanionData == null)
            {
                Plugin.Log.LogError("BEAKON CHEST TEST | Companion save data unavailable.");
                callback?.Invoke();
                return;
            }

            CompanionDataAsset beakon = null;
            var database = AddressableSingletonScriptableObject<CompanionDatabase>.Instance;
            if (database == null || database.contentTable == null)
            {
                Plugin.Log.LogError("BEAKON CHEST TEST | Companion database unavailable.");
                callback?.Invoke();
                return;
            }

            foreach (KeyValuePair<string, CompanionDataAsset> entry in database.contentTable)
            {
                if (entry.Value != null &&
                    string.Equals(entry.Value.Guid, BeakonGuid, StringComparison.OrdinalIgnoreCase))
                {
                    beakon = entry.Value;
                    break;
                }
            }

            if (beakon == null)
            {
                Plugin.Log.LogError("BEAKON CHEST TEST | Beakon not found in CompanionDatabase.");
                callback?.Invoke();
                return;
            }

            Dictionary<string, int> levels = save.saveGameCompanionLevelData.unlockedLevelData;
            if (levels == null)
            {
                Plugin.Log.LogError("BEAKON CHEST TEST | Companion levels dictionary unavailable.");
                callback?.Invoke();
                return;
            }

            int oldLevel;
            if (!levels.TryGetValue(BeakonGuid, out oldLevel) || oldLevel < 0)
                oldLevel = 0;
            int newLevel = Math.Min(3, oldLevel + 1);
            levels[BeakonGuid] = newLevel;

            Plugin.Log.LogInfo("BEAKON CHEST TEST | Granted Beakon | Level:" + oldLevel + "->" + newLevel);

            if (save.saveGameCompanionData.companionData == null)
            {
                GameplayHelper.SetCompanionUnit(beakon);
                GameplayHelper.SummonCompanion(Vector3.right * 1.2f, Facing.FRONTLEFT);
                Plugin.Log.LogInfo("BEAKON CHEST TEST | Automatically equipped and summoned Beakon.");
            }
            else
            {
                Plugin.Log.LogInfo("BEAKON CHEST TEST | Existing companion selection preserved: " +
                    save.saveGameCompanionData.companionData.name);
            }
            // Reuse the vanilla companion-award UI, independent of Tavern Tale clears.
            if (Contexts.sharedInstance == null ||
                Contexts.sharedInstance.gUI == null ||
                Contexts.sharedInstance.gUI.companionAwardPanel.value == null)
            {
                Plugin.Log.LogWarning("BEAKON CHEST TEST | Companion award panel unavailable; continuing without popup.");
                callback?.Invoke();
                return;
            }

            CompanionAwardPanelUIInfo info = new CompanionAwardPanelUIInfo
            {
                companionAwarded = beakon,
                initialUnlock = newLevel == 1,
                startValue = oldLevel,
                endValue = newLevel
            };

            Plugin.Log.LogInfo("BEAKON CHEST TEST | Showing companion award panel | Level:" +
                oldLevel + "->" + newLevel);

            Contexts.sharedInstance.gUI.companionAwardPanel.value.Show(
                info,
                delegate
                {
                    Plugin.Log.LogInfo("BEAKON CHEST TEST | Companion award panel dismissed.");
                    callback?.Invoke();
                });
        }
    }
}
