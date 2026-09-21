using Gentlebros;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class SaveSlotRandomizerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(SaveLoadPanel),
            "TransitToGameHelper"
        )]
        public static void TransitToGameHelperPrefix(
            SaveLoadPanel __instance)
        {
            if (__instance == null)
            {
                return;
            }


            int profileIndex =
                GetTrackedProfileIndex(
                    __instance
                );


            if (__instance.startingGameMode ==
                SaveLoadPanel.StartingGameMode.NEW)
            {
                RandomizerRunConfiguration pending =
                    RandomizerScreen.PendingConfiguration;


                if (pending != null)
                {
                    RandomizerRuntime.Activate(
                        profileIndex,
                        pending
                    );
                }
                else
                {
                    RandomizerRuntime.ActivateVanilla(
                        profileIndex
                    );

                    Plugin.Log.LogWarning(
                        "RANDOMIZER RUNTIME | " +
                        "No pending configuration was available for new game. " +
                        "Using vanilla configuration."
                    );
                }


                return;
            }


            if (__instance.startingGameMode ==
                SaveLoadPanel.StartingGameMode.LOAD)
            {
                RestoreRuntimeConfiguration(
                    profileIndex
                );
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(SaveLoadPanel),
            "TransitToGameHelper"
        )]
        public static void TransitToGameHelperPostfix(
            SaveLoadPanel __instance)
        {
            if (__instance == null ||
                __instance.startingGameMode !=
                SaveLoadPanel.StartingGameMode.NEW)
            {
                return;
            }


            int profileIndex =
                GetTrackedProfileIndex(
                    __instance
                );


            SaveGameManager saveGameManager =
                SingletonMonoBehaviour<SaveGameManager>
                    .Instance;


            if (saveGameManager == null ||
                saveGameManager.currSaveSlot == null)
            {
                Plugin.Log.LogWarning(
                    "RANDOMIZER SAVE | " +
                    "New game reached TransitToGameHelper, " +
                    "but currSaveSlot was unavailable. " +
                    "Profile index:" +
                    profileIndex
                );

                return;
            }


            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;


            if (configuration == null)
            {
                Plugin.Log.LogWarning(
                    "RANDOMIZER SAVE | " +
                    "No active runtime configuration was available " +
                    "for profile index:" +
                    profileIndex
                );

                return;
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SAVE | " +
                "New game committed to profile index:" +
                profileIndex +
                " | Save type:" +
                saveGameManager
                    .currSaveSlot
                    .mostRecentSavedType
            );


            bool persisted =
                RandomizerSaveData.SaveConfiguration(
                    profileIndex,
                    configuration
                );


            if (!persisted)
            {
                return;
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SAVE | " +
                "Stored configuration for profile index:" +
                profileIndex +
                " | Enabled:" +
                configuration.Enabled +
                " | Seed:" +
                configuration.SeedText +
                " | Hash:" +
                configuration.SeedValue
            );
        }


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UITitleMenuScreen),
            "ContinueFromLastSavedData"
        )]
        public static void ContinueFromLastSavedDataPrefix()
        {
            SaveGameManager saveGameManager =
                SingletonMonoBehaviour<SaveGameManager>
                    .Instance;


            if (saveGameManager == null ||
                saveGameManager.currSaveSlot == null)
            {
                Plugin.Log.LogWarning(
                    "RANDOMIZER LOAD | " +
                    "Title Continue selected, but currSaveSlot was unavailable."
                );

                RandomizerRuntime.ActivateVanilla(
                    -1
                );

                return;
            }


            int profileIndex =
                FindProfileIndex(
                    saveGameManager,
                    saveGameManager.currSaveSlot
                );


            if (profileIndex < 0)
            {
                Plugin.Log.LogWarning(
                    "RANDOMIZER LOAD | " +
                    "Title Continue could not match currSaveSlot to a profile."
                );

                RandomizerRuntime.ActivateVanilla(
                    -1
                );

                return;
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER LOAD | " +
                "Title Continue matched profile index:" +
                profileIndex +
                " | Save type:" +
                saveGameManager
                    .currSaveSlot
                    .mostRecentSavedType
            );


            RestoreRuntimeConfiguration(
                profileIndex
            );
        }


        private static void RestoreRuntimeConfiguration(
            int profileIndex)
        {
            RandomizerRunConfiguration configuration;


            if (!RandomizerSaveData.TryLoadConfiguration(
                profileIndex,
                out configuration))
            {
                RandomizerRuntime.ActivateVanilla(
                    profileIndex
                );

                Plugin.Log.LogInfo(
                    "RANDOMIZER LOAD | " +
                    "No randomizer configuration restored for profile index:" +
                    profileIndex
                );

                return;
            }


            RandomizerRuntime.Activate(
                profileIndex,
                configuration
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER LOAD | " +
                "Restored profile index:" +
                profileIndex +
                " | Enabled:" +
                configuration.Enabled +
                " | Equipment:" +
                configuration.Equipment +
                " | Spells:" +
                configuration.Spells +
                " | QuestItems:" +
                configuration.QuestItems +
                " | ManaCrystals:" +
                configuration.ManaCrystals +
                " | ResourcesCollectibles:" +
                configuration.ResourcesCollectibles +
                " | ShipKey:" +
                configuration.ShipKey +
                " | InfinityKey:" +
                configuration.InfinityKey +
                " | BirdPoop:" +
                configuration.BirdPoop +
                " | MatchPlayerLevel:" +
                configuration.MatchPlayerLevel +
                " | Seed:" +
                configuration.SeedText +
                " | Hash:" +
                configuration.SeedValue
            );
        }


        private static int GetTrackedProfileIndex(
            SaveLoadPanel panel)
        {
            return Traverse.Create(
                    panel
                )
                .Field<int>(
                    "cachedTrackedSaveGameIndex"
                )
                .Value;
        }


        private static int FindProfileIndex(
            SaveGameManager saveGameManager,
            SaveGameData currentSave)
        {
            IList<SaveGameProfile> profiles =
                saveGameManager.GetSaveProfiles();


            if (profiles == null)
            {
                return -1;
            }


            for (int i = 0;
                 i < profiles.Count;
                 i++)
            {
                SaveGameProfile profile =
                    profiles[i];


                if (profile == null)
                {
                    continue;
                }


                if (ReferenceEquals(
                        profile.manualSave,
                        currentSave) ||
                    ReferenceEquals(
                        profile.autoSave,
                        currentSave))
                {
                    return i;
                }


                if (profile.manualSave != null &&
                    !string.IsNullOrEmpty(
                        profile.manualSave.SavePath) &&
                    profile.manualSave.SavePath ==
                        currentSave.SavePath)
                {
                    return i;
                }


                if (profile.autoSave != null &&
                    !string.IsNullOrEmpty(
                        profile.autoSave.autoSavePath) &&
                    profile.autoSave.autoSavePath ==
                        currentSave.autoSavePath)
                {
                    return i;
                }
            }


            return -1;
        }
    }
}
