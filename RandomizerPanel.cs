using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Randomizer.CatQuest3
{
    public static class RandomizerPanel
    {
        private static bool isOpen;

        private static readonly List<GameObject>
            hiddenObjects =
                new List<GameObject>();

        private static readonly List<bool>
            hiddenObjectStates =
                new List<bool>();

        private static readonly List<Behaviour>
            localizationBehaviours =
                new List<Behaviour>();

        private static readonly List<bool>
            localizationStates =
                new List<bool>();

        private static readonly List<GameObject>
            dummyButtons =
                new List<GameObject>();

        private static TextMeshProUGUI titleText;

        private static string originalTitleText;


        private static readonly FieldInfo
            ControllerPanelField =
                AccessTools.Field(
                    typeof(UIInventorySubPanel),
                    "controllerPanel"
                );

        private static readonly MethodInfo
            SetupNavigationForButtonsMethod =
                AccessTools.Method(
                    typeof(UIScreen),
                    "SetupNavigationForButtons"
                );


        public static void Open(
            UITitleMenuScreen titleScreen,
            UIButton activateButton)
        {
            if (isOpen ||
                titleScreen == null)
            {
                return;
            }


            OptionsControllerPanel optionsController =
                GetOptionsPanel();

            if (optionsController == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "Could not find native Options panel."
                );

                return;
            }


            UIControllerPanel titleController =
                GetTitleController(
                    titleScreen
                );

            if (titleController == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "Could not find title controller."
                );

                return;
            }


            if (!PrepareRandomizerShell(
                optionsController))
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "Could not prepare Randomizer shell."
                );

                RestoreOptionsView();

                return;
            }


            isOpen = true;


            optionsController.navigationTracker =
                titleController.navigationTracker;

            optionsController.closePanelBtn =
                titleController.closePanelBtn;

            optionsController.previousScreen =
                titleScreen;

            optionsController.activationBtns.Clear();


            titleScreen.Hide(
                playAnim: false
            );


            optionsController
                .onReadyForInput
                .AddOnce(
                    delegate
                    {
                        ConfigureTestButtons(
                            optionsController
                        );
                    }
                );


            optionsController
                .onPanelHideCompleted
                .AddOnce(
                    delegate
                    {
                        RestoreOptionsView();

                        isOpen = false;

                        Plugin.Log.LogInfo(
                            "RANDOMIZER PANEL | Closed."
                        );
                    }
                );


            optionsController.Show(
                activateButton
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER PANEL | Opened."
            );
        }


        private static bool PrepareRandomizerShell(
            OptionsControllerPanel optionsController)
        {
            Transform anchoring =
                optionsController.anchoringGrp;

            if (anchoring == null)
            {
                return false;
            }


            Transform titleTransform =
                anchoring.Find(
                    "OptionsTitle"
                );

            if (titleTransform == null)
            {
                return false;
            }


            titleText =
                titleTransform
                    .GetComponent<TextMeshProUGUI>();

            if (titleText == null)
            {
                return false;
            }


            originalTitleText =
                titleText.text;


            DisableLocalizationAndRemember(
                titleTransform.gameObject
            );


            titleText.text =
                "Randomizer";


            HideAndRemember(
                anchoring.Find(
                    "CategoryTabs"
                )
            );

            HideAndRemember(
                anchoring.Find(
                    "KeyBindingRoot"
                )
            );

            HideAndRemember(
                anchoring.Find(
                    "OptionsDescriptionsRoot"
                )
            );


            return true;
        }


        private static void ConfigureTestButtons(
            OptionsControllerPanel optionsController)
        {
            Transform anchoring =
                optionsController.anchoringGrp;

            if (anchoring == null)
            {
                return;
            }


            Transform optionsRoot =
                anchoring.Find(
                    "OptionsPanelRoot"
                );

            if (optionsRoot == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "Could not find OptionsPanelRoot."
                );

                return;
            }


            OptionsPanel optionsPanel =
                optionsRoot
                    .GetComponent<OptionsPanel>();

            if (optionsPanel == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "OptionsPanelRoot has no OptionsPanel."
                );

                return;
            }

            optionsPanel.canvas.enabled = true;

            Transform buttonsLayout =
                optionsRoot.Find(
                    "ButtonsLayoutGrp"
                );

            if (buttonsLayout == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "Could not find ButtonsLayoutGrp."
                );

                return;
            }


            Transform buttonTemplate =
                buttonsLayout.Find(
                    "ApplyChanges"
                );

            if (buttonTemplate == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "Could not find button template."
                );

                return;
            }


            for (int i = 0;
                 i < optionsRoot.childCount;
                 i++)
            {
                Transform child =
                    optionsRoot.GetChild(i);

                if (child == buttonsLayout)
                {
                    continue;
                }

                HideAndRemember(
                    child
                );
            }


            for (int i = 0;
                 i < buttonsLayout.childCount;
                 i++)
            {
                Transform child =
                    buttonsLayout.GetChild(i);

                HideAndRemember(
                    child
                );
            }


            UIButton optionA =
                CreateTestButton(
                    buttonTemplate.gameObject,
                    buttonsLayout,
                    "TestOptionA",
                    "Test Option A",
                    optionsController
                );

            UIButton optionB =
                CreateTestButton(
                    buttonTemplate.gameObject,
                    buttonsLayout,
                    "TestOptionB",
                    "Test Option B",
                    optionsController
                );

            UIButton optionC =
                CreateTestButton(
                    buttonTemplate.gameObject,
                    buttonsLayout,
                    "TestOptionC",
                    "Test Option C",
                    optionsController
                );


            if (optionA == null ||
                optionB == null ||
                optionC == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER PANEL | " +
                    "Could not create all test buttons."
                );

                return;
            }


            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                buttonsLayout
                    .GetComponent<RectTransform>()
            );

            Canvas.ForceUpdateCanvases();


            if (SetupNavigationForButtonsMethod != null)
            {
                SetupNavigationForButtonsMethod.Invoke(
                    optionsPanel,
                    new object[]
                    {
                        false
                    }
                );
            }


            optionsPanel.defaultButtonSelection =
                optionA;


            optionsController
                .navigationTracker
                .RemoveTrackPanelSelectables(
                    optionsPanel
                );

            optionsController
                .navigationTracker
                .AddTrackPanelSelectables(
                    optionsPanel
                );


            foreach (GameEntity player
                     in Contexts.sharedInstance.game
                         .GetGroup(GameMatcher.PlayerId))
            {
                optionA.Select(
                    player.playerId.value,
                    isMouseInput: false,
                    playAudio: false
                );
            }


            optionsController
                .navigationTracker
                .SnapAllHandIcons(
                    optionA
                );

            optionsController
                .navigationTracker
                .ShowHandIcons();


            LogCanvasDiagnostics(
                optionsRoot,
                "OPTIONS ROOT"
            );

            LogButtonDiagnostics(
                optionA,
                "A"
            );

            LogButtonDiagnostics(
                optionB,
                "B"
            );

            LogButtonDiagnostics(
                optionC,
                "C"
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER PANEL | " +
                "Test controls ready."
            );
        }


        private static UIButton CreateTestButton(
            GameObject template,
            Transform parent,
            string objectName,
            string label,
            OptionsControllerPanel optionsController)
        {
            GameObject obj =
                UnityEngine.Object.Instantiate(
                    template,
                    parent
                );

            obj.name =
                objectName;


            DisableLocalizationOnClone(
                obj
            );


            UIButton button =
                obj.GetComponent<UIButton>();

            if (button == null)
            {
                UnityEngine.Object.Destroy(
                    obj
                );

                return null;
            }


            TextMeshProUGUI text =
                obj.GetComponentInChildren
                    <TextMeshProUGUI>(
                        true
                    );

            if (text != null)
            {
                text.text =
                    label;
            }


            button
                .OnSelectionConfirmed
                .RemoveAll();

            button
                .OnReselectionConfirmed
                .RemoveAll();


            button
                .OnSelectionConfirmed
                .AddListener(
                    delegate (
                        UIButton selectedButton,
                        int playerIndex)
                    {
                        Plugin.Log.LogInfo(
                            "RANDOMIZER PANEL | " +
                            label +
                            " selected."
                        );


                        selectedButton
                            .DeselectConfirm();

                        selectedButton
                            .Select(
                                playerIndex,
                                isMouseInput: false,
                                playAudio: false
                            );

                        optionsController
                            .navigationTracker
                            .SnapPlayerHandIcon(
                                playerIndex,
                                selectedButton
                            );
                    }
                );


            obj.SetActive(
                true
            );


            dummyButtons.Add(
                obj
            );


            return button;
        }


        private static void LogCanvasDiagnostics(
            Transform optionsRoot,
            string label)
        {
            Canvas canvas =
                optionsRoot.GetComponent<Canvas>();

            RectTransform rect =
                optionsRoot.GetComponent<RectTransform>();

            CanvasGroup group =
                optionsRoot.GetComponent<CanvasGroup>();


            Plugin.Log.LogInfo(
                "RANDOMIZER CANVAS DIAG | " +
                $"{label} | " +
                $"ActiveSelf:{optionsRoot.gameObject.activeSelf} | " +
                $"ActiveHierarchy:{optionsRoot.gameObject.activeInHierarchy}"
            );


            if (canvas != null)
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER CANVAS DIAG | " +
                    $"{label} | " +
                    $"CanvasEnabled:{canvas.enabled} | " +
                    $"RenderMode:{canvas.renderMode} | " +
                    $"OverrideSorting:{canvas.overrideSorting} | " +
                    $"SortingOrder:{canvas.sortingOrder} | " +
                    $"SortingLayer:{canvas.sortingLayerName}"
                );
            }
            else
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER CANVAS DIAG | " +
                    $"{label} | Canvas:<null>"
                );
            }


            if (group != null)
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER CANVAS DIAG | " +
                    $"{label} | " +
                    $"CanvasGroupAlpha:{group.alpha} | " +
                    $"Interactable:{group.interactable} | " +
                    $"BlocksRaycasts:{group.blocksRaycasts}"
                );
            }


            if (rect != null)
            {
                Vector3[] corners =
                    new Vector3[4];

                rect.GetWorldCorners(
                    corners
                );

                Plugin.Log.LogInfo(
                    "RANDOMIZER CANVAS DIAG | " +
                    $"{label} | " +
                    $"WorldBL:{corners[0]} | " +
                    $"WorldTL:{corners[1]} | " +
                    $"WorldTR:{corners[2]} | " +
                    $"WorldBR:{corners[3]}"
                );
            }
        }


        private static void LogButtonDiagnostics(
            UIButton button,
            string label)
        {
            if (button == null)
            {
                return;
            }


            GameObject obj =
                button.gameObject;

            RectTransform buttonRect =
                obj.GetComponent<RectTransform>();

            TextMeshProUGUI text =
                obj.GetComponentInChildren
                    <TextMeshProUGUI>(
                        true
                    );


            Plugin.Log.LogInfo(
                "RANDOMIZER UI DIAG | " +
                $"{label} | " +
                $"Button:{obj.name} | " +
                $"ActiveSelf:{obj.activeSelf} | " +
                $"ActiveHierarchy:{obj.activeInHierarchy} | " +
                $"ButtonState:{button.buttonState}"
            );


            if (buttonRect != null)
            {
                Vector3[] corners =
                    new Vector3[4];

                buttonRect.GetWorldCorners(
                    corners
                );

                Plugin.Log.LogInfo(
                    "RANDOMIZER UI DIAG | " +
                    $"{label} | " +
                    $"RectPos:{buttonRect.anchoredPosition} | " +
                    $"RectSize:{buttonRect.rect.size} | " +
                    $"WorldBL:{corners[0]} | " +
                    $"WorldTL:{corners[1]} | " +
                    $"WorldTR:{corners[2]} | " +
                    $"WorldBR:{corners[3]}"
                );
            }


            CanvasRenderer buttonRenderer =
                obj.GetComponent<CanvasRenderer>();

            if (buttonRenderer != null)
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER UI DIAG | " +
                    $"{label} | " +
                    $"ButtonRendererCull:{buttonRenderer.cull} | " +
                    $"ButtonRendererAlpha:{buttonRenderer.GetAlpha()}"
                );
            }


            if (text == null)
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER UI DIAG | " +
                    $"{label} | Text:<null>"
                );

                return;
            }


            RectTransform textRect =
                text.GetComponent<RectTransform>();

            CanvasRenderer textRenderer =
                text.GetComponent<CanvasRenderer>();


            Plugin.Log.LogInfo(
                "RANDOMIZER UI DIAG | " +
                $"{label} | " +
                $"Text:'{text.text}' | " +
                $"TextEnabled:{text.enabled} | " +
                $"TextAlpha:{text.alpha} | " +
                $"TextColor:{text.color}"
            );


            if (textRect != null)
            {
                Vector3[] corners =
                    new Vector3[4];

                textRect.GetWorldCorners(
                    corners
                );

                Plugin.Log.LogInfo(
                    "RANDOMIZER UI DIAG | " +
                    $"{label} | " +
                    $"TextPos:{textRect.anchoredPosition} | " +
                    $"TextSize:{textRect.rect.size} | " +
                    $"TextWorldBL:{corners[0]} | " +
                    $"TextWorldTL:{corners[1]} | " +
                    $"TextWorldTR:{corners[2]} | " +
                    $"TextWorldBR:{corners[3]}"
                );
            }


            if (textRenderer != null)
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER UI DIAG | " +
                    $"{label} | " +
                    $"TextRendererCull:{textRenderer.cull} | " +
                    $"TextRendererAlpha:{textRenderer.GetAlpha()}"
                );
            }


            Canvas parentCanvas =
                obj.GetComponentInParent<Canvas>();

            if (parentCanvas != null)
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER UI DIAG | " +
                    $"{label} | " +
                    $"ParentCanvas:{parentCanvas.name} | " +
                    $"CanvasEnabled:{parentCanvas.enabled} | " +
                    $"SortingOrder:{parentCanvas.sortingOrder}"
                );
            }
        }


        private static void HideAndRemember(
            Transform transform)
        {
            if (transform == null)
            {
                return;
            }


            GameObject obj =
                transform.gameObject;


            if (hiddenObjects.Contains(
                obj))
            {
                return;
            }


            hiddenObjects.Add(
                obj
            );

            hiddenObjectStates.Add(
                obj.activeSelf
            );

            obj.SetActive(
                false
            );
        }


        private static void DisableLocalizationAndRemember(
            GameObject obj)
        {
            Component[] components =
                obj.GetComponents<Component>();

            foreach (Component component
                     in components)
            {
                if (component == null)
                {
                    continue;
                }


                if (component
                        .GetType()
                        .Name !=
                    "LocalizeStringEvent")
                {
                    continue;
                }


                Behaviour behaviour =
                    component as Behaviour;

                if (behaviour == null)
                {
                    continue;
                }


                localizationBehaviours.Add(
                    behaviour
                );

                localizationStates.Add(
                    behaviour.enabled
                );

                behaviour.enabled =
                    false;
            }
        }


        private static void DisableLocalizationOnClone(
            GameObject obj)
        {
            Component[] components =
                obj.GetComponentsInChildren<Component>(
                    true
                );

            foreach (Component component
                     in components)
            {
                if (component == null)
                {
                    continue;
                }


                if (component
                        .GetType()
                        .Name !=
                    "LocalizeStringEvent")
                {
                    continue;
                }


                Behaviour behaviour =
                    component as Behaviour;

                if (behaviour != null)
                {
                    behaviour.enabled =
                        false;
                }
            }
        }


        private static void RestoreOptionsView()
        {
            foreach (GameObject obj
                     in dummyButtons)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(
                        obj
                    );
                }
            }

            dummyButtons.Clear();


            if (titleText != null)
            {
                titleText.text =
                    originalTitleText;
            }


            int localizationCount =
                Mathf.Min(
                    localizationBehaviours.Count,
                    localizationStates.Count
                );

            for (int i = 0;
                 i < localizationCount;
                 i++)
            {
                Behaviour behaviour =
                    localizationBehaviours[i];

                if (behaviour != null)
                {
                    behaviour.enabled =
                        localizationStates[i];
                }
            }


            int hiddenCount =
                Mathf.Min(
                    hiddenObjects.Count,
                    hiddenObjectStates.Count
                );

            for (int i = 0;
                 i < hiddenCount;
                 i++)
            {
                GameObject obj =
                    hiddenObjects[i];

                if (obj != null)
                {
                    obj.SetActive(
                        hiddenObjectStates[i]
                    );
                }
            }


            hiddenObjects.Clear();
            hiddenObjectStates.Clear();

            localizationBehaviours.Clear();
            localizationStates.Clear();

            titleText = null;
            originalTitleText = null;
        }


        private static OptionsControllerPanel
            GetOptionsPanel()
        {
            GUIContext gui =
                Contexts.sharedInstance.gUI;

            if (gui == null ||
                !gui.hasOptionsPanel)
            {
                return null;
            }


            return gui.optionsPanel.value;
        }


        private static UIControllerPanel
            GetTitleController(
                UITitleMenuScreen titleScreen)
        {
            if (ControllerPanelField == null)
            {
                return null;
            }


            return ControllerPanelField.GetValue(
                titleScreen
            ) as UIControllerPanel;
        }
    }
}