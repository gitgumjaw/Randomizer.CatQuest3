using System;
using System.Collections.Generic;
using Gentlebros;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // Permanent progressive companion reward delivery.
    // Each Reward(RewardType.Companion, companionGuid) advances that bird one star.
    public static class CompanionRewardGranter
    {
        public static void Grant(Reward reward, Action callback)
        {
            bool callbackCalled = false;
            Action finish = delegate
            {
                if (callbackCalled) return;
                callbackCalled = true;
                callback?.Invoke();
            };

            try
            {
                RandomizerRunConfiguration configuration = RandomizerRuntime.CurrentConfiguration;
                if (configuration == null || !configuration.Enabled || !configuration.TavernTalesParrots)
                {
                    Plugin.Log.LogWarning("PROGRESSIVE COMPANION | Reward encountered while Tavern Tales Parrots disabled.");
                    finish();
                    return;
                }

                if (reward == null || string.IsNullOrEmpty(reward.Id))
                {
                    Plugin.Log.LogError("PROGRESSIVE COMPANION | Missing companion GUID.");
                    finish();
                    return;
                }

                var manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
                SaveGameData save = manager == null ? null : manager.currSaveSlot;
                if (save == null || save.saveGameCompanionLevelData == null || save.saveGameCompanionData == null ||
                    save.saveGameCompanionLevelData.unlockedLevelData == null)
                {
                    Plugin.Log.LogError("PROGRESSIVE COMPANION | Companion save data unavailable.");
                    finish();
                    return;
                }

                var database = AddressableSingletonScriptableObject<CompanionDatabase>.Instance;
                if (database == null || database.contentTable == null)
                {
                    Plugin.Log.LogError("PROGRESSIVE COMPANION | Companion database unavailable.");
                    finish();
                    return;
                }

                CompanionDataAsset companion = null;
                foreach (KeyValuePair<string, CompanionDataAsset> entry in database.contentTable)
                {
                    if (entry.Value != null && string.Equals(entry.Value.Guid, reward.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        companion = entry.Value;
                        break;
                    }
                }

                if (companion == null)
                {
                    Plugin.Log.LogError("PROGRESSIVE COMPANION | Unrecognized companion GUID:" + reward.Id);
                    finish();
                    return;
                }

                Dictionary<string, int> levels = save.saveGameCompanionLevelData.unlockedLevelData;
                int oldLevel;
                if (!levels.TryGetValue(companion.Guid, out oldLevel) || oldLevel < 0) oldLevel = 0;
                int newLevel = Math.Min(3, oldLevel + 1);
                levels[companion.Guid] = newLevel;

                Plugin.Log.LogInfo("PROGRESSIVE COMPANION | Guid:" + companion.Guid +
                    " | Level:" + oldLevel + "->" + newLevel);

                if (save.saveGameCompanionData.companionData == null)
                {
                    GameplayHelper.SetCompanionUnit(companion);
                    GameplayHelper.SummonCompanion(Vector3.right * 1.2f, Facing.FRONTLEFT);
                    Plugin.Log.LogInfo("PROGRESSIVE COMPANION | Automatically equipped:" + companion.name);
                }
                else
                {
                    Plugin.Log.LogInfo("PROGRESSIVE COMPANION | Preserved equipped:" +
                        save.saveGameCompanionData.companionData.name);
                }

                if (Contexts.sharedInstance == null || Contexts.sharedInstance.gUI == null ||
                    Contexts.sharedInstance.gUI.companionAwardPanel.value == null)
                {
                    Plugin.Log.LogWarning("PROGRESSIVE COMPANION | Award panel unavailable; continuing.");
                    finish();
                    return;
                }

                CompanionAwardPanelUIInfo info = new CompanionAwardPanelUIInfo
                {
                    companionAwarded = companion,
                    initialUnlock = newLevel == 1,
                    startValue = oldLevel,
                    endValue = newLevel
                };

                Contexts.sharedInstance.gUI.companionAwardPanel.value.Show(info, finish);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("PROGRESSIVE COMPANION | Grant failed: " + ex);
                finish();
            }
        }
    }
}
