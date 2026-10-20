using System;
using HarmonyLib;
using Gentlebros;
using ProjectStar.Data;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // Temporary, read-only playtest instrumentation.
    // Remove this file after diagnosing the persistence issue.
    [HarmonyPatch]
    public static class PersistenceDiagnostics
    {
        private const string Prefix = "PERSIST DIAG | ";
        private const string Cutlass = "f14e4bbc80f3e6f4f8c6992049be567e";
        private const string Shirt = "a9a12b1d652518c4c833750e965ad108";
        private const string Blunderpuss = "be1a97b87da865e4ba66bd2a2e67b3f9";
        private const string Flamepurr = "c0a5ef232225049c0b9b73926d8e1f0e";
        private const string Cannonpaws = "73109fddb5f110740b09af87859843f8";

        private static float nextSample;
        private static string previousState;
        private static float lastHeartbeat;
        private static int equipmentDepth;
        private static int spellDepth;

        private static bool Enabled => RandomizerRuntime.IsEnabled;

        private static void Log(string message)
        {
            if (!Enabled) return;
            Plugin.Log.LogInfo(Prefix + "t=" + Time.realtimeSinceStartup.ToString("F1") +
                " | profile=" + RandomizerRuntime.CurrentProfileIndex + " | " + message);
        }

        private static string EquipmentSaveState(string guid)
        {
            var data = SaveGameEquipmentData.currSaveFileInstance;
            if (data == null || data.equipmentSaveData == null) return "unavailable";
            int level;
            return data.equipmentSaveData.TryGetValue(guid, out level) ? "saved:L" + level : "not-saved";
        }

        private static string SpellSaveState(string guid)
        {
            var data = SaveGameSpellData.currSaveFileInstance;
            if (data == null || data.spellSaveData == null) return "unavailable";
            return data.spellSaveData.ContainsKey(guid) ? "saved" : "not-saved";
        }

        private static string EquipmentState()
        {
            return "cutlass=" + EquipmentSaveState(Cutlass) +
                ", shirt=" + EquipmentSaveState(Shirt) +
                ", blunderpuss=" + EquipmentSaveState(Blunderpuss) +
                ", flamepurr=" + SpellSaveState(Flamepurr) +
                ", cannonpaws=" + ShipSpellSaveState();
        }

        private static string ShipSpellSaveState()
        {
            var data = SaveGameShipSpellData.currSaveFileInstance;
            if (data == null || data.shipSpellSaveData == null) return "unavailable";
            return data.shipSpellSaveData.ContainsKey(Cannonpaws) ? "saved" : "not-saved";
        }

        private static string ShipState()
        {
            var settings = RandomizerState.Settings;
            if (settings == null || !settings.RandomizeShipKey) return "ship-randomization=off";

            string ownership = "unknown";
            var key = RewardDataResolver.GetQuestItem(SpecialRewards.ShipKeyGuid);
            if (key != null && key.key != null && SaveGameKeyData.currSaveFileInstance != null)
                ownership = SaveGameKeyData.currSaveFileInstance.ContainsKey(key.key).ToString();

            var contexts = Contexts.sharedInstance;
            string blocked = contexts != null && contexts.game != null
                ? contexts.game.isShipEnterBlocked.ToString() : "unavailable";

            string colliderState = "unavailable";
            var lookup = AddressableSingletonScriptableObject<RuntimeLookupTable>.Instance;
            if (lookup != null)
            {
                GameObject ship = lookup.Get(RuntimeLookupTable.Constants.playerShip);
                if (ship != null)
                {
                    SphereCollider collider = ship.GetComponentInChildren<SphereCollider>(true);
                    colliderState = collider != null ? collider.enabled.ToString() : "missing";
                }
            }
            return "key=" + ownership + ", blocked=" + blocked + ", colliderEnabled=" + colliderState;
        }

        // Sample once per second and log only changed states, plus a heartbeat every 60 seconds.
        // This can catch a transient unlocked ship without needing a precise reproduction step.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProjectStar.Main), "Update")]
        private static void MainUpdatePostfix()
        {
            if (!Enabled) { previousState = null; return; }
            if (Time.realtimeSinceStartup < nextSample) return;
            nextSample = Time.realtimeSinceStartup + 1f;
            try
            {
                string state = ShipState() + " | " + EquipmentState();
                bool changed = state != previousState;
                if (changed || Time.realtimeSinceStartup - lastHeartbeat >= 60f)
                {
                    Log((changed ? "STATE CHANGE" : "HEARTBEAT") + " | " + state);
                    previousState = state;
                    lastHeartbeat = Time.realtimeSinceStartup;
                }
            }
            catch (Exception ex)
            {
                Log("SAMPLING ERROR | " + ex.GetType().Name + ": " + ex.Message);
                nextSample = Time.realtimeSinceStartup + 10f;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AddEquipmentSystem), "PopulateDefaultMasterList")]
        private static void EquipmentPrefix()
        {
            equipmentDepth++;
            Log("DEFAULT EQUIPMENT BEGIN | depth=" + equipmentDepth + " | " + EquipmentState());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AddEquipmentSystem), "PopulateDefaultMasterList")]
        private static void EquipmentPostfix()
        {
            Log("DEFAULT EQUIPMENT END | depth=" + equipmentDepth + " | " + EquipmentState());
            equipmentDepth--;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AddSpellSystem), "ReloadAllUnlockedSpells")]
        private static void SpellsPrefix()
        {
            spellDepth++;
            Log("SPELL RELOAD BEGIN | depth=" + spellDepth + " | " + EquipmentState());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AddSpellSystem), "ReloadAllUnlockedSpells")]
        private static void SpellsPostfix()
        {
            Log("SPELL RELOAD END | depth=" + spellDepth + " | " + EquipmentState());
            spellDepth--;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AddSpellSystem), "AddSpellIfNotAddedYet")]
        private static void AddSpellPrefix(SpellConfigBase spellConfig)
        {
            if (spellConfig == null || !Enabled) return;
            if (spellConfig.Guid == Flamepurr || spellConfig.Guid == Cannonpaws)
                Log("STARTER SPELL ADD ATTEMPT | guid=" + spellConfig.Guid +
                    " | insideReload=" + (spellDepth > 0));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AddSpellSystem), "AddSpellIfNotAddedYet")]
        private static void AddSpellPostfix(SpellConfigBase spellConfig, int __result)
        {
            if (spellConfig == null || !Enabled) return;
            if (spellConfig.Guid == Flamepurr || spellConfig.Guid == Cannonpaws)
                Log("STARTER SPELL ADD RESULT | guid=" + spellConfig.Guid +
                    " | result=" + __result + " | " + EquipmentState());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveGameManager), "ReloadCurrentSaveFromMostRecentFile")]
        private static void ReloadPrefix()
        {
            Log("GAME SAVE RELOAD BEGIN | " + EquipmentState() + " | " + ShipState());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveGameManager), "ReloadCurrentSaveFromMostRecentFile")]
        private static void ReloadPostfix(bool __result)
        {
            Log("GAME SAVE RELOAD END | success=" + __result + " | " + EquipmentState() + " | " + ShipState());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SaveLoadPanel), "TransitToGameHelper")]
        private static void LoadPrefix(SaveLoadPanel __instance)
        {
            Log("LOAD TRANSIT BEGIN | mode=" + (__instance == null ? "null" : __instance.startingGameMode.ToString()));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadPanel), "TransitToGameHelper")]
        private static void LoadPostfix()
        {
            Log("LOAD TRANSIT END | " + EquipmentState() + " | " + ShipState());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UITitleMenuScreen), "ContinueFromLastSavedData")]
        private static void ContinuePrefix()
        {
            Log("TITLE CONTINUE BEGIN");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerSleepExitAnimatorStateBehaviour), "OnStateExit")]
        private static void WakePostfix()
        {
            Log("WAKE EXIT | " + ShipState());
        }
    }
}
