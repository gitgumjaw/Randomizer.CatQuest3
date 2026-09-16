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

        public static bool HasShipKey()
        {
            QuestItem shipKey =
                RewardDataResolver.GetQuestItem(
                    SpecialRewards.ShipKeyGuid
                );

            if (
                shipKey == null ||
                shipKey.key == null
            )
            {
                Plugin.Log.LogWarning(
                    "Ship Key state | " +
                    "Could not resolve Ship Key data."
                );

                return false;
            }

            if (
                SaveGameKeyData.currSaveFileInstance ==
                    null
            )
            {
                Plugin.Log.LogWarning(
                    "Ship Key state | " +
                    "Save key data is not available."
                );

                return false;
            }

            return
                SaveGameKeyData
                    .currSaveFileInstance
                    .ContainsKey(
                        shipKey.key
                    );
        }

        public static void RefreshShipState()
        {
            if (
                RandomizerState.Settings == null ||
                !RandomizerState.Settings
                    .RandomizeShipKey
            )
            {
                return;
            }

            bool hasShipKey =
                HasShipKey();

            Contexts.sharedInstance.game
                .isShipEnterBlocked =
                    !hasShipKey;

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
                    "Ship Key state | " +
                    "Player ship object was not found."
                );

                return;
            }

            SphereCollider collider =
                ship.GetComponentInChildren<
                    SphereCollider
                >();

            if (collider == null)
            {
                Plugin.Log.LogWarning(
                    "Ship Key state | " +
                    "Player ship collider was not found."
                );

                return;
            }

            collider.enabled =
                hasShipKey;

            Plugin.Log.LogInfo(
                "Ship Key state | " +
                $"Ship entry " +
                $"{(hasShipKey ? "enabled" : "disabled")}."
            );
        }
    }

    // ============================================================
    // SHIP KEY ACQUISITION
    // ============================================================

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
                    .RefreshShipState();

                originalCallback?.Invoke();
            };
        }
    }

    // ============================================================
    // WAKE ANIMATION DIAGNOSTIC
    //
    // For now, only prove whether these callbacks actually run
    // during the load/wake sequence.
    // ============================================================

    [HarmonyPatch(
    typeof(PlayerSleepExitAnimatorStateBehaviour),
    "OnStateExit"
)]
    public static class ShipKeySleepExitRefreshPatch
    {
        public static void Postfix()
        {
            ShipKeyUnlockHelper
                .RefreshShipState();
        }
    }
}