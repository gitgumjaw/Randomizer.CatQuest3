using System;
using System.Collections.Generic;
using Gentlebros;
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    // Diagnostic only: does not alter the vanilla clear or reward flow.
    [HarmonyPatch(typeof(TavernTalesUpdateStatsOnClear), "OnEnter")]
    internal static class TavernTalesClearDiagnosticPatch
    {
        internal sealed class ClearSnapshot
        {
            public string EncounterName;
            public string CompanionGuid;
            public int Difficulty;
            public int? PreviousHighestClear;
            public int? PreviousCompanionLevel;
        }

        [HarmonyPrefix]
        private static void Prefix(out ClearSnapshot __state)
        {
            __state = null;

            try
            {
                __state = Capture();
                LogSnapshot("BEFORE", __state);
            }
            catch (Exception exception)
            {
                Plugin.Log.LogWarning(
                    "TAVERN CLEAR DIAGNOSTIC | BEFORE failed: " + exception);
            }
        }

        [HarmonyPostfix]
        private static void Postfix(ClearSnapshot __state)
        {
            try
            {
                if (__state == null)
                {
                    Plugin.Log.LogWarning(
                        "TAVERN CLEAR DIAGNOSTIC | AFTER | No before snapshot.");
                    return;
                }

                ClearSnapshot after = Capture();
                LogSnapshot("AFTER", after);

                var game = Contexts.sharedInstance.game;
                if (game.hasLatestTavernTalesEncounterInfo)
                {
                    var info = game.latestTavernTalesEncounterInfo;
                    Plugin.Log.LogInfo(
                        "TAVERN CLEAR DIAGNOSTIC | FLAGS" +
                        " | Cleared:" + info.cleared +
                        " | CompanionNewlyInspired:" + info.companionNewlyInspired +
                        " | PreviousCompanionLevel:" + info.previousCompanionLevel);
                }
                else
                {
                    Plugin.Log.LogInfo(
                        "TAVERN CLEAR DIAGNOSTIC | FLAGS | Latest encounter info absent.");
                }

                if (after != null &&
                    (__state.EncounterName != after.EncounterName ||
                     __state.CompanionGuid != after.CompanionGuid ||
                     __state.Difficulty != after.Difficulty))
                {
                    Plugin.Log.LogWarning(
                        "TAVERN CLEAR DIAGNOSTIC | Encounter identity changed during OnEnter.");
                }
            }
            catch (Exception exception)
            {
                Plugin.Log.LogWarning(
                    "TAVERN CLEAR DIAGNOSTIC | AFTER failed: " + exception);
            }
        }

        private static ClearSnapshot Capture()
        {
            var selected = Contexts.sharedInstance.game.selectedTavernTalesData;
            var save = SingletonMonoBehaviour<SaveGameManager>
                .Instance.currSaveSlot;

            string encounterName = selected.encounter.name;
            string companionGuid = selected.encounter.rewardCompanion.Guid;

            Dictionary<string, int> highestClears =
                save.saveGameTavernTalesRecordData.highestClearedDifficultyData;
            Dictionary<string, int> companionLevels =
                save.saveGameCompanionLevelData.unlockedLevelData;

            int previousHighest;
            int previousCompanion;

            return new ClearSnapshot
            {
                EncounterName = encounterName,
                CompanionGuid = companionGuid,
                Difficulty = selected.difficultyLevel,
                PreviousHighestClear = highestClears.TryGetValue(
                    encounterName, out previousHighest)
                    ? (int?)previousHighest : null,
                PreviousCompanionLevel = companionLevels.TryGetValue(
                    companionGuid, out previousCompanion)
                    ? (int?)previousCompanion : null
            };
        }

        private static void LogSnapshot(string phase, ClearSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Plugin.Log.LogWarning(
                    "TAVERN CLEAR DIAGNOSTIC | " + phase + " | Snapshot unavailable.");
                return;
            }

            Plugin.Log.LogInfo(
                "TAVERN CLEAR DIAGNOSTIC | " + phase +
                " | Encounter:" + snapshot.EncounterName +
                " | CompanionGuid:" + snapshot.CompanionGuid +
                " | Difficulty:" + snapshot.Difficulty +
                " | HighestClear:" + Format(snapshot.PreviousHighestClear) +
                " | CompanionLevel:" + Format(snapshot.PreviousCompanionLevel));
        }

        private static string Format(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "NOT_IN_SAVE";
        }
    }
}
