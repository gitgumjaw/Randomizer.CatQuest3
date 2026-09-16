using System;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class TentakeyLockState
    {
        private const string Tentakey1Guid =
            "bc103da6bbfd2fc419c24fa84be14a8a";

        private const string Tentakey2Guid =
            "c57d682e39ed68742b78ac8f10fa11ff";

        private const string Tentakey3Guid =
            "7fe0b53d50a362d42bba16375b91b18d";


        // ============================================================
        // LIVE KEY ACQUISITION
        // ============================================================

        public static void OnKeyAdded(
            KeyData key)
        {
            if (key == null)
            {
                return;
            }

            if (
                MatchesTentakey(
                    key,
                    Tentakey1Guid
                )
            )
            {
                HideLock(
                    "Tropical_Lock_01",
                    "Tentakey 1"
                );

                return;
            }

            if (
                MatchesTentakey(
                    key,
                    Tentakey2Guid
                )
            )
            {
                HideLock(
                    "Tropical_Lock_02",
                    "Tentakey 2"
                );

                return;
            }

            if (
                MatchesTentakey(
                    key,
                    Tentakey3Guid
                )
            )
            {
                HideLock(
                    "Tropical_Lock_03",
                    "Tentakey 3"
                );
            }
        }


        // ============================================================
        // QUEST COMPLETION
        // ============================================================

        public static void DetachLockForQuest(
            GameObject questObject)
        {
            if (questObject == null)
            {
                return;
            }

            string lockName;

            switch (questObject.name)
            {
                case "MainQuest_07_Key_01":

                    lockName =
                        "Tropical_Lock_01";

                    break;

                case "MainQuest_07_Key_02":

                    lockName =
                        "Tropical_Lock_02";

                    break;

                case "MainQuest_07_Key_03":

                    lockName =
                        "Tropical_Lock_03";

                    break;

                default:

                    return;
            }

            Transform lockTransform =
                questObject.transform.Find(
                    lockName
                );

            if (lockTransform == null)
            {
                Plugin.Log.LogWarning(
                    "Tentakey compatibility | " +
                    $"{lockName} not found under " +
                    $"{questObject.name}."
                );

                return;
            }

            lockTransform.SetParent(
                questObject.transform.parent,
                true
            );

            Plugin.Log.LogInfo(
                "Tentakey compatibility | " +
                $"Detached {lockName} from " +
                $"{questObject.name} before quest completion."
            );
        }


        // ============================================================
        // LOAD REFRESH
        // ============================================================

        public static void RefreshFromSave()
        {
            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return;
            }

            RefreshLock(
                "MainQuest_07_Key_01",
                "Tropical_Lock_01",
                Tentakey1Guid
            );

            RefreshLock(
                "MainQuest_07_Key_02",
                "Tropical_Lock_02",
                Tentakey2Guid
            );

            RefreshLock(
                "MainQuest_07_Key_03",
                "Tropical_Lock_03",
                Tentakey3Guid
            );
        }

        private static void RefreshLock(
            string questName,
            string lockName,
            string questItemGuid)
        {
            GameObject questObject =
                FindSceneObject(
                    questName
                );

            GameObject lockObject =
                FindSceneObject(
                    lockName
                );

            if (lockObject == null)
            {
                return;
            }

            if (
                questObject != null &&
                lockObject.transform.IsChildOf(
                    questObject.transform
                )
            )
            {
                lockObject.transform.SetParent(
                    questObject.transform.parent,
                    true
                );

                Plugin.Log.LogInfo(
                    "Tentakey compatibility | " +
                    $"Detached {lockName} on load."
                );
            }

            bool hasKey =
                HasTentakey(
                    questItemGuid
                );

            lockObject.SetActive(
                !hasKey
            );

            Plugin.Log.LogInfo(
                "Tentakey compatibility | " +
                $"{lockName} restored " +
                $"{(hasKey ? "hidden" : "visible")} " +
                $"from save."
            );
        }


        // ============================================================
        // KEY LOOKUP
        // ============================================================

        private static bool MatchesTentakey(
            KeyData addedKey,
            string questItemGuid)
        {
            QuestItem questItem =
                RewardDataResolver.GetQuestItem(
                    questItemGuid
                );

            if (questItem == null ||
                questItem.key == null)
            {
                return false;
            }

            return
                addedKey == questItem.key ||
                addedKey.Guid ==
                    questItem.key.Guid;
        }

        private static bool HasTentakey(
            string questItemGuid)
        {
            if (
                SaveGameKeyData
                    .currSaveFileInstance == null
            )
            {
                return false;
            }

            QuestItem questItem =
                RewardDataResolver.GetQuestItem(
                    questItemGuid
                );

            if (questItem == null ||
                questItem.key == null)
            {
                return false;
            }

            return SaveGameKeyData
                .currSaveFileInstance
                .ContainsKey(
                    questItem.key
                );
        }


        // ============================================================
        // LOCK CONTROL
        // ============================================================

        private static void HideLock(
            string lockName,
            string tentakeyName)
        {
            GameObject lockObject =
                FindSceneObject(
                    lockName
                );

            if (lockObject == null)
            {
                Plugin.Log.LogWarning(
                    "Tentakey compatibility | " +
                    $"{lockName} not found when " +
                    $"{tentakeyName} was obtained."
                );

                return;
            }

            lockObject.SetActive(
                false
            );

            Plugin.Log.LogInfo(
                "Tentakey compatibility | " +
                $"{tentakeyName} obtained. " +
                $"Hid {lockName}."
            );
        }


        // ============================================================
        // SCENE LOOKUP
        // ============================================================

        private static GameObject FindSceneObject(
            string objectName)
        {
            GameObject[] objects =
                Resources.FindObjectsOfTypeAll<
                    GameObject
                >();

            foreach (
                GameObject gameObject
                in objects)
            {
                if (
                    gameObject == null ||
                    gameObject.name != objectName
                )
                {
                    continue;
                }

                if (!gameObject.scene.IsValid())
                {
                    continue;
                }

                return gameObject;
            }

            return null;
        }
    }


    // ================================================================
    // ACTUAL TENTAKEY ACQUIRED
    // ================================================================

    [HarmonyPatch(
        typeof(SaveGameKeyData),
        "AddKey",
        new Type[]
        {
            typeof(KeyData)
        }
    )]
    public static class TentakeyKeyAddedPatch
    {
        public static void Postfix(
            KeyData __0)
        {
            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return;
            }

            TentakeyLockState.OnKeyAdded(
                __0
            );
        }
    }


    // ================================================================
    // SAVE LOAD COMPLETE
    // ================================================================

    [HarmonyPatch(
        typeof(PlayerSleepExitAnimatorStateBehaviour),
        "OnStateExit"
    )]
    public static class TentakeyLoadRefreshPatch
    {
        public static void Postfix()
        {
            TentakeyLockState.RefreshFromSave();
        }
    }
}