using HarmonyLib;
using System;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class ShipKeyUnlockHelper
    {
        public static bool IsShipKey(
            QuestItem questItem)
        {
            return
                questItem != null &&
                questItem.Guid ==
                    SpecialRewards.ShipKeyGuid;
        }

        public static void TryEnableShip()
        {
            if (
                RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeShipKey
            )
            {
                return;
            }

            // Float is unlocked during the rewritten
            // MainQuest_01 tutorial.
            //
            // If Float is still blocked, the player has
            // not reached that point yet, so possessing
            // the Ship Key should not unlock the ship.
            if (
                Contexts.sharedInstance.game
                    .isFloatBlocked
            )
            {
                Plugin.Log.LogInfo(
                    "Ship Key unlock | " +
                    "Ship Key acquired before Float. " +
                    "Ship remains locked."
                );

                return;
            }

            GameObject ship =
                AddressableSingletonScriptableObject<
                    RuntimeLookupTable
                >
                .Instance
                .Get(
                    RuntimeLookupTable.Constants
                        .playerShip
                );

            if (ship == null)
            {
                Plugin.Log.LogWarning(
                    "Ship Key unlock | " +
                    "Player ship object not found."
                );

                return;
            }

            SphereCollider collider =
                ship.GetComponentInChildren<
                    SphereCollider
                >();

            Contexts.sharedInstance.game
                .isShipEnterBlocked = false;

            if (collider != null)
            {
                collider.enabled = true;
            }

            Plugin.Log.LogInfo(
                "Ship Key unlock | " +
                "Ship entry enabled."
            );
        }
    }

    [HarmonyPatch(
        typeof(GameplayHelper),
        "AwardQuestItem"
    )]
    public static class ShipKeyAwardUnlockPatch
    {
        public static void Prefix(
            QuestItem questItem,
            ref Action callback)
        {
            if (
                !ShipKeyUnlockHelper
                    .IsShipKey(questItem)
            )
            {
                return;
            }

            if (
                RandomizerState.Settings == null ||
                !RandomizerState.Settings
                    .RandomizeShipKey
            )
            {
                return;
            }

            Action originalCallback =
                callback;

            callback = delegate
            {
                ShipKeyUnlockHelper
                    .TryEnableShip();

                originalCallback?.Invoke();
            };
        }
    }
}