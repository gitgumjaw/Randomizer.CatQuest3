using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;

namespace Randomizer.CatQuest3
{
    public static class SeekerCompatibility
    {
        public static bool ShouldSkip(
            FsmStateAction action)
        {
            if (action == null ||
                action.Fsm == null ||
                action.Fsm.GameObject == null ||
                action.State == null)
            {
                return false;
            }

            if (RandomizerState.Settings == null ||
                !RandomizerState.Settings.RandomizeQuestItems)
            {
                return false;
            }

            string questName =
                action.Fsm.GameObject.name;

            string stateName =
                action.State.Name;


            // Orion unlock presentation.
            if (
                questName == "MainQuest_03" &&
                stateName ==
                    "Main Ruins Unlock (Use Quest Item)"
            )
            {
                return true;
            }


            // Antares unlock presentation.
            if (
                questName == "MainQuest_04" &&
                stateName ==
                    "Main Ruin Lock breaks (Use Quest Item)"
            )
            {
                return true;
            }


            // Centauri unlock presentation.
            if (
                questName == "MainQuest_05" &&
                stateName ==
                    "Main Ruin Lock breaks (Use Quest Item)"
            )
            {
                return true;
            }


            // Follow-up dialogue after the lock presentation.
            if (
                (
                    questName == "MainQuest_03" ||
                    questName == "MainQuest_04" ||
                    questName == "MainQuest_05"
                ) &&
                stateName ==
                    "Cappey notices something is unlocked"
            )
            {
                return true;
            }


            return false;
        }


        public static bool Skip(
            FsmStateAction action)
        {
            if (!ShouldSkip(action))
            {
                return true;
            }

            Plugin.Log.LogInfo(
                "Seeker Key compatibility | " +
                $"Skipped {action.GetType().Name} in " +
                $"{action.Fsm.GameObject.name} / " +
                $"{action.State.Name}."
            );

            action.Finish();

            return false;
        }
    }


    [HarmonyPatch]
    public static class SeekerUnlockActionPatch
    {
        public static MethodBase[] TargetMethods()
        {
            string[] actionTypes =
            {
                "AudioStopBGM",
                "CameraAdjustFOV",
                "SetInputVibration",
                "CameraShakeCamera",
                "AddDialogue",
                "AddDialogueFocus",
                "ShowMessagePanel",
                "StopInputVibration",
                "CutSceneFastForwardDisable",
                "FadeScreenToBlack",
                "OverrideSceneZoneVisualAction",
                "CameraMoveToLookAt",
                "CameraRotateByOffset",
                "HutongGames.PlayMaker.Actions.Wait",
                "RestoreFadeScreen",
                "PlayAnimationClip",
                "HideGameObject",
                "AudioPlaySound",
                "CameraFollowRestoreDefault",
                "RestoreSceneZoneVisualDetectionAction",
                "UseQuestItem",
                "AudioPlayBackZoneBGM",
                "CutSceneFastForwardEnable"
            };

            List<MethodBase> methods =
                new List<MethodBase>();

            foreach (string typeName in actionTypes)
            {
                Type type =
                    AccessTools.TypeByName(
                        typeName
                    );

                if (type == null)
                {
                    Plugin.Log.LogWarning(
                        "Seeker Key compatibility | " +
                        $"Could not find action type {typeName}."
                    );

                    continue;
                }

                MethodInfo method =
                    AccessTools.Method(
                        type,
                        "OnEnter"
                    );

                if (method == null)
                {
                    Plugin.Log.LogWarning(
                        "Seeker Key compatibility | " +
                        $"Could not find OnEnter for {typeName}."
                    );

                    continue;
                }

                methods.Add(
                    method
                );
            }

            return methods.ToArray();
        }


        public static bool Prefix(
            FsmStateAction __instance)
        {
            return SeekerCompatibility.Skip(
                __instance
            );
        }
    }
}