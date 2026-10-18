using Gentlebros;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    // Suppress all five vanilla Tavern Tales bird unlocks while companion
    // randomization is enabled, without interfering with vanilla clear records.
    [HarmonyPatch(typeof(TavernTalesUpdateStatsOnClear), "OnEnter")]
    public static class TavernTalesCompanionSuppressionPatch
    {
        private static readonly Dictionary<string, string> CompanionsByEncounter =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "TavernTalesEncounter_BoarBoss", "5a855371377284baa94f29916ca93f61" },
                { "TavernTalesEncounter_MewtallicaBoss", "09525eba5b6fd8a4b89c969b2032d17a" },
                { "TavernTalesEncounter_SpicySquidTakomeowkiBoss", "5c1ea0c570901904c8b3916fd21c3833" },
                { "TavernTalesEncounter_PirateKing", "5c26ee3b0ef6f8e41bd53bdf73b208f6" },
                { "TavernTalesEncounter_Gentlebros", "615915087dc6bd74db8fa0578a228c08" }
            };

        private struct PriorState
        {
            public bool Active;
            public bool HadCompanion;
            public int Level;
            public string CompanionGuid;
        }

        [HarmonyPrefix]
        private static void Prefix(ref PriorState __state)
        {
            __state = default(PriorState);
            RandomizerRunConfiguration config = RandomizerRuntime.CurrentConfiguration;
            if (config == null || !config.Enabled || !config.TavernTalesParrots) return;

            var contexts = Contexts.sharedInstance;
            if (contexts == null || contexts.game == null) return;
            var game = contexts.game;
            if (game.selectedTavernTalesData == null || !game.hasLatestTavernTalesEncounterInfo) return;
            var selection = game.selectedTavernTalesData;
            if (selection.encounter == null || selection.encounter.rewardCompanion == null) return;

            string companionGuid;
            if (!CompanionsByEncounter.TryGetValue(selection.encounter.name, out companionGuid) ||
                !string.Equals(selection.encounter.rewardCompanion.Guid, companionGuid, StringComparison.OrdinalIgnoreCase))
                return;

            var manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
            if (manager == null || manager.currSaveSlot == null || manager.currSaveSlot.saveGameCompanionLevelData == null)
                return;

            Dictionary<string, int> levels = manager.currSaveSlot.saveGameCompanionLevelData.unlockedLevelData;
            if (levels == null) return;
            __state.Active = true;
            __state.CompanionGuid = companionGuid;
            __state.HadCompanion = levels.TryGetValue(companionGuid, out __state.Level);
        }

        [HarmonyPostfix]
        private static void Postfix(PriorState __state)
        {
            if (!__state.Active) return;
            var manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
            var contexts = Contexts.sharedInstance;
            if (manager == null || manager.currSaveSlot == null ||
                manager.currSaveSlot.saveGameCompanionLevelData == null ||
                contexts == null || contexts.game == null ||
                !contexts.game.hasLatestTavernTalesEncounterInfo) return;

            Dictionary<string, int> levels = manager.currSaveSlot.saveGameCompanionLevelData.unlockedLevelData;
            if (levels == null) return;
            if (__state.HadCompanion) levels[__state.CompanionGuid] = __state.Level;
            else levels.Remove(__state.CompanionGuid);

            var info = contexts.game.latestTavernTalesEncounterInfo;
            // Do not clear companionNewlyInspired here: vanilla uses it to
            // reach TavernTalesShowCompanionReward after leaving the arena.
            // That action is intercepted by TavernTalesRewardPatch before its UI runs.

            Plugin.Log.LogInfo("TAVERN SUPPRESSION | Restored companion:" + __state.CompanionGuid +
                " | Level:" + (__state.HadCompanion ? __state.Level.ToString() : "NOT_IN_SAVE") +
                " | Vanilla clear records unchanged");
        }
    }
}
