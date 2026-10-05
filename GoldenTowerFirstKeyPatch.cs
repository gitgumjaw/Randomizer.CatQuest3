using System.Collections.Generic;
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    public static class GoldenTowerFirstKeyState
    {
        private static readonly List<QuestAddPointOfInterest>
            waitingPointOfInterestActions =
                new List<QuestAddPointOfInterest>();


        private static readonly List<ObjectiveInteract>
            waitingGateActions =
                new List<ObjectiveInteract>();


        public static bool IsFirstFloorPointOfInterest(
            QuestAddPointOfInterest action)
        {
            if (action == null ||
                action.Fsm == null ||
                action.Fsm.GameObject == null)
            {
                return false;
            }


            return
                action.Fsm.GameObject.name ==
                    "DungeonQuest_GoldenTower" &&
                action.Fsm.ActiveStateName ==
                    "Point of Interest on gate";
        }


        public static bool IsFirstFloorGate(
            ObjectiveInteract action)
        {
            if (action == null ||
                action.Fsm == null ||
                action.Fsm.GameObject == null)
            {
                return false;
            }


            return
                action.Fsm.GameObject.name ==
                    "DungeonQuest_GoldenTower" &&
                action.Fsm.ActiveStateName ==
                    "Interact with the Gate";
        }


        public static bool HasFirstFloorKey()
        {
            if (Contexts.sharedInstance == null ||
                Contexts.sharedInstance.game == null ||
                !Contexts.sharedInstance.game
                    .hasAwardedQuestItems)
            {
                return false;
            }


            QuestItemContainer container =
                Contexts.sharedInstance.game
                    .awardedQuestItems
                    .value;


            if (container == null)
            {
                return false;
            }


            QuestItem key =
                RewardDataResolver.GetQuestItem(
                    SpecialRewards
                        .GoldenTowerFirstFloorKeyGuid
                );


            if (key == null)
            {
                Plugin.Log.LogWarning(
                    "GOLDEN TOWER KEY | " +
                    "Could not resolve First Floor key."
                );

                return false;
            }


            return container.AwardedTableContains(
                key
            );
        }


        public static void WaitForPointOfInterest(
            QuestAddPointOfInterest action)
        {
            if (!waitingPointOfInterestActions.Contains(
                    action))
            {
                waitingPointOfInterestActions.Add(
                    action
                );
            }
        }


        public static void WaitForGate(
            ObjectiveInteract action)
        {
            if (!waitingGateActions.Contains(
                    action))
            {
                waitingGateActions.Add(
                    action
                );
            }
        }


        public static void Refresh()
        {
            if (!HasFirstFloorKey())
            {
                return;
            }


            for (int i =
                     waitingPointOfInterestActions.Count - 1;
                 i >= 0;
                 i--)
            {
                QuestAddPointOfInterest action =
                    waitingPointOfInterestActions[i];


                if (!IsFirstFloorPointOfInterest(
                        action))
                {
                    waitingPointOfInterestActions.RemoveAt(
                        i
                    );

                    continue;
                }


                waitingPointOfInterestActions.RemoveAt(
                    i
                );


                Plugin.Log.LogInfo(
                    "GOLDEN TOWER KEY | " +
                    "First Floor key acquired | " +
                    "Enabling gate point of interest."
                );


                action.OnEnter();
            }


            for (int i =
                     waitingGateActions.Count - 1;
                 i >= 0;
                 i--)
            {
                ObjectiveInteract action =
                    waitingGateActions[i];


                if (!IsFirstFloorGate(
                        action))
                {
                    waitingGateActions.RemoveAt(
                        i
                    );

                    continue;
                }


                waitingGateActions.RemoveAt(
                    i
                );


                Plugin.Log.LogInfo(
                    "GOLDEN TOWER KEY | " +
                    "First Floor key acquired | " +
                    "Enabling gate interaction."
                );


                action.OnEnter();
            }
        }
    }


    // ============================================================
    // FIRST FLOOR GATE POINT OF INTEREST
    //
    // Do not show the flashing gate marker until the actual
    // randomized First Floor Golden Key is owned.
    //
    // Holding this action also keeps the quest from progressing
    // into the gate-interaction state prematurely.
    // ============================================================

    [HarmonyPatch(
        typeof(QuestAddPointOfInterest),
        "OnEnter"
    )]
    public static class
        GoldenTowerFirstKeyPointOfInterestPatch
    {
        public static bool Prefix(
            QuestAddPointOfInterest __instance)
        {
            if (!GoldenTowerFirstKeyState
                    .IsFirstFloorPointOfInterest(
                        __instance))
            {
                return true;
            }


            if (GoldenTowerFirstKeyState
                    .HasFirstFloorKey())
            {
                Plugin.Log.LogInfo(
                    "GOLDEN TOWER KEY | " +
                    "First Floor key already owned | " +
                    "Gate point of interest enabled."
                );

                return true;
            }


            Plugin.Log.LogInfo(
                "GOLDEN TOWER KEY | " +
                "Waiting for First Floor key | " +
                "Gate point of interest blocked."
            );


            GoldenTowerFirstKeyState
                .WaitForPointOfInterest(
                    __instance
                );


            return false;
        }
    }


    // ============================================================
    // FIRST FLOOR GATE
    //
    // Secondary safety check. Normally the FSM cannot reach this
    // state until the point-of-interest action above is released.
    // ============================================================

    [HarmonyPatch(
        typeof(ObjectiveInteract),
        "OnEnter"
    )]
    public static class
        GoldenTowerFirstKeyGatePatch
    {
        public static bool Prefix(
            ObjectiveInteract __instance)
        {
            if (!GoldenTowerFirstKeyState
                    .IsFirstFloorGate(
                        __instance))
            {
                return true;
            }


            if (GoldenTowerFirstKeyState
                    .HasFirstFloorKey())
            {
                Plugin.Log.LogInfo(
                    "GOLDEN TOWER KEY | " +
                    "First Floor key already owned | " +
                    "Gate interaction enabled."
                );

                return true;
            }


            Plugin.Log.LogInfo(
                "GOLDEN TOWER KEY | " +
                "Waiting for First Floor key | " +
                "Gate interaction blocked."
            );


            GoldenTowerFirstKeyState
                .WaitForGate(
                    __instance
                );


            return false;
        }
    }


    // ============================================================
    // ACTUAL QUEST ITEM ACQUISITION
    //
    // When the randomized First Floor Golden Key enters the
    // player's real QuestItem inventory, release whichever
    // Golden Tower action is currently waiting for it.
    // ============================================================

    [HarmonyPatch(
        typeof(QuestItemContainer),
        "AddToAwardedTable"
    )]
    public static class
        GoldenTowerFirstKeyAwardPatch
    {
        public static void Postfix(
            QuestItem questItem)
        {
            if (questItem == null ||
                questItem.Guid !=
                    SpecialRewards
                        .GoldenTowerFirstFloorKeyGuid)
            {
                return;
            }


            Plugin.Log.LogInfo(
                "GOLDEN TOWER KEY | " +
                "First Floor key entered inventory."
            );


            GoldenTowerFirstKeyState.Refresh();
        }
    }
}