using HarmonyLib;
using HutongGames.PlayMaker;
using System;
using System.Reflection;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class TentakeyCompatibility
    {
        private const string UnlockState =
            "Tentacle Gate Unlocks";

        public static bool ShouldSkip(
            FsmStateAction action)
        {
            if (action == null ||
                action.Fsm == null ||
                action.Fsm.GameObject == null)
            {
                return false;
            }

            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return false;
            }

            if (action.Fsm.ActiveStateName != UnlockState)
            {
                return false;
            }

            string questName =
                action.Fsm.GameObject.name;

            return
                questName == "MainQuest_07_Key_01" ||
                questName == "MainQuest_07_Key_02" ||
                questName == "MainQuest_07_Key_03";
        }

        public static bool Skip(
            FsmStateAction action)
        {
            if (!ShouldSkip(action))
            {
                return true;
            }

            action.Finish();

            return false;
        }
    }


    [HarmonyPatch(
        typeof(CameraMoveToLookAt),
        "OnEnter"
    )]
    public static class
        TentakeySkipCameraMoveToLookAtPatch
    {
        public static bool Prefix(
            CameraMoveToLookAt __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(CameraRotateByOffset),
        "OnEnter"
    )]
    public static class
        TentakeySkipCameraRotateByOffsetPatch
    {
        public static bool Prefix(
            CameraRotateByOffset __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(AudioPlaySound),
        "OnEnter"
    )]
    public static class
        TentakeySkipAudioPlaySoundPatch
    {
        public static bool Prefix(
            AudioPlaySound __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(PlayAnimationClip),
        "OnEnter"
    )]
    public static class
        TentakeySkipPlayAnimationClipPatch
    {
        public static bool Prefix(
            PlayAnimationClip __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(HideGameObject),
        "OnEnter"
    )]
    public static class
        TentakeySkipHideGameObjectPatch
    {
        public static bool Prefix(
            HideGameObject __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(FadeScreenToBlack),
        "OnEnter"
    )]
    public static class
        TentakeySkipFadeScreenToBlackPatch
    {
        public static bool Prefix(
            FadeScreenToBlack __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(CameraFollowRestoreDefault),
        "OnEnter"
    )]
    public static class
        TentakeySkipCameraFollowRestoreDefaultPatch
    {
        public static bool Prefix(
            CameraFollowRestoreDefault __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(RestoreFadeScreen),
        "OnEnter"
    )]
    public static class
        TentakeySkipRestoreFadeScreenPatch
    {
        public static bool Prefix(
            RestoreFadeScreen __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch(
        typeof(SpiritMove),
        "OnEnter"
    )]
    public static class
        TentakeySkipSpiritMovePatch
    {
        public static bool Prefix(
            SpiritMove __instance)
        {
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }


    [HarmonyPatch]
    public static class
        TentakeySkipWaitPatch
    {
        public static MethodBase TargetMethod()
        {
            Type waitType =
                AccessTools.TypeByName(
                    "HutongGames.PlayMaker.Actions.Wait"
                );

            if (waitType == null)
            {
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
            return TentakeyCompatibility.Skip(
                __instance
            );
        }
    }

    [HarmonyPatch(
    typeof(QuestComplete),
    "OnEnter"
)]
    public static class
    TentakeyLockReparentPatch
    {
        public static void Prefix(
            QuestComplete __instance)
        {
            if (__instance == null ||
                __instance.Fsm == null ||
                __instance.Fsm.GameObject == null)
            {
                return;
            }

            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return;
            }

            TentakeyLockState
                .DetachLockForQuest(
                    __instance.Fsm.GameObject
                );
        }
    }

    [HarmonyPatch(
    typeof(GameplayHelper),
    "AwardQuestItem"
)]
    public static class
    TentakeyVanillaChestAwardPatch
    {
        public static bool Prefix(
    QuestItem questItem,
    Action callback)
        {
            if (!ShouldSuppress(
                    questItem,
                    callback))
            {
                return true;
            }

            Plugin.Log.LogInfo(
                $"Tentakey compatibility | " +
                $"Suppressed vanilla chest award | " +
                $"QuestItem:{questItem.Guid}"
            );

            // The vanilla Tentakey quests react to the KeyData
            // event fired by SaveGameKeyData.AddKey().
            //
            // We need that progression event to occur even though
            // this vanilla location must not actually give the
            // player the Tentakey.
            if (questItem.key != null)
            {
                Relays.keyEvents
                    .GetKeyEvent(questItem.key)
                    .Dispatch();

                Plugin.Log.LogInfo(
                    $"Tentakey compatibility | " +
                    $"Dispatched vanilla key progression event | " +
                    $"Key:{questItem.key.Guid}"
                );
            }
            else
            {
                Plugin.Log.LogWarning(
                    $"Tentakey compatibility | " +
                    $"Could not dispatch key progression event | " +
                    $"QuestItem:{questItem.Guid}"
                );
            }

            // Continue the completion path that DropQuestItem()
            // supplied to GameplayHelper.AwardQuestItem().
            callback?.Invoke();

            return false;
        }

        private static bool ShouldSuppress(
            QuestItem questItem,
            Action callback)
        {
            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return false;
            }

            if (questItem == null)
            {
                return false;
            }

            bool isTentakey =
                questItem.Guid ==
                    "bc103da6bbfd2fc419c24fa84be14a8a" ||
                questItem.Guid ==
                    "c57d682e39ed68742b78ac8f10fa11ff";

            if (!isTentakey)
            {
                return false;
            }

            if (callback == null ||
                callback.Method == null)
            {
                return false;
            }

            string methodName =
                callback.Method.Name;

            Type declaringType =
                callback.Method.DeclaringType;

            string declaringTypeName =
                declaringType != null
                    ? declaringType.FullName
                    : string.Empty;

            return
                methodName.Contains(
                    "DropQuestItem"
                ) &&
                declaringTypeName.Contains(
                    "ChestBehaviour"
                );
        }
    }
}