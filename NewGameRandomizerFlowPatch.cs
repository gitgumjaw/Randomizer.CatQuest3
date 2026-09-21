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


            Transform titleTransform =
                display.transform.Find(
                    "Title"
                );

            RectTransform titleRect =
                titleTransform
                    ?.GetComponent<RectTransform>();

            CenterRect(
                titleRect,
                new Vector2(
                    -120f,
                    415f // CHANGE POSITION FOR TITLE HERE.
                )
            );


            // Master Randomizer toggle.
            // Keep the internal name because RandomizerScreen
            // already looks for Display/EnableRandomizer.
            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "EnableRandomizer",
                "",
                415f))
            {
                Object.Destroy(root);
                return null;
            }


            Transform enableTransform =
                display.transform.Find(
                    "EnableRandomizer"
                );


            // Hide the arrow graphics while leaving the controls
            // active so the UISlider keeps its normal behavior.
            Transform enableLeftArrow =
                enableTransform
                    ?.Find(
                        "HorizontalGrp/LeftButton"
                    );

            Transform enableRightArrow =
                enableTransform
                    ?.Find(
                        "HorizontalGrp/RightButton"
                    );

            DisableGraphics(
                enableLeftArrow
            );

            DisableGraphics(
                enableRightArrow
            );

            Transform enableValueBackground =
    enableTransform
        ?.Find(
            "HorizontalGrp/Slider/BG"
        );

            Graphic enableValueBackgroundGraphic =
                enableValueBackground
                    ?.GetComponent<Graphic>();

            if (enableValueBackgroundGraphic != null)
            {
                enableValueBackgroundGraphic.enabled =
                    false;
            }


            // Make Off / On look like part of the title row.
            Transform enableValueTransform =
                enableTransform
                    ?.Find(
                        "HorizontalGrp/Slider"
                    );

            RectTransform enableValueRect =
                enableValueTransform
                    ?.GetComponent<RectTransform>();

            if (enableValueRect != null)
            {
                enableValueRect.sizeDelta =
                    new Vector2(
                        180f,
                        enableValueRect.sizeDelta.y
                    );
            }


            Transform enableValueTextTransform =
                enableTransform
                    ?.Find(
                        "HorizontalGrp/Slider/BG/Text"
                    );

            TextMeshProUGUI enableValueText =
                enableValueTextTransform
                    ?.GetComponent<TextMeshProUGUI>();

            TextMeshProUGUI titleText =
                titleTransform
                    ?.GetComponent<TextMeshProUGUI>();

            if (enableValueText != null &&
                titleText != null)
            {
                enableValueText.font =
                    titleText.font;

                enableValueText.fontSharedMaterial =
                    titleText.fontSharedMaterial;

                enableValueText.fontStyle =
                    titleText.fontStyle;

                enableValueText.enableAutoSizing =
                    false;

                enableValueText.fontSize =
                    titleText.fontSize * 0.65f;
            }


            RectTransform enableValueTextRect =
                enableValueTextTransform
                    ?.GetComponent<RectTransform>();

            if (enableValueTextRect != null)
            {
                enableValueTextRect.sizeDelta =
                    new Vector2(
                        180f,
                        enableValueTextRect.sizeDelta.y
                    );

                enableValueTextRect.anchoredPosition =
                    new Vector2(
                        -65f,
                        -19f
                    );
            }


            // Put the selection hand beside the title-row value.
            Transform enableHandTransform =
                enableTransform
                    ?.Find(
                        "RandomizerHandAnchor"
                    );

            RectTransform enableHandRect =
                enableHandTransform
                    ?.GetComponent<RectTransform>();

            if (enableHandRect != null)
            {
                enableHandRect.anchoredPosition =
                    new Vector2(
                        75f,
                        -40f
                    );
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "EquipmentAndBlueprints",
                "Equipment", // CHANGE NAME PLAYER SEES HERE
                300f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "Spells",
                "Spells", // CHANGE NAME PLAYER SEES HERE
                245f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "QuestItems",
                "Quest Items", // CHANGE NAME PLAYER SEES HERE
                190f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "ManaCrystals",
                "Mana Crystals", // CHANGE NAME PLAYER SEES HERE
                135f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "ResourcesCollectibles",
                "Gold / Exp / Magic", // CHANGE NAME PLAYER SEES HERE
                80f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "ShipKey",
                "Ship Key", // CHANGE NAME PLAYER SEES HERE
                5f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "InfinityKey",
                "Infinity Key", // CHANGE NAME PLAYER SEES HERE
                -50f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "BirdPoop",
                "Bird Poop", // CHANGE NAME PLAYER SEES HERE
                -105f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateBooleanRow(
                optionsController,
                display.transform,
                "MatchPlayerLevel",
                "Drop Level = Player Level", // CHANGE NAME PLAYER SEES HERE
                -180f)) // CHANGE POSITION FOR THIS OPTION HERE.
            {
                Object.Destroy(root);
                return null;
            }


            if (!CreateSeedInput(
                optionsController,
                display.transform,
                -255f)) // CHANGE POSITION FOR SEED ROW HERE.
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


            Transform continueTransform =
                display.transform.Find(
                    "Continue"
                );

            RectTransform continueRect =
                continueTransform
                    ?.GetComponent<RectTransform>();

            CenterRect(
                continueRect,
                new Vector2(
                    0f,
                    -340f // CHANGE POSITION FOR THIS OPTION HERE.
                )
            );


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


        private static bool CreateSeedInput(
            OptionsControllerPanel optionsController,
            Transform parent,
            float yPosition)
        {
            Transform optionsRoot =
                optionsController
                    .anchoringGrp
                    ?.Find("OptionsPanelRoot");


            if (optionsRoot == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | OptionsPanelRoot not found for Seed input."
                );

                return false;
            }


            Transform rowTemplate =
                optionsRoot.Find(
                    "CameraShake"
                );


            Transform labelTemplate =
                rowTemplate
                    ?.Find(
                        "Label/Display/LabelText"
                    );

            TextMeshProUGUI labelSource =
                labelTemplate
                    ?.GetComponent<TextMeshProUGUI>();


            if (labelSource == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | Seed label text template not found."
                );

                return false;
            }


            GameObject seedObject =
                new GameObject(
                    "SeedInput",
                    typeof(RectTransform)
                );


            seedObject.transform.SetParent(
                parent,
                false
            );


            RectTransform seedRect =
                seedObject.GetComponent<RectTransform>();

            seedRect.sizeDelta =
                new Vector2(
                    900f,
                    60f
                );

            CenterRect(
                seedRect,
                new Vector2(
                    -75f, // REPOSITION SEED ROW X POSITION HERE.
                    yPosition
                )
            );


            GameObject labelObject =
                new GameObject(
                    "Label",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI)
                );


            labelObject.transform.SetParent(
                seedObject.transform,
                false
            );


            RectTransform labelRect =
                labelObject.GetComponent<RectTransform>();

            labelRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            labelRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            labelRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            labelRect.sizeDelta =
                new Vector2(
                    260f,
                    60f
                );

            labelRect.anchoredPosition =
                new Vector2(
                    -265f,
                    -2f
                );


            TextMeshProUGUI labelText =
                labelObject.GetComponent<TextMeshProUGUI>();

            CopyTextStyle(
                labelSource,
                labelText
            );

            labelText.text =
                "Seed";

            labelText.alignment =
                TextAlignmentOptions.MidlineRight;

            labelText.raycastTarget =
                false;


            GameObject inputObject =
                new GameObject(
                    "Input",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(TMP_InputField)
                );


            inputObject.transform.SetParent(
                seedObject.transform,
                false
            );


            RectTransform inputRect =
                inputObject.GetComponent<RectTransform>();

            inputRect.anchorMin =
                new Vector2(
                    0.5f,
                    0.5f
                );

            inputRect.anchorMax =
                new Vector2(
                    0.5f,
                    0.5f
                );

            inputRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            inputRect.sizeDelta =
                new Vector2(
                    245f,
                    54f
                );

            inputRect.anchoredPosition =
                new Vector2(
                    15f,
                    0f
                );


            Image inputBackground =
                inputObject.GetComponent<Image>();

            inputBackground.color =
                new Color(
                    0.25f,
                    0.08f,
                    0.08f,
                    0.18f
                );


            GameObject viewportObject =
                new GameObject(
                    "Text Area",
                    typeof(RectTransform),
                    typeof(RectMask2D)
                );


            viewportObject.transform.SetParent(
                inputObject.transform,
                false
            );


            RectTransform viewportRect =
                viewportObject.GetComponent<RectTransform>();

            viewportRect.anchorMin =
                Vector2.zero;

            viewportRect.anchorMax =
                Vector2.one;

            viewportRect.offsetMin =
                new Vector2(
                    14f,
                    5f
                );

            viewportRect.offsetMax =
                new Vector2(
                    -14f,
                    -5f
                );


            GameObject placeholderObject =
                new GameObject(
                    "Placeholder",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI)
                );


            placeholderObject.transform.SetParent(
                viewportObject.transform,
                false
            );


            RectTransform placeholderRect =
                placeholderObject.GetComponent<RectTransform>();

            placeholderRect.anchorMin =
                Vector2.zero;

            placeholderRect.anchorMax =
                Vector2.one;

            placeholderRect.offsetMin =
                Vector2.zero;

            placeholderRect.offsetMax =
                Vector2.zero;

            placeholderRect.anchoredPosition =
            new Vector2(
                placeholderRect.anchoredPosition.x,
                -7f // CHANGE POSITION FOR TEXT HERE.
            );

            TextMeshProUGUI placeholderText =
                placeholderObject.GetComponent<TextMeshProUGUI>();

            CopyTextStyle(
                labelSource,
                placeholderText
            );

            placeholderText.text =
                "Enter Seed";

            placeholderText.alignment =
                TextAlignmentOptions.BottomLeft;

            placeholderText.raycastTarget =
                false;

            Color placeholderColor =
                placeholderText.color;

            placeholderColor.a =
                0.45f;

            placeholderText.color =
                placeholderColor;


            GameObject textObject =
                new GameObject(
                    "Text",
                    typeof(RectTransform),
                    typeof(TextMeshProUGUI)
                );


            textObject.transform.SetParent(
                viewportObject.transform,
                false
            );


            RectTransform textRect =
                textObject.GetComponent<RectTransform>();

            textRect.anchorMin =
                Vector2.zero;

            textRect.anchorMax =
                Vector2.one;

            textRect.offsetMin =
                Vector2.zero;

            textRect.offsetMax =
                Vector2.zero;

            textRect.anchoredPosition =
            new Vector2(
                textRect.anchoredPosition.x,
                -7f // CHANGE POSITION FOR TEXT HERE.
            );


            TextMeshProUGUI inputText =
                textObject.GetComponent<TextMeshProUGUI>();

            CopyTextStyle(
                labelSource,
                inputText
            );

            inputText.text =
                "";

            inputText.alignment =
                TextAlignmentOptions.BottomLeft;

            inputText.raycastTarget =
                false;


            TMP_InputField inputField =
                inputObject.GetComponent<TMP_InputField>();

            inputField.textViewport =
                viewportRect;

            inputField.textComponent =
                inputText;

            inputField.placeholder =
                placeholderText;

            inputField.targetGraphic =
                inputBackground;

            inputField.lineType =
                TMP_InputField.LineType.SingleLine;

            inputField.contentType =
                TMP_InputField.ContentType.Standard;

            inputField.characterLimit =
                32;

            inputField.resetOnDeActivation =
                false;

            inputField.customCaretColor =
                true;

            inputField.caretColor =
                labelSource.color;

            inputField.caretWidth =
                3;


            if (!CreateGenerateSeedButton(
                optionsController,
                seedObject.transform))
            {
                Object.Destroy(seedObject);
                return false;
            }


            seedObject.SetActive(
                true
            );


            return true;
        }


        private static bool CreateGenerateSeedButton(
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
                    "RANDOMIZER FLOW | Generate button template not found."
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
                "Generate";


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
                    "Generate";
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


            if (rect != null)
            {
                rect.sizeDelta =
                    new Vector2(
                        220f,
                        60f
                    );
            }


            CenterRect(
                rect,
                new Vector2(
                    248f, // CHANGE GENERATE BUTTON X POSITION HERE.
                    0f
                )
            );


            // Use the Seed row itself as the coordinate system
            // for the Generate selection hand.
            GameObject handAnchorObject =
                new GameObject(
                    "GenerateHandAnchor",
                    typeof(RectTransform)
                );


            handAnchorObject.transform.SetParent(
                parent,
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
                    175f, // CHANGE GENERATE HAND X POSITION HERE.
                    -40f  // CHANGE GENERATE HAND Y POSITION HERE.
                );


            button.overrideHandIconPosX =
                true;

            button.overrideHandIconPosY =
                true;

            button.handIconOverridePos =
                handAnchor;


            buttonObject.SetActive(
                true
            );


            return true;
        }


        private static void CopyTextStyle(
            TextMeshProUGUI source,
            TextMeshProUGUI destination)
        {
            if (source == null ||
                destination == null)
            {
                return;
            }


            destination.font =
                source.font;

            destination.fontSharedMaterial =
                source.fontSharedMaterial;

            destination.fontStyle =
                source.fontStyle;

            destination.color =
                source.color;

            destination.enableAutoSizing =
                false;

            destination.fontSize =
                source.fontSize;

            destination.enableWordWrapping =
                false;

            destination.overflowMode =
                TextOverflowModes.Overflow;
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


        private static void SetGraphicsVisible(
            Transform root,
            bool visible)
        {
            if (root == null)
            {
                return;
            }


            Graphic[] graphics =
                root.GetComponentsInChildren
                    <Graphic>(
                        true
                    );


            for (int i = 0;
                 i < graphics.Length;
                 i++)
            {
                Color color =
                    graphics[i].color;

                color.a =
                    visible
                        ? 1f
                        : 0f;

                graphics[i].color =
                    color;
            }
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

        private static void DisableGraphics(
    Transform root)
        {
            if (root == null)
            {
                return;
            }


            Graphic[] graphics =
                root.GetComponentsInChildren
                    <Graphic>(
                        true
                    );


            for (int i = 0;
                 i < graphics.Length;
                 i++)
            {
                graphics[i].enabled =
                    false;
            }
        }
    }
}