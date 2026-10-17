using BepInEx;
using Gentlebros;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace Randomizer.CatQuest3
{
    // Claims are deliberately independent of vanilla highest-cleared difficulty.
    // A claim must be recorded by the reward delivery path, not by the clear hook.
    [HarmonyPatch]
    public static class TavernTalesProgress
    {
        private const int Version = 1;
        private static readonly HashSet<string> claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static int profile = -1;
        private static int seed;

        private static bool Enabled
        {
            get
            {
                RandomizerRunConfiguration c = RandomizerRuntime.CurrentConfiguration;
                return c != null && c.Enabled && c.TavernTalesParrots;
            }
        }

        public static string LocationKey(string encounterName, int difficulty)
        {
            if (string.IsNullOrWhiteSpace(encounterName) || difficulty < 1 || difficulty > 3)
                return null;
            return "RANDOMIZER|TAVERN_TALES|" + encounterName + "|" + difficulty;
        }

        public static bool IsClaimed(string encounterName, int difficulty)
        {
            string key = LocationKey(encounterName, difficulty);
            return key != null && claimed.Contains(key);
        }

        // Only call after the actual randomized reward has been granted.
        public static bool MarkClaimed(string encounterName, int difficulty)
        {
            if (!Enabled) return false;
            string key = LocationKey(encounterName, difficulty);
            if (key == null || !claimed.Add(key)) return false;
            Plugin.Log.LogInfo("TAVERN PROGRESS | Claimed:" + key + " | Pending game save");
            return true;
        }

        public static void Reset()
        {
            claimed.Clear();
            profile = -1;
            seed = 0;
        }

        private static string DirectoryPath => Path.Combine(Paths.ConfigPath, "Randomizer.CatQuest3", "SaveSlots");
        private static string SnapshotPath(int slot, SaveGameType type)
        {
            return Path.Combine(DirectoryPath, "profile_" + slot + "_tavern_tales_" +
                (type == SaveGameType.AUTO_SAVE ? "auto" : "manual") + ".cfg");
        }

        private static void Start(int slot, int seedValue)
        {
            claimed.Clear();
            profile = slot;
            seed = seedValue;
            if (slot < 0) return;
            foreach (SaveGameType type in new[] { SaveGameType.AUTO_SAVE, SaveGameType.MANUAL_SAVE })
            {
                try
                {
                    string path = SnapshotPath(slot, type);
                    if (File.Exists(path)) File.Delete(path);
                }
                catch (Exception ex) { Plugin.Log.LogError("TAVERN PROGRESS | New run cleanup failed: " + ex); }
            }
            Plugin.Log.LogInfo("TAVERN PROGRESS | New run | Profile:" + slot + " | Seed:" + seed);
        }

        private static void Load(int slot, int seedValue, SaveGameType type)
        {
            claimed.Clear();
            profile = slot;
            seed = seedValue;
            if (slot < 0) return;
            string path = SnapshotPath(slot, type);
            if (!File.Exists(path))
            {
                Plugin.Log.LogInfo("TAVERN PROGRESS | No " + type + " snapshot | Profile:" + slot);
                return;
            }
            try
            {
                string[] lines = File.ReadAllLines(path);
                int storedVersion = -1, storedSeed = 0;
                HashSet<string> loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string line in lines)
                {
                    if (line.StartsWith("Version=", StringComparison.OrdinalIgnoreCase))
                        int.TryParse(line.Substring(8), out storedVersion);
                    else if (line.StartsWith("SeedValue=", StringComparison.OrdinalIgnoreCase))
                        int.TryParse(line.Substring(10), out storedSeed);
                    else if (line.StartsWith("Claimed=", StringComparison.OrdinalIgnoreCase))
                    {
                        string key = line.Substring(8).Trim();
                        if (key.StartsWith("RANDOMIZER|TAVERN_TALES|", StringComparison.OrdinalIgnoreCase))
                            loaded.Add(key);
                    }
                }
                if (storedVersion != Version || storedSeed != seedValue)
                {
                    Plugin.Log.LogWarning("TAVERN PROGRESS | Snapshot version or seed mismatch; ignoring | Profile:" + slot);
                    return;
                }
                claimed.UnionWith(loaded);
                Plugin.Log.LogInfo("TAVERN PROGRESS | Loaded " + type + " | Profile:" + slot + " | Claimed:" + claimed.Count);
            }
            catch (Exception ex) { Plugin.Log.LogError("TAVERN PROGRESS | Load failed: " + ex); }
        }

        private static void Save(SaveGameType type)
        {
            if (profile < 0) return;
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                List<string> lines = new List<string> { "Version=" + Version, "SeedValue=" + seed };
                List<string> keys = new List<string>(claimed);
                keys.Sort(StringComparer.OrdinalIgnoreCase);
                foreach (string key in keys) lines.Add("Claimed=" + key);
                File.WriteAllLines(SnapshotPath(profile, type), lines);
                Plugin.Log.LogInfo("TAVERN PROGRESS | Saved " + type + " | Profile:" + profile + " | Claimed:" + claimed.Count);
            }
            catch (Exception ex) { Plugin.Log.LogError("TAVERN PROGRESS | Save failed: " + ex); }
        }

        private static bool SamePath(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            try { return string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase); }
            catch { return string.Equals(a, b, StringComparison.OrdinalIgnoreCase); }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveGameData), "SaveToFile", new Type[] { typeof(string), typeof(bool) })]
        private static void SavePostfix(SaveGameData __instance, string path, bool updateSavedTime)
        {
            if (!Enabled || __instance == null || profile < 0 || __instance.saveSlotIndex != profile) return;
            if (SamePath(path, __instance.autoSavePath)) Save(SaveGameType.AUTO_SAVE);
            else if (SamePath(path, __instance.SavePath)) Save(SaveGameType.MANUAL_SAVE);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadPanel), "TransitToGameHelper")]
        private static void GameStartPostfix(SaveLoadPanel __instance)
        {
            if (__instance == null) return;
            if (!Enabled) { Reset(); return; }
            RandomizerRunConfiguration config = RandomizerRuntime.CurrentConfiguration;
            int slot = RandomizerRuntime.CurrentProfileIndex;
            if (__instance.startingGameMode == SaveLoadPanel.StartingGameMode.NEW)
                Start(slot, config.SeedValue);
            else if (__instance.startingGameMode == SaveLoadPanel.StartingGameMode.LOAD)
            {
                SaveGameManager manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
                if (manager == null || manager.currSaveSlot == null) { Reset(); return; }
                Load(slot, config.SeedValue, manager.currSaveSlot.mostRecentSavedType);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UITitleMenuScreen), "ContinueFromLastSavedData")]
        private static void ContinuePostfix()
        {
            if (!Enabled) { Reset(); return; }
            SaveGameManager manager = SingletonMonoBehaviour<SaveGameManager>.Instance;
            if (manager == null || manager.currSaveSlot == null) { Reset(); return; }
            Load(RandomizerRuntime.CurrentProfileIndex, RandomizerRuntime.CurrentConfiguration.SeedValue,
                manager.currSaveSlot.mostRecentSavedType);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveGameManager), "ReloadCurrentSaveFromMostRecentFile")]
        private static void ReloadPostfix(SaveGameManager __instance, bool __result)
        {
            if (!__result || __instance == null || __instance.currSaveSlot == null || !Enabled) return;
            Load(RandomizerRuntime.CurrentProfileIndex, RandomizerRuntime.CurrentConfiguration.SeedValue,
                __instance.currSaveSlot.mostRecentSavedType);
        }
    }
}
