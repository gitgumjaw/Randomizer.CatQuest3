using System;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class SeekerLockState
    {
        private const string OrionGuid =
            "d437c226478785e4b8f7b058ed3a9728";

        private const string AntaresGuid =
            "9ed0dd9195f53d741ac56fba693b50c6";

        private const string CentauriGuid =
            "5a36b5c0341129b408cc08b37a33b84c";


        public static void OnKeyAdded(
            KeyData key)
        {
            if (key == null)
            {
                return;
            }

            if (MatchesKey(key, OrionGuid))
            {
                HideLock(
                    "Tropical_Lock_Orion",
                    "Orion"
                );

                return;
            }

            if (MatchesKey(key, AntaresGuid))
            {
                HideLock(
                    "Tropical_Lock_Antares",
                    "Antares"
                );

                return;
            }

            if (MatchesKey(key, CentauriGuid))
            {
                HideLock(
                    "Tropical_Lock_Centauris",
                    "Centauri"
                );
            }
        }


        public static void RefreshFromSave()
        {
            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return;
            }

            RefreshLock(
                "Tropical_Lock_Orion",
                OrionGuid
            );

            RefreshLock(
                "Tropical_Lock_Antares",
                AntaresGuid
            );

            RefreshLock(
                "Tropical_Lock_Centauris",
                CentauriGuid
            );
        }


        private static void RefreshLock(
            string lockName,
            string questItemGuid)
        {
            GameObject lockObject =
                FindSceneObject(lockName);

            if (lockObject == null)
            {
                return;
            }

            bool hasKey =
                HasKey(questItemGuid);

            lockObject.SetActive(!hasKey);

            Plugin.Log.LogInfo(
                "Seeker Key compatibility | " +
                $"{lockName} restored " +
                $"{(hasKey ? "hidden" : "visible")} " +
                "from save."
            );
        }


        private static bool MatchesKey(
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


        private static bool HasKey(
            string questItemGuid)
        {
            if (SaveGameKeyData
                    .currSaveFileInstance == null)
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


        private static void HideLock(
            string lockName,
            string keyName)
        {
            GameObject lockObject =
                FindSceneObject(lockName);

            if (lockObject == null)
            {
                Plugin.Log.LogWarning(
                    "Seeker Key compatibility | " +
                    $"{lockName} not found when " +
                    $"{keyName} Key was obtained."
                );

                return;
            }

            lockObject.SetActive(false);

            Plugin.Log.LogInfo(
                "Seeker Key compatibility | " +
                $"{keyName} Key obtained. " +
                $"Hid {lockName}."
            );
        }


        private static GameObject FindSceneObject(
            string objectName)
        {
            GameObject[] objects =
                Resources.FindObjectsOfTypeAll<
                    GameObject
                >();

            foreach (GameObject gameObject in objects)
            {
                if (gameObject == null ||
                    gameObject.name != objectName)
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


    [HarmonyPatch(
        typeof(SaveGameKeyData),
        "AddKey",
        new Type[]
        {
            typeof(KeyData)
        }
    )]
    public static class SeekerKeyAddedPatch
    {
        public static void Postfix(
            KeyData __0)
        {
            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return;
            }

            SeekerLockState.OnKeyAdded(__0);
        }
    }


    [HarmonyPatch(
        typeof(PlayerSleepExitAnimatorStateBehaviour),
        "OnStateExit"
    )]
    public static class SeekerKeyLoadRefreshPatch
    {
        public static void Postfix()
        {
            SeekerLockState.RefreshFromSave();
        }
    }
}