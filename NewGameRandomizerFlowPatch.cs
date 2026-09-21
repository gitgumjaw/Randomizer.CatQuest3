using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class NewGameRandomizerFlowPatch
    {
        private static RandomizerScreen
            randomizerScreen;


        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UIDifficultyMenu),
            "InitializeScreen"
        )]
        public static void InitializeDifficultyPostfix(
            UIDifficultyMenu __instance)
        {
            if (__instance == null ||
                randomizerScreen != null)
            {
                return;
            }


            UIGenderSelectionScreen genderScreen =
                __instance.showNextScreen
                    as UIGenderSelectionScreen;


            if (genderScreen == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    "Difficulty next screen is not UIGenderSelectionScreen."
                );

                return;
            }


            RandomizerScreen newScreen =
                CreateRandomizerScreen(
                    genderScreen
                );


            if (newScreen == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    "Could not create RandomizerScreen."
                );

                return;
            }


            newScreen.Configure(
                __instance
            );

            newScreen.InitializeScreen();

            randomizerScreen =
                newScreen;


            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "RandomizerScreen prepared before Difficulty."
            );
        }


        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UIScreen),
            "Show"
        )]
        public static bool ScreenShowPrefix(
            UIScreen __instance,
            UIButton activateBtn)
        {
            UIDifficultyMenu difficulty =
                __instance as UIDifficultyMenu;


            if (difficulty == null ||
                randomizerScreen == null)
            {
                return true;
            }


            SaveLoadPanel saveLoadPanel =
                GetSaveLoadPanel();


            if (saveLoadPanel == null ||
                saveLoadPanel.startingGameMode !=
                SaveLoadPanel.StartingGameMode.NEW)
            {
                return true;
            }


            if (activateBtn == null)
            {
                return true;
            }


            randomizerScreen.Show(
                activateBtn
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "New Game -> Randomizer."
            );


            return false;
        }


        private static RandomizerScreen
            CreateRandomizerScreen(
                UIGenderSelectionScreen genderScreen)
        {
            OptionsControllerPanel optionsController =
                GetOptionsController();


            if (optionsController == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    "Options visual templates not found."
                );

                return null;
            }


            GameObject root =
                new GameObject(
                    "RandomizerMenuRoot",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasGroup),
                    typeof(GraphicRaycaster)
                );


            root.SetActive(
                false
            );


            root.transform.SetParent(
                genderScreen.transform.parent,
                false
            );


            CopyRectTransform(
                genderScreen
                    .GetComponent<RectTransform>(),
                root
                    .GetComponent<RectTransform>()
            );


            CopyCanvas(
                genderScreen
                    .GetComponent<Canvas>(),
                root
                    .GetComponent<Canvas>()
            );


            GameObject display =
                new GameObject(
                    "Display",
                    typeof(RectTransform),
                    typeof(CanvasGroup)
                );


            display.transform.SetParent(
                root.transform,
                false
            );


            RectTransform displayRect =
                display.GetComponent<RectTransform>();

            displayRect.anchorMin =
                Vector2.zero;

            displayRect.anchorMax =
                Vector2.one;

            displayRect.offsetMin =
                Vector2.zero;

            displayRect.offsetMax =
                Vector2.zero;


            if (!CreateTitle(
                optionsController,
                display.transform))
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "EnableRandomizer",
                "Enable Randomizer",
                30f))
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "EquipmentAndBlueprints",
                "Equipment & Blueprints",
                -20f))
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "Spells",
                "Spells",
                -70f))
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateContinueButton(
                optionsController,
                display.transform))
            {
                Object.Destroy(root);
                return null;
            }


            if (genderScreen.backButton == null)
            {
                Object.Destroy(root);
                return null;
            }


            GameObject backObject =
                Object.Instantiate(
                    genderScreen
                        .backButton
                        .gameObject,
                    root.transform,
                    false
                );

            backObject.name =
                "BackButton";


            RandomizerScreen screen =
                root.AddComponent
                    <RandomizerScreen>();


            root.SetActive(
                true
            );


            return screen;
        }


        private static bool CreateTitle(
            OptionsControllerPanel optionsController,
            Transform parent)
        {
            Transform titleTemplate =
                optionsController
                    .anchoringGrp
                    ?.Find("OptionsTitle");


            if (titleTemplate == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | OptionsTitle template not found."
                );

                return false;
            }


            GameObject titleObject =
                Object.Instantiate(
                    titleTemplate.gameObject,
                    parent,
                    false
                );

            titleObject.name =
                "Title";


            DisableLocalization(
                titleObject
            );


            TextMeshProUGUI title =
                titleObject
                    .GetComponent<TextMeshProUGUI>();


            if (title == null)
            {
                return false;
            }


            title.text =
                "Randomizer";


            RectTransform rect =
                titleObject
                    .GetComponent<RectTransform>();


            CenterRect(
                rect,
                new Vector2(
                    0f,
                    115f
                )
            );


            return true;
        }


        private static bool CreateBooleanRow(
            OptionsControllerPanel optionsController,
            Transform parent,
            string objectName,
            string label,
            float yPosition)
        {
            Transform optionsRoot =
                optionsController
                    .anchoringGrp
                    ?.Find("OptionsPanelRoot");


            if (optionsRoot == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | OptionsPanelRoot not found."
                );

                return false;
            }


            Transform template =
                optionsRoot.Find(
                    "CameraShake"
                );


            if (template == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | CameraShake template not found."
                );

                return false;
            }


            GameObject rowObject =
                Object.Instantiate(
                    template.gameObject,
                    parent,
                    false
                );

            rowObject.name =
                objectName;


            DisableLocalization(
                rowObject
            );


            UISlider slider =
                rowObject.GetComponent<UISlider>();


            if (slider == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    objectName +
                    " has no UISlider."
                );

                return false;
            }


            slider.isTextSlider = true;
            slider.wrapValue = false;

            slider.textArray =
                new string[]
                {
                    "Off",
                    "On"
                };


            Transform labelTextTransform =
                rowObject.transform.Find(
                    "Label/Display/LabelText"
                );


            if (labelTextTransform == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    objectName +
                    " LabelText not found."
                );

                return false;
            }


            TextMeshProUGUI labelText =
                labelTextTransform
                    .GetComponent<TextMeshProUGUI>();


            if (labelText == null)
            {
                return false;
            }


            labelText.text =
                label;


            Transform hitbox =
                rowObject.transform.Find(
                    "Hitbox"
                );


            if (hitbox != null)
            {
                TextMeshProUGUI hitboxText =
                    hitbox.GetComponent<TextMeshProUGUI>();

                if (hitboxText != null)
                {
                    hitboxText.text = "";
                }
            }


            Transform valueTransform =
                rowObject.transform.Find(
                    "HorizontalGrp/Slider"
                );

            Transform leftButtonTransform =
                rowObject.transform.Find(
                    "HorizontalGrp/LeftButton"
                );

            Transform rightButtonTransform =
                rowObject.transform.Find(
                    "HorizontalGrp/RightButton"
                );


            RectTransform valueRect =
                valueTransform
                    ?.GetComponent<RectTransform>();

            RectTransform leftRect =
                leftButtonTransform
                    ?.GetComponent<RectTransform>();

            RectTransform rightRect =
                rightButtonTransform
                    ?.GetComponent<RectTransform>();


            if (valueRect != null)
            {
                valueRect.sizeDelta =
                    new Vector2(
                        100f,
                        valueRect.sizeDelta.y
                    );
            }


            if (valueRect != null &&
                leftRect != null)
            {
                leftRect.anchoredPosition =
                    new Vector2(
                        valueRect.anchoredPosition.x - 85f,
                        leftRect.anchoredPosition.y
                    );
            }


            if (valueRect != null &&
                rightRect != null)
            {
                rightRect.anchoredPosition =
                    new Vector2(
                        valueRect.anchoredPosition.x + 85f,
                        rightRect.anchoredPosition.y
                    );
            }


            Transform valueTextTransform =
                rowObject.transform.Find(
                    "HorizontalGrp/Slider/BG/Text"
                );

            RectTransform valueTextRect =
                valueTextTransform
                    ?.GetComponent<RectTransform>();


            if (valueTextRect != null)
            {
                valueTextRect.anchoredPosition =
                    new Vector2(
                        valueTextRect.anchoredPosition.x - 25f,
                        valueTextRect.anchoredPosition.y
                    );
            }


            GameObject handAnchorObject =
                new GameObject(
                    "RandomizerHandAnchor",
                    typeof(RectTransform)
                );


            handAnchorObject.transform.SetParent(
                rowObject.transform,
                false
            );


            RectTransform handAnchor =
                handAnchorObject
                    .GetComponent<RectTransform>();


            handAnchor.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            handAnchor.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            handAnchor.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );


            handAnchor.anchoredPosition =
                new Vector2(
                    -405f,
                    -40f
                );


            slider.overrideHandIconPosX = true;
            slider.overrideHandIconPosY = true;
            slider.handIconOverridePos = handAnchor;


            RectTransform rect =
                rowObject
                    .GetComponent<RectTransform>();


            CenterRect(
                rect,
                new Vector2(
                    75f,
                    yPosition
                )
            );


            rowObject.SetActive(
                true
            );


            return true;
        }


        private static bool CreateContinueButton(
            OptionsControllerPanel optionsController,
            Transform parent)
        {
            Transform optionsRoot =
                optionsController
                    .anchoringGrp
                    ?.Find("OptionsPanelRoot");


            if (optionsRoot == null)
            {
                return false;
            }


            Transform buttonsLayout =
                optionsRoot.Find(
                    "ButtonsLayoutGrp"
                );


            if (buttonsLayout == null)
            {
                return false;
            }


            Transform template =
                buttonsLayout.Find(
                    "ApplyChanges"
                );


            if (template == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | ApplyChanges template not found."
                );

                return false;
            }


            GameObject buttonObject =
                Object.Instantiate(
                    template.gameObject,
                    parent,
                    false
                );

            buttonObject.name =
                "Continue";


            DisableLocalization(
                buttonObject
            );


            UIButton button =
                buttonObject
                    .GetComponent<UIButton>();


            if (button == null)
            {
                return false;
            }


            TextMeshProUGUI text =
                buttonObject
                    .GetComponentInChildren
                        <TextMeshProUGUI>(
                            true
                        );


            if (text != null)
            {
                text.text =
                    "Continue";
            }


            button
                .OnSelectionConfirmed
                .RemoveAll();

            button
                .OnReselectionConfirmed
                .RemoveAll();


            RectTransform rect =
                buttonObject
                    .GetComponent<RectTransform>();


            CenterRect(
                rect,
                new Vector2(
                    0f,
                    -140f
                )
            );


            buttonObject.SetActive(
                true
            );


            return true;
        }


        private static void CenterRect(
            RectTransform rect,
            Vector2 position)
        {
            if (rect == null)
            {
                return;
            }


            rect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rect.anchoredPosition =
                position;
        }


        private static OptionsControllerPanel
            GetOptionsController()
        {
            if (Contexts.sharedInstance == null ||
                Contexts.sharedInstance.gUI == null ||
                !Contexts.sharedInstance.gUI.hasOptionsPanel)
            {
                return null;
            }


            return Contexts.sharedInstance
                .gUI
                .optionsPanel
                .value;
        }


        private static SaveLoadPanel
            GetSaveLoadPanel()
        {
            if (Contexts.sharedInstance == null ||
                Contexts.sharedInstance.gUI == null ||
                !Contexts.sharedInstance.gUI.hasSaveLoadPanel)
            {
                return null;
            }


            return Contexts.sharedInstance
                .gUI
                .saveLoadPanel
                .value;
        }


        private static void CopyRectTransform(
            RectTransform source,
            RectTransform destination)
        {
            if (source == null ||
                destination == null)
            {
                return;
            }


            destination.anchorMin =
                source.anchorMin;

            destination.anchorMax =
                source.anchorMax;

            destination.pivot =
                source.pivot;

            destination.anchoredPosition =
                source.anchoredPosition;

            destination.sizeDelta =
                source.sizeDelta;

            destination.localScale =
                source.localScale;

            destination.localRotation =
                source.localRotation;
        }


        private static void CopyCanvas(
            Canvas source,
            Canvas destination)
        {
            if (source == null ||
                destination == null)
            {
                return;
            }


            destination.renderMode =
                source.renderMode;

            destination.worldCamera =
                source.worldCamera;

            destination.planeDistance =
                source.planeDistance;

            destination.pixelPerfect =
                source.pixelPerfect;

            destination.overrideSorting =
                source.overrideSorting;

            destination.sortingLayerID =
                source.sortingLayerID;

            destination.sortingOrder =
                source.sortingOrder;

            destination.additionalShaderChannels =
                source.additionalShaderChannels;
        }


        private static void DisableLocalization(
            GameObject obj)
        {
            if (obj == null)
            {
                return;
            }


            Component[] components =
                obj.GetComponentsInChildren
                    <Component>(
                        true
                    );


            for (int i = 0;
                 i < components.Length;
                 i++)
            {
                Component component =
                    components[i];


                if (component == null ||
                    component.GetType().Name !=
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
    }
}