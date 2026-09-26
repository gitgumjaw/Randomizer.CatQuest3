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
}