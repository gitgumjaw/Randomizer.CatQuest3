


using Gentlebros;
using HarmonyLib;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    // Initial controlled test: suppress Beakon only. No randomized reward is granted yet.
    [HarmonyPatch(typeof(TavernTalesUpdateStatsOnClear), "OnEnter")]
    public static class TavernTalesCompanionSuppressionPatch
    {
        private const string TestEncounter = "TavernTalesEncounter_BoarBoss";
        private const string BeakonGuid = "5a855371377284baa94f29916ca93f61";

        private struct PriorState
        {
            public bool Active;
            public bool HadCompanion;
            public int Level;
        }

        [HarmonyPrefix]
        private static void Prefix(ref PriorState __state)
        {
            __state = default(PriorState);
            RandomizerRunConfiguration config = RandomizerRuntime.CurrentConfiguration;
            if (config == null || !config.Enabled || !config.TavernTalesParrots) return;

            var game = Contexts.sharedInstance.game;
            if (game == null || game.selectedTavernTalesData == null || !game.hasLatestTavernTalesEncounterInfo) return;
            var selection = game.selectedTavernTalesData;
            if (selection.encounter == null || selection.encounter.name != TestEncounter ||
                selection.encounter.rewardCompanion == null || selection.encounter.rewardCompanion.Guid != BeakonGuid) return;
            var manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
            if (manager == null || manager.currSaveSlot == null || manager.currSaveSlot.saveGameCompanionLevelData == null) return;

            Dictionary<string, int> levels = manager.currSaveSlot.saveGameCompanionLevelData.unlockedLevelData;
            if (levels == null) return;
            __state.Active = true;
            __state.HadCompanion = levels.TryGetValue(BeakonGuid, out __state.Level);
        }

        [HarmonyPostfix]
        private static void Postfix(PriorState __state)
        {
            if (!__state.Active) return;
            var manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
            var game = Contexts.sharedInstance.game;
            if (manager == null || manager.currSaveSlot == null || game == null || !game.hasLatestTavernTalesEncounterInfo) return;
            Dictionary<string, int> levels = manager.currSaveSlot.saveGameCompanionLevelData.unlockedLevelData;
            if (__state.HadCompanion) levels[BeakonGuid] = __state.Level;
            else levels.Remove(BeakonGuid);

            var info = game.latestTavernTalesEncounterInfo;
            info.companionNewlyInspired = false;
            info.previousCompanionLevel = 0;

            var selection = game.selectedTavernTalesData;
            Plugin.Log.LogInfo("TAVERN SUPPRESSION TEST | Encounter:" + selection.encounter.name +
                " | Difficulty:" + selection.difficultyLevel +
                " | RestoredCompanion:" + (__state.HadCompanion ? __state.Level.ToString() : "NOT_IN_SAVE") +
                " | HighestClear remains vanilla | Claimed:" +
                TavernTalesProgress.IsClaimed(selection.encounter.name, selection.difficultyLevel) +
                " | No randomized reward granted");
        }
    }
}

