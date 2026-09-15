using HarmonyLib;
using HutongGames.PlayMaker;

namespace Randomizer.CatQuest3
{
    public static class ShipKeyTutorialHelper
    {
        public static bool IsRandomizedMainQuestState(
            FsmStateAction action,
            string stateName)
        {
            if (
                RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeShipKey
            )
            {
                return false;
            }

            if (
                action == null ||
                action.Fsm == null ||
                action.Fsm.GameObject == null
            )
            {
                return false;
            }

            return
                action.Fsm.GameObject.name ==
                    "MainQuest_01" &&
                action.Fsm.ActiveStateName ==
                    stateName;
        }

        public static int GetActiveActionIndex(
            FsmStateAction action)
        {
            if (
                action == null ||
                action.Fsm == null ||
                action.Fsm.ActiveState == null ||
                action.Fsm.ActiveState.Actions == null
            )
            {
                return -1;
            }

            FsmStateAction[] actions =
                action.Fsm.ActiveState.Actions;

            for (
                int i = 0;
                i < actions.Length;
                i++
            )
            {
                if (
                    object.ReferenceEquals(
                        actions[i],
                        action
                    )
                )
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool Skip(
            FsmStateAction action,
            string stateName,
            string actionName)
        {
            if (
                !IsRandomizedMainQuestState(
                    action,
                    stateName
                )
            )
            {
                return true;
            }

            Plugin.Log.LogInfo(
                "Ship Key tutorial rewrite | " +
                $"Skipped {actionName} in " +
                $"{stateName}."
            );

            action.Finish();

            return false;
        }

        public static bool SkipAtIndex(
            FsmStateAction action,
            string stateName,
            int actionIndex,
            string actionName)
        {
            if (
                !IsRandomizedMainQuestState(
                    action,
                    stateName
                )
            )
            {
                return true;
            }

            if (
                GetActiveActionIndex(action) !=
                    actionIndex
            )
            {
                return true;
            }

            Plugin.Log.LogInfo(
                "Ship Key tutorial rewrite | " +
                $"Skipped action {actionIndex} " +
                $"({actionName}) in {stateName}."
            );

            action.Finish();

            return false;
        }
    }

    // ============================================================
    // DEFEATED MR CLEAN
    //
    // Keep:
    //   0 AudioStopBGM
    //   1 AwardQuestItem
    //   10 AudioPlayBackZoneBGM
    //   11 CameraFollowRestoreDefault
    //
    // Skip actions 2-9.
    // ============================================================

    [HarmonyPatch(
        typeof(PlayerEnableShipEnter),
        "OnEnter"
    )]
    public static class SkipMrCleanEnableShipPatch
    {
        public static bool Prefix(
            PlayerEnableShipEnter __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Defeated Mr Clean",
                nameof(PlayerEnableShipEnter)
            );
        }
    }

    [HarmonyPatch(
        typeof(CameraMoveToLookAt),
        "OnEnter"
    )]
    public static class SkipMrCleanCameraMovePatch
    {
        public static bool Prefix(
            CameraMoveToLookAt __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Defeated Mr Clean",
                nameof(CameraMoveToLookAt)
            );
        }
    }

