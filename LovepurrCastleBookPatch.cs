using HarmonyLib;
using HutongGames.PlayMaker;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class LovepurrCastleBookState
    {
        private static readonly List<WaitForKeyAcquired>
            waitingActions =
                new List<WaitForKeyAcquired>();


        public static bool TryGetBookGuid(
            WaitForKeyAcquired action,
            out string bookGuid)
        {
            bookGuid = null;


            if (action == null ||
                action.Fsm == null ||
                action.Fsm.GameObject == null)
            {
                return false;
            }


            string objectName =
                action.Fsm.GameObject.name;

            string stateName =
                action.Fsm.ActiveStateName;


            if (
                objectName ==
                    "DungeonQuest_LovepurrCastle_Book_01" &&
                stateName ==
                    "Wait for player to get 1st book"
            )
            {
                bookGuid =
                    SpecialRewards.LovepurrBook1Guid;

                return true;
            }


            if (
                objectName ==
                    "DungeonQuest_LovepurrCastle_Book_02" &&
                stateName ==
                    "Wait for player to get 2nd book"
            )
            {
                bookGuid =
                    SpecialRewards.LovepurrBook2Guid;

                return true;
            }


            if (
                objectName ==
                    "DungeonQuest_LovepurrCastle_Book_03" &&
                stateName ==
                    "Wait for player to get 3rd book"
            )
            {
                bookGuid =
                    SpecialRewards.LovepurrBook3Guid;

                return true;
            }


            return false;
        }


        public static bool HasBook(
            string bookGuid)
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


            QuestItem book =
                RewardDataResolver.GetQuestItem(
                    bookGuid
                );


            if (book == null)
            {
                Plugin.Log.LogWarning(
                    $"LOVEBOOK CASTLE | " +
                    $"Could not resolve book | " +
                    $"Guid:{bookGuid}"
                );

                return false;
            }


            return container.AwardedTableContains(
                book
            );
        }


        public static void Wait(
            WaitForKeyAcquired action)
        {
            if (!waitingActions.Contains(
                    action))
            {
                waitingActions.Add(
                    action
                );
            }
        }


        public static void Remove(
            WaitForKeyAcquired action)
        {
            waitingActions.Remove(
                action
            );
        }


        public static void Refresh()
        {
            for (int i =
                     waitingActions.Count - 1;
                 i >= 0;
                 i--)
            {
                WaitForKeyAcquired action =
                    waitingActions[i];


                if (!TryGetBookGuid(
                        action,
                        out string bookGuid))
                {
                    waitingActions.RemoveAt(
                        i
                    );

                    continue;
                }


                if (!HasBook(
                        bookGuid))
                {
                    continue;
                }


                waitingActions.RemoveAt(
                    i
                );


                Plugin.Log.LogInfo(
                    $"LOVEBOOK CASTLE | " +
                    $"Actual book acquired | " +
                    $"Object:{action.Fsm.GameObject.name} | " +
                    $"Book:{bookGuid}"
                );


                action.Finish();


                PlayMakerUtils.UpdateFsmThisFrame(
                    action.Fsm
                );
            }
        }
    }


    // ============================================================
    // CASTLE BOOK WAIT
    //
    // The vanilla FSM waits for the hidden KeyData associated
    // with each LoveBook.
    //
    // The randomizer must keep those hidden keys tied to the
    // vanilla locations so the Chapter 1 -> 2 -> 3 sequence works.
    //
    // Therefore these three castle states instead wait for the
    // actual randomized QuestItem to be owned.
    // ============================================================

    [HarmonyPatch(
        typeof(WaitForKeyAcquired),
        "OnEnter"
    )]
    public static class
        LovepurrCastleBookWaitPatch
    {
        public static bool Prefix(
            WaitForKeyAcquired __instance)
        {
            if (!LovepurrCastleBookState
                    .TryGetBookGuid(
                        __instance,
                        out string bookGuid))
            {
                return true;
            }


            if (LovepurrCastleBookState
                    .HasBook(
                        bookGuid))
            {
                Plugin.Log.LogInfo(
                    $"LOVEBOOK CASTLE | " +
                    $"Book already owned | " +
                    $"Object:" +
                    $"{__instance.Fsm.GameObject.name} | " +
                    $"Book:{bookGuid}"
                );


                __instance.Finish();


                PlayMakerUtils.UpdateFsmThisFrame(
                    __instance.Fsm
                );


                return false;
            }


            Plugin.Log.LogInfo(
                $"LOVEBOOK CASTLE | " +
                $"Waiting for actual book | " +
                $"Object:" +
                $"{__instance.Fsm.GameObject.name} | " +
                $"Book:{bookGuid}"
            );


            LovepurrCastleBookState.Wait(
                __instance
            );


            return false;
        }
    }


    // Remove an abandoned/disabled waiter so we do not retain
    // stale PlayMaker actions after scene/state changes.

    [HarmonyPatch(
        typeof(WaitForKeyAcquired),
        "OnDisabled"
    )]
    public static class
        LovepurrCastleBookDisabledPatch
    {
        public static void Prefix(
            WaitForKeyAcquired __instance)
        {
            LovepurrCastleBookState.Remove(
                __instance
            );
        }
    }


    // ============================================================
    // ACTUAL QUEST ITEM ACQUISITION
    //
    // Every real QuestItem award ultimately enters the awarded
    // QuestItemContainer here. Refresh any castle FSM that is
    // waiting for one of the randomized LoveBooks.
    // ============================================================

    [HarmonyPatch(
        typeof(QuestItemContainer),
        "AddToAwardedTable"
    )]
    public static class
        LovepurrCastleBookAwardPatch
    {
        public static void Postfix(
            QuestItem questItem)
        {
            if (questItem == null)
            {
                return;
            }


            if (
                questItem.Guid !=
                    SpecialRewards.LovepurrBook1Guid &&
                questItem.Guid !=
                    SpecialRewards.LovepurrBook2Guid &&
                questItem.Guid !=
                    SpecialRewards.LovepurrBook3Guid
            )
            {
                return;
            }


            Plugin.Log.LogInfo( 
                $"LOVEBOOK CASTLE | " +
                $"Actual LoveBook entered inventory | " +
                $"QuestItem:{questItem.Guid}"
            );


            LovepurrCastleBookState.Refresh();
        }
    }
}