using BepInEx;
using Gentlebros;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class MagicShopProgress
    {
        private const int CurrentVersion = 1;


        private static readonly HashSet<string>
            purchasedChecks =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


        private static int activeProfileIndex = -1;
        private static int activeSeedValue = 0;


        // ============================================================
        // PUBLIC PROGRESS
        // ============================================================

        public static bool IsPurchased(
            string spellGuid,
            int tier)
        {
            if (string.IsNullOrEmpty(spellGuid))
            {
                return false;
            }


            if (tier != 2 &&
                tier != 3)
            {
                return false;
            }


            return purchasedChecks.Contains(
                BuildCheckKey(
                    spellGuid,
                    tier
                )
            );
        }


        public static int GetNextTier(
            string spellGuid)
        {
            if (string.IsNullOrEmpty(spellGuid))
            {
                return 0;
            }


            if (!IsPurchased(
                    spellGuid,
                    2))
            {
                return 2;
            }


            if (!IsPurchased(
                    spellGuid,
                    3))
            {
                return 3;
            }


            return 0;
        }


        public static bool MarkPurchased(
            string spellGuid,
            int tier)
        {
            if (string.IsNullOrEmpty(spellGuid))
            {
                return false;
            }


            if (tier != 2 &&
                tier != 3)
            {
                return false;
            }


            string key =
                BuildCheckKey(
                    spellGuid,
                    tier
                );


            if (!purchasedChecks.Add(key))
            {
                return false;
            }


            Plugin.Log.LogInfo(
                "MAGIC SHOP PROGRESS | " +
                "Purchased:" +
                key +
                " | Pending game save"
            );


            // IMPORTANT:
            //
            // Do NOT write progress here.
            //
            // The lockbox state now remains in memory until
            // the game itself writes a manual save or autosave.
            // That allows loading an older game snapshot to roll
            // Magic Shop progress back with everything else.


            return true;
        }


        public static void Reset()
        {
            purchasedChecks.Clear();

            activeProfileIndex = -1;
            activeSeedValue = 0;
        }


        // ============================================================
        // NEW RUN
        // ============================================================

        private static void StartNewRun(
            int profileIndex,
            int seedValue)
        {
            purchasedChecks.Clear();


            activeProfileIndex =
                profileIndex;

            activeSeedValue =
                seedValue;


            DeleteStoredProgress(
                profileIndex
            );


            Plugin.Log.LogInfo(
                "MAGIC SHOP PROGRESS | " +
                "Started new run | " +
                "Profile:" +
                profileIndex +
                " | Seed:" +
                seedValue
            );
        }


        // ============================================================
        // LOAD SNAPSHOT
        // ============================================================

        private static void Load(
            int profileIndex,
            int seedValue,
            SaveGameType saveType)
        {
            purchasedChecks.Clear();


            activeProfileIndex =
                profileIndex;

            activeSeedValue =
                seedValue;


            if (profileIndex < 0)
            {
                return;
            }


            string path =
                GetSnapshotPath(
                    profileIndex,
                    saveType
                );


            string typeName =
                GetSaveTypeName(
                    saveType
                );


            if (!File.Exists(path))
            {
                Plugin.Log.LogInfo(
                    "MAGIC SHOP PROGRESS | " +
                    "No stored " +
                    typeName +
                    " snapshot | " +
                    "Profile:" +
                    profileIndex
                );

                return;
            }


            try
            {
                string[] lines =
                    File.ReadAllLines(
                        path
                    );


                int storedVersion = -1;
                int storedSeed = 0;


                HashSet<string> loadedChecks =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase
                    );


                for (int i = 0;
                     i < lines.Length;
                     i++)
                {
                    string line =
                        lines[i];


                    if (string.IsNullOrWhiteSpace(
                            line))
                    {
                        continue;
                    }


                    if (line.StartsWith(
                            "Version=",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(
                            line.Substring(
                                "Version=".Length
                            ),
                            out storedVersion
                        );

                        continue;
                    }


                    if (line.StartsWith(
                            "SeedValue=",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        int.TryParse(
                            line.Substring(
                                "SeedValue=".Length
                            ),
                            out storedSeed
                        );

                        continue;
                    }


                    if (line.StartsWith(
                            "Purchased=",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        string key =
                            line.Substring(
                                "Purchased=".Length
                            )
                            .Trim();


                        if (!string.IsNullOrEmpty(
                                key))
                        {
                            loadedChecks.Add(
                                key
                            );
                        }
                    }
                }


                if (storedVersion !=
                        CurrentVersion ||
                    storedSeed !=
                        seedValue)
                {
                    Plugin.Log.LogWarning(
                        "MAGIC SHOP PROGRESS | " +
                        typeName +
                        " snapshot does not match " +
                        "the active run. Ignoring. | " +
                        "Profile:" +
                        profileIndex +
                        " | StoredSeed:" +
                        storedSeed +
                        " | ActiveSeed:" +
                        seedValue
                    );


                    purchasedChecks.Clear();

                    return;
                }


                purchasedChecks.UnionWith(
                    loadedChecks
                );


                Plugin.Log.LogInfo(
                    "MAGIC SHOP PROGRESS | " +
                    "Loaded " +
                    typeName +
                    " snapshot | " +
                    "Profile:" +
                    profileIndex +
                    " | Seed:" +
                    seedValue +
                    " | PurchasedChecks:" +
                    purchasedChecks.Count
                );
            }
            catch (Exception ex)
            {
                purchasedChecks.Clear();


                Plugin.Log.LogError(
                    "MAGIC SHOP PROGRESS | " +
                    "Failed to load " +
                    typeName +
                    " snapshot | " +
                    ex
                );
            }
        }


        // ============================================================
        // SAVE SNAPSHOT
        // ============================================================

        private static void SaveSnapshot(
            SaveGameType saveType)
        {
            if (activeProfileIndex < 0)
            {
                return;
            }


            string typeName =
                GetSaveTypeName(
                    saveType
                );


            try
            {
                Directory.CreateDirectory(
                    SaveDirectory
                );


                List<string> lines =
                    new List<string>
                    {
                        "Version=" +
                            CurrentVersion,

                        "SeedValue=" +
                            activeSeedValue
                    };


                List<string> sortedChecks =
                    new List<string>(
                        purchasedChecks
                    );


                sortedChecks.Sort(
                    StringComparer.OrdinalIgnoreCase
                );


                for (int i = 0;
                     i < sortedChecks.Count;
                     i++)
                {
                    lines.Add(
                        "Purchased=" +
                        sortedChecks[i]
                    );
                }


                string path =
                    GetSnapshotPath(
                        activeProfileIndex,
                        saveType
                    );


                File.WriteAllLines(
                    path,
                    lines
                );


                Plugin.Log.LogInfo(
                    "MAGIC SHOP PROGRESS | " +
                    "Saved " +
                    typeName +
                    " snapshot | " +
                    "Profile:" +
                    activeProfileIndex +
                    " | Seed:" +
                    activeSeedValue +
                    " | PurchasedChecks:" +
                    purchasedChecks.Count
                );
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "MAGIC SHOP PROGRESS | " +
                    "Failed to save " +
                    typeName +
                    " snapshot | " +
                    ex
                );
            }
        }


        // ============================================================
        // DELETE OLD PROGRESS
        // ============================================================

        private static void DeleteStoredProgress(
            int profileIndex)
        {
            if (profileIndex < 0)
            {
                return;
            }


            TryDelete(
                GetSnapshotPath(
                    profileIndex,
                    SaveGameType.MANUAL_SAVE
                )
            );


            TryDelete(
                GetSnapshotPath(
                    profileIndex,
                    SaveGameType.AUTO_SAVE
                )
            );


            // Remove the old pre-snapshot file too.
            TryDelete(
                GetLegacyProfilePath(
                    profileIndex
                )
            );
        }


        private static void TryDelete(
            string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "MAGIC SHOP PROGRESS | " +
                    "Failed to delete old progress | " +
                    "Path:" +
                    path +
                    " | " +
                    ex
                );
            }
        }


        // ============================================================
        // PATH / TYPE HELPERS
        // ============================================================

        private static string BuildCheckKey(
            string spellGuid,
            int tier)
        {
            return
                spellGuid +
                "|" +
                tier;
        }


        private static string SaveDirectory
        {
            get
            {
                return Path.Combine(
                    Paths.ConfigPath,
                    "Randomizer.CatQuest3",
                    "SaveSlots"
                );
            }
        }


        private static string GetSnapshotPath(
            int profileIndex,
            SaveGameType saveType)
        {
            string suffix =
                saveType ==
                    SaveGameType.AUTO_SAVE
                    ? "auto"
                    : "manual";


            return Path.Combine(
                SaveDirectory,
                "profile_" +
                    profileIndex +
                    "_magic_shop_" +
                    suffix +
                    ".cfg"
            );
        }


        private static string GetLegacyProfilePath(
            int profileIndex)
        {
            return Path.Combine(
                SaveDirectory,
                "profile_" +
                    profileIndex +
                    "_magic_shop.cfg"
            );
        }


        private static string GetSaveTypeName(
            SaveGameType saveType)
        {
            return
                saveType ==
                    SaveGameType.AUTO_SAVE
                    ? "Autosave"
                    : "Manual";
        }


        private static bool PathsMatch(
            string left,
            string right)
        {
            if (string.IsNullOrWhiteSpace(left) ||
                string.IsNullOrWhiteSpace(right))
            {
                return false;
            }


            try
            {
                string leftFull =
                    Path.GetFullPath(left);

                string rightFull =
                    Path.GetFullPath(right);


                return string.Equals(
                    leftFull,
                    rightFull,
                    StringComparison.OrdinalIgnoreCase
                );
            }
            catch
            {
                return string.Equals(
                    left,
                    right,
                    StringComparison.OrdinalIgnoreCase
                );
            }
        }


        private static SaveGameType?
            GetSaveTypeFromPath(
                SaveGameData saveData,
                string path)
        {
            if (saveData == null ||
                string.IsNullOrWhiteSpace(path))
            {
                return null;
            }


            if (PathsMatch(
                    path,
                    saveData.autoSavePath))
            {
                return
                    SaveGameType.AUTO_SAVE;
            }


            if (PathsMatch(
                    path,
                    saveData.SavePath))
            {
                return
                    SaveGameType.MANUAL_SAVE;
            }


            return null;
        }


        // ============================================================
        // GAME SAVE HOOK
        // ============================================================

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(SaveGameData),
            "SaveToFile",
            new Type[]
            {
                typeof(string),
                typeof(bool)
            }
        )]
        private static void SaveToFilePostfix(
            SaveGameData __instance,
            string path,
            bool updateSavedTime)
        {
            if (__instance == null)
            {
                return;
            }


            if (activeProfileIndex < 0 ||
                __instance.saveSlotIndex !=
                    activeProfileIndex)
            {
                return;
            }


            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;


            if (configuration == null ||
                !configuration.Enabled ||
                !configuration.Spells)
            {
                return;
            }


            SaveGameType? saveType =
                GetSaveTypeFromPath(
                    __instance,
                    path
                );


            if (!saveType.HasValue)
            {
                return;
            }


            SaveSnapshot(
                saveType.Value
            );
        }


        // ============================================================
        // SAVE / LOAD MENU
        // ============================================================

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(SaveLoadPanel),
            "TransitToGameHelper"
        )]
        private static void TransitToGameHelperPostfix(
            SaveLoadPanel __instance)
        {
            if (__instance == null)
            {
                return;
            }


            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;


            if (configuration == null ||
                !configuration.Enabled ||
                !configuration.Spells)
            {
                Reset();

                return;
            }


            int profileIndex =
                RandomizerRuntime.CurrentProfileIndex;


            if (__instance.startingGameMode ==
                SaveLoadPanel.StartingGameMode.NEW)
            {
                StartNewRun(
                    profileIndex,
                    configuration.SeedValue
                );

                return;
            }


            if (__instance.startingGameMode ==
                SaveLoadPanel.StartingGameMode.LOAD)
            {
                SaveGameManager saveGameManager =
                    SingletonMonoBehaviour<
                        SaveGameManager>
                        .Instance;


                if (saveGameManager == null ||
                    saveGameManager.currSaveSlot == null)
                {
                    purchasedChecks.Clear();

                    Plugin.Log.LogWarning(
                        "MAGIC SHOP PROGRESS | " +
                        "Load completed, but currSaveSlot " +
                        "was unavailable."
                    );

                    return;
                }


                Load(
                    profileIndex,
                    configuration.SeedValue,
                    saveGameManager
                        .currSaveSlot
                        .mostRecentSavedType
                );
            }
        }


        // ============================================================
        // TITLE-SCREEN CONTINUE
        // ============================================================

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UITitleMenuScreen),
            "ContinueFromLastSavedData"
        )]
        private static void
            ContinueFromLastSavedDataPostfix()
        {
            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;


            if (configuration == null ||
                !configuration.Enabled ||
                !configuration.Spells)
            {
                Reset();

                return;
            }


            SaveGameManager saveGameManager =
                SingletonMonoBehaviour<
                    SaveGameManager>
                    .Instance;


            if (saveGameManager == null ||
                saveGameManager.currSaveSlot == null)
            {
                purchasedChecks.Clear();

                Plugin.Log.LogWarning(
                    "MAGIC SHOP PROGRESS | " +
                    "Continue completed, but currSaveSlot " +
                    "was unavailable."
                );

                return;
            }


            int profileIndex =
                RandomizerRuntime.CurrentProfileIndex;


            Load(
                profileIndex,
                configuration.SeedValue,
                saveGameManager
                    .currSaveSlot
                    .mostRecentSavedType
            );


            Plugin.Log.LogInfo(
                "MAGIC SHOP PROGRESS | " +
                "Continue restored " +
                GetSaveTypeName(
                    saveGameManager
                        .currSaveSlot
                        .mostRecentSavedType
                ) +
                " snapshot"
            );
        }


        // ============================================================
        // IN-GAME SAVE RELOAD
        // ============================================================

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(SaveGameManager),
            "ReloadCurrentSaveFromMostRecentFile"
        )]
        private static void
            ReloadCurrentSaveFromMostRecentFilePostfix(
                SaveGameManager __instance,
                bool __result)
        {
            if (!__result ||
                __instance == null ||
                __instance.currSaveSlot == null)
            {
                return;
            }


            RandomizerRunConfiguration configuration =
                RandomizerRuntime.CurrentConfiguration;


            if (configuration == null ||
                !configuration.Enabled ||
                !configuration.Spells)
            {
                return;
            }


            int profileIndex =
                RandomizerRuntime.CurrentProfileIndex;


            Load(
                profileIndex,
                configuration.SeedValue,
                __instance
                    .currSaveSlot
                    .mostRecentSavedType
            );


            Plugin.Log.LogInfo(
                "MAGIC SHOP PROGRESS | " +
                "Reloaded with game save | " +
                "Type:" +
                GetSaveTypeName(
                    __instance
                        .currSaveSlot
                        .mostRecentSavedType
                )
            );
        }
    }
}