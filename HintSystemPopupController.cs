
using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    // HintSystemPopupController.cs
    // Updated: 2026-10-23
    //
    // Displays a hint before a PlayMaker dialogue action.
    // After the hint is dismissed, the original dialogue resumes.
    //
    // The caller is responsible for identifying an eligible
    // hint source and supplying its formatted hint text.

    internal static class HintSystemPopupController
    {
        private static bool popupActive;
        private static bool resumingVanilla;

        private static readonly MethodInfo StartDialogueMethod =
            AccessTools.Method(
                typeof(AddDialogueFocus),
                "StartDialogue");

        public static bool IsResumingVanilla =>
            resumingVanilla;

        public static bool IsPopupActive =>
            popupActive;

        // Returns true if the original dialogue was intercepted.
        // Returns false if it should proceed normally.
        public static bool TryShowBeforeDialogue(
            AddDialogueFocus dialogueAction,
            string spoilerText)
        {
            if (popupActive || resumingVanilla)
                return false;

            if (dialogueAction == null ||
                string.IsNullOrEmpty(spoilerText))
                return false;

            if (StartDialogueMethod == null ||
                Plugin.Instance == null)
                return false;

            MessageUIInfo popupInfo;

            try
            {
                popupInfo = CreateHintMessage(
                    dialogueAction,
                    spoilerText);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "HINT SYSTEM | Failed to prepare popup: " +
                    ex);

                return false;
            }

            bool inputPushed = false;

            try
            {
                popupActive = true;

                Contexts.sharedInstance.input
                    .CreateInputStateCommand(
                        InputStateCommand.Push,
                        InputState.UI);

                inputPushed = true;

                Contexts.sharedInstance.gUI
                    .messagePanel.value.Show(
                        popupInfo,
                        delegate
                        {
                            Plugin.Log.LogInfo(
                                "HINT SYSTEM | Hint dismissed.");

                            Plugin.Instance.StartCoroutine(
                                ResumeOriginalDialogue(
                                    dialogueAction));
                        });

                Plugin.Log.LogInfo(
                    "HINT SYSTEM | Hint opened.");

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "HINT SYSTEM | Failed to show popup: " +
                    ex);

                if (inputPushed)
                {
                    try
                    {
                        Contexts.sharedInstance.input
                            .CreateInputStateCommand(
                                InputStateCommand.Pop);
                    }
                    catch (Exception cleanupEx)
                    {
                        Plugin.Log.LogError(
                            "HINT SYSTEM | Input cleanup failed: " +
                            cleanupEx);
                    }
                }

                popupActive = false;
                return false;
            }
        }


        private static MessageUIInfo CreateHintMessage(
            AddDialogueFocus dialogueAction,
            string spoilerText)
        {
            return new MessageUIInfo
            {
                displaySprite = null,
                displaySpriteSizeFactor = 1f,

                contentMessage = new Line
                {
                    content = spoilerText,
                    translateID = "",
                    duration = 1f,
                    textVibrationEffect = false
                },

                showChoicePrompts = false,
                showOnlyCancelPrompt = false,
                allowMouseInput = true,
                initiatorPlayerID = -1
            };
        }

        private static IEnumerator ResumeOriginalDialogue(
            AddDialogueFocus dialogueAction)
        {
            // Preserve the timing proven by our Belmont Boat test.
            // Allow the hint panel to complete its UI cleanup first.

            yield return null;

            yield return new WaitForSecondsRealtime(
                0.1f);

            try
            {
                Contexts.sharedInstance.input
                    .CreateInputStateCommand(
                        InputStateCommand.Pop);

                resumingVanilla = true;

                Plugin.Log.LogInfo(
                    "HINT SYSTEM | Resuming original dialogue.");

                StartDialogueMethod.Invoke(
                    dialogueAction,
                    null);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "HINT SYSTEM | Failed to resume dialogue: " +
                    ex);
            }
            finally
            {
                resumingVanilla = false;
                popupActive = false;
            }
        }


        // Displays a spoiler before an original ShowMessagePanel action.
        // Returns true when the original action should be deferred.
        public static bool TryShowBeforeMessage(
            ShowMessagePanel messageAction,
            string spoilerText)
        {
            if (popupActive || resumingVanilla)
                return false;

            if (messageAction == null ||
                messageAction.displayInfo == null ||
                string.IsNullOrEmpty(spoilerText) ||
                Plugin.Instance == null)
                return false;

            MessageUIInfo originalInfo =
                messageAction.displayInfo;

            MessageUIInfo hintInfo = new MessageUIInfo
            {
                displaySprite = null,
                displaySpriteSizeFactor = 1f,

                contentMessage = new Line
                {
                    content = spoilerText,
                    translateID = "",
                    duration = 1f,
                    textVibrationEffect = false
                },

                showChoicePrompts = false,
                showOnlyCancelPrompt = false,
                allowMouseInput = originalInfo.allowMouseInput,
                initiatorPlayerID = originalInfo.initiatorPlayerID
            };

            bool inputPushed = false;

            try
            {
                popupActive = true;

                Contexts.sharedInstance.input
                    .CreateInputStateCommand(
                        InputStateCommand.Push,
                        InputState.UI);

                inputPushed = true;

                Contexts.sharedInstance.gUI
                    .messagePanel.value.Show(
                        hintInfo,
                        delegate
                        {
                            Plugin.Log.LogInfo(
                                "HINT SYSTEM | Message-first hint dismissed.");

                            Plugin.Instance.StartCoroutine(
                                ResumeOriginalMessage(messageAction));
                        });

                Plugin.Log.LogInfo(
                    "HINT SYSTEM | Message-first hint opened.");

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "HINT SYSTEM | Failed to show message-first spoiler: " +
                    ex);

                if (inputPushed)
                {
                    try
                    {
                        Contexts.sharedInstance.input
                            .CreateInputStateCommand(
                                InputStateCommand.Pop);
                    }
                    catch (Exception cleanupEx)
                    {
                        Plugin.Log.LogError(
                            "HINT SYSTEM | Message input cleanup failed: " +
                            cleanupEx);
                    }
                }

                popupActive = false;
                return false;
            }
        }

        private static IEnumerator ResumeOriginalMessage(
            ShowMessagePanel messageAction)
        {
            // Let the spoiler's dismissal finish before opening
            // the vanilla inscription on the same message panel.
            yield return null;

            yield return new WaitForSecondsRealtime(0.1f);

            try
            {
                Contexts.sharedInstance.input
                    .CreateInputStateCommand(
                        InputStateCommand.Pop);

                resumingVanilla = true;

                Plugin.Log.LogInfo(
                    "HINT SYSTEM | Resuming original inscription.");

                // The Harmony prefix will allow this invocation
                // because IsResumingVanilla is true.
                messageAction.OnEnter();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "HINT SYSTEM | Failed to resume inscription: " +
                    ex);
            }
            finally
            {
                resumingVanilla = false;
                popupActive = false;
            }
        }

    }
}
