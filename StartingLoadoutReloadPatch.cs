using HarmonyLib;
using ProjectStar.Data;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class StartingLoadoutReloadPatch
    {
        private const string
            FlamepurrRuntimeGuid =
                "c0a5ef232225049c0b9b73926d8e1f0e";

        private const string
            SuperCannonpawsRuntimeGuid =
                "73109fddb5f110740b09af87859843f8";


        private static bool isLoadingExistingSave;

        private static bool isReloadingSpells;


        // ============================================================
        // LOAD MODE
        // ============================================================

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


            if (__instance.startingGameMode ==
                SaveLoadPanel.StartingGameMode.NEW)
            {
                isLoadingExistingSave =
                    false;


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT RELOAD | " +
                    "Entering new game."
                );


                return;
            }


            if (__instance.startingGameMode ==
                SaveLoadPanel.StartingGameMode.LOAD)
            {
                isLoadingExistingSave =
                    true;


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT RELOAD | " +
                    "Entering existing save."
                );
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UITitleMenuScreen),
            "ContinueFromLastSavedData"
        )]
        public static void
            ContinueFromLastSavedDataPrefix()
        {
            isLoadingExistingSave =
                true;


            Plugin.Log.LogInfo(
                "STARTING LOADOUT RELOAD | " +
                "Title Continue entering existing save."
            );
        }


        // ============================================================
        // SPELL RELOAD SCOPE
        // ============================================================

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(AddSpellSystem),
            "ReloadAllUnlockedSpells"
        )]
        public static void
            ReloadAllUnlockedSpellsPrefix()
        {
            isReloadingSpells =
                true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(AddSpellSystem),
            "ReloadAllUnlockedSpells"
        )]
        public static void
            ReloadAllUnlockedSpellsPostfix()
        {
            isReloadingSpells =
                false;
        }


        // ============================================================
        // VANILLA STARTING SPELLS
        // ============================================================

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(AddSpellSystem),
            "AddSpellIfNotAddedYet"
        )]
        public static bool
            AddSpellIfNotAddedYetPrefix(
                SpellConfigBase spellConfig,
                ref int __result)
        {
            if (!isLoadingExistingSave ||
                !isReloadingSpells ||
                spellConfig == null ||
                !RandomizerRuntime.IsEnabled ||
                RandomizerState.Settings == null)
            {
                return true;
            }


            if (
                RandomizerState.Settings
                    .RandomizeSpells &&
                spellConfig.Guid ==
                    FlamepurrRuntimeGuid
            )
            {
                __result =
                    0;


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT RELOAD | " +
                    "Suppressed vanilla Flamepurr restoration."
                );


                return false;
            }


            if (
                RandomizerState.Settings
                    .RandomizeEquipment &&
                spellConfig.Guid ==
                    SuperCannonpawsRuntimeGuid
            )
            {
                __result =
                    0;


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT RELOAD | " +
                    "Suppressed vanilla Super Cannonpaws restoration."
                );


                return false;
            }


            return true;
        }


        // ============================================================
        // VANILLA STARTING EQUIPMENT
        // ============================================================

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(AddEquipmentSystem),
            "PopulateDefaultMasterList"
        )]
        public static bool
            PopulateDefaultMasterListPrefix()
        {
            if (!isLoadingExistingSave ||
                !IsEquipmentRandomizerActive())
            {
                return true;
            }


            SaveGameEquipmentData saveData =
                SaveGameEquipmentData
                    .currSaveFileInstance;


            if (saveData == null ||
                saveData.equipmentSaveData == null)
            {
                return true;
            }


            GameConfig gameConfig =
                AddressableSingletonScriptableObject<
                    GameConfig
                >
                    .Instance;


            if (gameConfig == null ||
                gameConfig.player == null)
            {
                return true;
            }


            if (gameConfig.player.startEquipments != null)
            {
                foreach (
                    EquipmentItemData startEquipment
                    in gameConfig.player.startEquipments)
                {
                    if (startEquipment == null)
                    {
                        continue;
                    }


                    if (!saveData
                        .equipmentSaveData
                        .TryGetValue(
                            startEquipment.Guid,
                            out int savedLevel
                        ))
                    {
                        Plugin.Log.LogInfo(
                            "STARTING LOADOUT RELOAD | " +
                            "Suppressed vanilla default equipment:" +
                            startEquipment.Guid
                        );


                        continue;
                    }


                    if (Contexts.sharedInstance.game
                        .GetEquipment(
                            startEquipment.Guid
                        ) == null)
                    {
                        AddEquipmentSystem
                            .AddEquipmentToMasterListHelper(
                                startEquipment,
                                savedLevel,
                                AddEquipmentSystem
                                    .IncrementCurrSaveEquipmentAchievement
                            );
                    }
                }
            }


            // Naked fallback weapons are infrastructure and
            // should still be populated normally.

            if (gameConfig.player
                .nakedMeleeWeapon != null)
            {
                AddEquipmentSystem
                    .AddEquipmentToMasterListHelper(
                        gameConfig.player
                            .nakedMeleeWeapon,
                        1,
                        AddEquipmentSystem
                            .IncrementCurrSaveEquipmentAchievement
                    );
            }


            if (gameConfig.player
                .nakedRangeWeapon != null)
            {
                AddEquipmentSystem
                    .AddEquipmentToMasterListHelper(
                        gameConfig.player
                            .nakedRangeWeapon,
                        1,
                        AddEquipmentSystem
                            .IncrementCurrSaveEquipmentAchievement
                    );
            }


            return false;
        }


        // ============================================================
        // STATE
        // ============================================================

        private static bool
            IsEquipmentRandomizerActive()
        {
            return
                RandomizerRuntime.IsEnabled &&
                RandomizerState.Settings != null &&
                RandomizerState.Settings
                    .RandomizeEquipment;
        }
    }
}