    [HarmonyPatch(
        typeof(AddDialogueFocus),
        "OnEnter"
    )]
    public static class SkipMrCleanDialoguePatch
    {
        public static bool Prefix(
            AddDialogueFocus __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Defeated Mr Clean",
                nameof(AddDialogueFocus)
            );
        }
    }

    [HarmonyPatch(
        typeof(SpiritMove),
        "OnEnter"
    )]
    public static class SkipSpiritMovePatch
    {
        public static bool Prefix(
            SpiritMove __instance)
        {
            if (
                ShipKeyTutorialHelper
                    .IsRandomizedMainQuestState(
                        __instance,
                        "Defeated Mr Clean"
                    )
            )
            {
                return ShipKeyTutorialHelper.Skip(
                    __instance,
                    "Defeated Mr Clean",
                    nameof(SpiritMove)
                );
            }

            if (
                ShipKeyTutorialHelper
                    .IsRandomizedMainQuestState(
                        __instance,
                        "Enter Ship"
                    )
            )
            {
                int actionIndex =
                    ShipKeyTutorialHelper
                        .GetActiveActionIndex(
                            __instance
                        );

                if (
                    actionIndex == 4 ||
                    actionIndex == 9
                )
                {
                    return ShipKeyTutorialHelper.Skip(
                        __instance,
                        "Enter Ship",
                        nameof(SpiritMove)
                    );
                }
            }

            return true;
        }
    }

    [HarmonyPatch(
        typeof(CameraAdjustFOV),
        "OnEnter"
    )]
    public static class SkipMrCleanCameraFovPatch
    {
        public static bool Prefix(
            CameraAdjustFOV __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Defeated Mr Clean",
                nameof(CameraAdjustFOV)
            );
        }
    }

    [HarmonyPatch(
        typeof(SpiritRestoreDefault),
        "OnEnter"
    )]
    public static class SkipSpiritRestorePatch
    {
        public static bool Prefix(
            SpiritRestoreDefault __instance)
        {
            if (
                ShipKeyTutorialHelper
                    .IsRandomizedMainQuestState(
                        __instance,
                        "Defeated Mr Clean"
                    )
            )
            {
                return ShipKeyTutorialHelper.Skip(
                    __instance,
                    "Defeated Mr Clean",
                    nameof(SpiritRestoreDefault)
                );
            }

            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Enter Ship",
                nameof(SpiritRestoreDefault)
            );
        }
    }

    // ============================================================
    // DOCK TUTORIAL
    // ============================================================

    [HarmonyPatch(
        typeof(AddTutorialPanel),
        "OnEnter"
    )]
    public static class SkipTutorialPanelPatch
    {
        public static bool Prefix(
            AddTutorialPanel __instance)
        {
            if (
                ShipKeyTutorialHelper
                    .IsRandomizedMainQuestState(
                        __instance,
                        "Dock Tutorial"
                    )
            )
            {
                return ShipKeyTutorialHelper.Skip(
                    __instance,
                    "Dock Tutorial",
                    nameof(AddTutorialPanel)
                );
            }

            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Ship Tutorial",
                nameof(AddTutorialPanel)
            );
        }
    }

    [HarmonyPatch(
        typeof(QuestAddPointOfInterest),
        "OnEnter"
    )]
    public static class SkipDockTutorialPoiPatch
    {
        public static bool Prefix(
            QuestAddPointOfInterest __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Dock Tutorial",
                nameof(QuestAddPointOfInterest)
            );
        }
    }

    // ============================================================
    // TALK TO SQUEAKY
    // ============================================================

    [HarmonyPatch(
        typeof(ObjectiveGoTo),
        "OnEnter"
    )]
    public static class SkipTalkToSqueakyObjectivePatch
    {
        public static bool Prefix(
            ObjectiveGoTo __instance)
        {
            // IMPORTANT:
            // Only skip the Talk to Squeaky objective.
            //
            // The ObjectiveGoTo in Ship Tutorial remains vanilla,
            // because it is what waits for the player to leave the bay.
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Talk to Squeaky",
                nameof(ObjectiveGoTo)
            );
        }
    }

    [HarmonyPatch(
        typeof(AddDialogue),
        "OnEnter"
    )]
    public static class SkipTalkToSqueakyDialoguePatch
    {
        public static bool Prefix(
            AddDialogue __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Talk to Squeaky",
                nameof(AddDialogue)
            );
        }
    }

    // ============================================================
    // ENTER SHIP
    //
    // Skip:
    //   0 ObjectiveInteractShipEnter
    //   2 Wait
    //   4 SpiritMove
    //   9 SpiritMove
    //   13 SpiritRestoreDefault
    //
    // Keep:
    //   Float popup
    //   PlayerEnableFloat
    //   Exploration cutscene
    //   Cleanup
    // ============================================================

    [HarmonyPatch(
        typeof(ObjectiveInteractShipEnter),
        "OnEnter"
    )]
    public static class SkipEnterShipObjectivePatch
    {
        public static bool Prefix(
            ObjectiveInteractShipEnter __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Enter Ship",
                nameof(ObjectiveInteractShipEnter)
            );
        }
    }

    [HarmonyPatch]
    public static class SkipEnterShipWaitPatch
    {
        public static System.Reflection.MethodBase
            TargetMethod()
        {
            System.Type waitType =
                AccessTools.TypeByName(
                    "HutongGames.PlayMaker.Actions.Wait"
                );

            if (waitType == null)
            {
                Plugin.Log.LogError(
                    "Ship Key tutorial rewrite | " +
                    "Could not find PlayMaker Wait action."
                );

                return null;
            }

            return AccessTools.Method(
                waitType,
                "OnEnter"
            );
        }

        public static bool Prefix(
            FsmStateAction __instance)
        {
            return ShipKeyTutorialHelper.SkipAtIndex(
                __instance,
                "Enter Ship",
                2,
                "Wait"
            );
        }
    }

    // ============================================================
    // FLOAT POPUP
    //
    // Change:
    //   "You found a float on the ship!"
    //
    // To:
    //   "You found a float!"
    //
    // Everything else about the vanilla popup remains unchanged.
    // ============================================================

    [HarmonyPatch(
    typeof(ShowMessagePanel),
    "OnEnter"
)]
    public static class ChangeFloatMessagePatch
    {
        public static void Prefix(
            ShowMessagePanel __instance)
        {
            if (
                !ShipKeyTutorialHelper
                    .IsRandomizedMainQuestState(
                        __instance,
                        "Enter Ship"
                    )
            )
            {
                return;
            }

            if (
                ShipKeyTutorialHelper
                    .GetActiveActionIndex(
                        __instance
                    ) != 5
            )
            {
                return;
            }

            if (
                __instance.displayInfo == null ||
                __instance.displayInfo
                    .contentMessage == null
            )
            {
                Plugin.Log.LogWarning(
                    "Ship Key tutorial rewrite | " +
                    "Could not change Float popup text."
                );

                return;
            }

            __instance.displayInfo
                .contentMessage
                .translateID =
                    string.Empty;

            __instance.displayInfo
                .contentMessage
                .content =
                    "You found a <b>Float</b>!";

            Plugin.Log.LogInfo(
                "Ship Key tutorial rewrite | " +
                "Changed Float popup text."
            );
        }
    }

    // ============================================================
    // SHIP TUTORIAL
    //
    // Hide the ship-control tutorial UI, but KEEP its ObjectiveGoTo.
    // That preserves the vanilla "leave the bay" quest trigger.
    // ============================================================

    [HarmonyPatch(
        typeof(RemoveTutorialPanel),
        "OnEnter"
    )]
    public static class SkipShipTutorialRemovePanelPatch
    {
        public static bool Prefix(
            RemoveTutorialPanel __instance)
        {
            return ShipKeyTutorialHelper.Skip(
                __instance,
                "Ship Tutorial",
                nameof(RemoveTutorialPanel)
            );
        }
    }
}