using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class RandomizerMenuBuilder
    {
        private static RandomizerMenuController randomizerScreen;

        // ------------------------------------------------------------
        // LAYOUT NUMBERS
        // ------------------------------------------------------------
        // These are the main values to tweak once functionality is done.

        private const float TitleY = 425f;

        private const float Column1X = -430f;
        private const float Column2X = 0f;
        private const float Column3X = 430f;

        private const float FirstRowY = 275f;
        private const float RowSpacing = 55f;

        private const float SeedY = -245f;
        private const float ContinueY = -335f;

        private const float ToggleWidth = 400f;
        private const float ToggleHeight = 42f;

        // Distance between the hand pointer and the first visible character
        // of each toggle line.
        internal const float ToggleHandPadding = -15f;
        internal const float ToggleHandYOffset = -30;

        // Visual contrast for disabled randomizer options.
        // 1f = fully opaque; lower values fade OFF rows more.
        internal const float OffToggleAlpha = 0.65f;

        private const float MasterToggleWidth = 700f;
        private const float MasterToggleHeight = 76f;


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

            RandomizerMenuController newScreen =
                CreateRandomizerMenuController(
                    genderScreen
                );

            if (newScreen == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    "Could not create RandomizerMenuController."
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
                "RandomizerMenuController prepared before Difficulty."
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


        private static RandomizerMenuController
            CreateRandomizerMenuController(
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

            RectTransform rootRect =
                root.GetComponent<RectTransform>();

            rootRect.anchorMin =
                Vector2.zero;

            rootRect.anchorMax =
                Vector2.one;

            rootRect.pivot =
                new Vector2(
                    0.5f,
                    0.5f
                );

            rootRect.offsetMin =
                Vector2.zero;

            rootRect.offsetMax =
                Vector2.zero;

            rootRect.anchoredPosition =
                Vector2.zero;

            rootRect.localScale =
                Vector3.one;

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


            // --------------------------------------------------------
            // TITLE / MASTER TOGGLE
            // --------------------------------------------------------

            if (!CreateToggleButton(
                optionsController,
                display.transform,
                "EnableRandomizer",
                "Randomizer",
                0f,
                TitleY,
                MasterToggleWidth,
                MasterToggleHeight,
                true))
            {
                Object.Destroy(root);
                return null;
            }


            // --------------------------------------------------------
            // COLUMN 1 — MAIN RANDOMIZER CATEGORIES
            // --------------------------------------------------------

            if (!CreateToggleButton(
                optionsController,
                display.transform,
                "EquipmentAndBlueprints",
                "Equipment & Blueprints",
                Column1X,
                FirstRowY,
                ToggleWidth,
                ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "Spells",
                    "Spells",
                    Column1X,
                    FirstRowY - RowSpacing,
                    ToggleWidth,
                    ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "QuestItems",
                    "Quest Items",
                    Column1X,
                    FirstRowY - RowSpacing * 2f,
                    ToggleWidth,
                    ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "ManaCrystals",
                    "Mana Crystals",
                    Column1X,
                    FirstRowY - RowSpacing * 3f,
                    ToggleWidth,
                    ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "ResourcesCollectibles",
                    "Gold / Exp / Magic",
                    Column1X,
                    FirstRowY - RowSpacing * 4f,
                    ToggleWidth,
                    ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "TavernTalesParrots",
                    "Tavern Tales Parrots",
                    Column1X,
                    FirstRowY - RowSpacing * 5f,
                    ToggleWidth,
                    ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "MatchPlayerLevel",
                    "Loot Drops at Player Level",
                    Column1X,
                    FirstRowY - RowSpacing * 6.5f,
                    ToggleWidth,
                    ToggleHeight))
            {
                Object.Destroy(root);
                return null;
            }


            // --------------------------------------------------------
            // COLUMN 2 — SPECIAL / SUPPLEMENTAL OPTIONS
            // --------------------------------------------------------

            if (!CreateToggleButton(
                optionsController,
                display.transform,
                "ShipKey",
                "Ship Key",
                Column2X,
                FirstRowY,
                ToggleWidth,
                ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "InfinityKey",
                    "Infinity Key",
                    Column2X,
                    FirstRowY - RowSpacing,
                    ToggleWidth,
                    ToggleHeight) ||

                !CreateToggleButton(
                    optionsController,
                    display.transform,
                    "BirdPoop",
                    "Bird Poop",
                    Column2X,
                    FirstRowY - RowSpacing * 2f,
                    ToggleWidth,
                    ToggleHeight))
            {
                Object.Destroy(root);
                return null;
            }


            // --------------------------------------------------------
            // COLUMN 3 — FUTURE ENEMY RANDOMIZER OPTIONS
            // --------------------------------------------------------

            if (!CreateToggleButton(
                optionsController,
                display.transform,
                "EnemyMode",
                "Enemies",
                Column3X,
                FirstRowY,
                ToggleWidth,
                ToggleHeight))
            {
                Object.Destroy(root);
                return null;
            }


            // --------------------------------------------------------
            // SEED / CONTINUE
            // --------------------------------------------------------

            if (!CreateSeedInput(
                optionsController,
                display.transform,
                SeedY))
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

            if (continueRect != null)
            {
                continueRect.sizeDelta =
                    new Vector2(
                        400f,
                        84f
                    );
            }

            CenterRect(
                continueRect,
                new Vector2(
                    0f,
                    ContinueY
                )
            );


            // Keep the game's normal Back button appearance and position.
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


            RandomizerMenuController screen =
                root.AddComponent
                    <RandomizerMenuController>();

            root.SetActive(
                true
            );

            return screen;
        }


        private static bool CreateToggleButton(
            OptionsControllerPanel optionsController,
            Transform parent,
            string objectName,
            string label,
            float xPosition,
            float yPosition,
            float width,
            float height,
            bool useTitleStyle = false)
        {
            Transform template =
                GetRegularButtonTemplate(
                    optionsController
                );

            if (template == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | Regular button template not found."
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
                objectName;

            DisableLocalization(
                buttonObject
            );

            UIButton button =
                buttonObject
                    .GetComponent<UIButton>();

            if (button == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    objectName +
                    " has no UIButton."
                );

                Object.Destroy(
                    buttonObject
                );

                return false;
            }

            button
                .OnSelectionConfirmed
                .RemoveAll();

            button
                .OnReselectionConfirmed
                .RemoveAll();

            // The ApplyChanges template sizes itself from its content.
            // That fights our fixed column widths, so remove those layout
            // controllers entirely for randomizer toggle rows.
            ContentSizeFitter contentSizeFitter =
                buttonObject.GetComponent<ContentSizeFitter>();

            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
                Object.Destroy(contentSizeFitter);
            }

            HorizontalLayoutGroup horizontalLayout =
                buttonObject.GetComponent<HorizontalLayoutGroup>();

            if (horizontalLayout != null)
            {
                horizontalLayout.enabled = false;
                Object.Destroy(horizontalLayout);
            }

            TextMeshProUGUI text =
                buttonObject
                    .GetComponentInChildren
                        <TextMeshProUGUI>(
                            true
                        );

            if (text == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER FLOW | " +
                    objectName +
                    " has no TextMeshProUGUI."
                );

                Object.Destroy(
                    buttonObject
                );

                return false;
            }

            // The ApplyChanges template keeps its text inside its own
            // nested layout hierarchy. Reparent the text directly under
            // our button so its RectTransform is measured against the
            // full toggle width. This makes every ON/OFF endpoint line up.
            text.transform.SetParent(
                buttonObject.transform,
                false
            );

            text.name =
                "ToggleText";

            text.text =
                label + ": OFF";

            if (useTitleStyle)
            {
                ApplyTitleTextStyle(
                    optionsController,
                    text
                );

                text.alignment =
                    TextAlignmentOptions.MidlineRight;
            }
            else
            {
                text.enableAutoSizing =
                    false;

                text.alignment =
                    TextAlignmentOptions.MidlineRight;
            }

            text.enableWordWrapping =
                false;

            text.overflowMode =
                TextOverflowModes.Overflow;

            RectTransform textRect =
                text.GetComponent<RectTransform>();

            if (textRect != null)
            {
                textRect.anchorMin =
                    Vector2.zero;

                textRect.anchorMax =
                    Vector2.one;

                textRect.offsetMin =
                    new Vector2(
                        8f,
                        0f
                    );

                textRect.offsetMax =
                    new Vector2(
                        -8f,
                        0f
                    );

                textRect.anchoredPosition =
                    Vector2.zero;
            }

            RectTransform rect =
                buttonObject
                    .GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.sizeDelta =
                    new Vector2(
                        width,
                        height
                    );
            }

            CenterRect(
                rect,
                new Vector2(
                    xPosition,
                    yPosition
                )
            );

            // Give every toggle its own hand anchor. The controller moves
            // this anchor to sit just left of the first rendered character
            // whenever the ON/OFF text changes.
            GameObject handAnchorObject =
                new GameObject(
                    "ToggleHandAnchor",
                    typeof(RectTransform)
                );

            handAnchorObject.transform.SetParent(
                buttonObject.transform,
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
                    -width * 0.5f - ToggleHandPadding,
                    ToggleHandYOffset
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


        private static void ApplyTitleTextStyle(
            OptionsControllerPanel optionsController,
            TextMeshProUGUI destination)
        {
            if (optionsController == null ||
                destination == null)
            {
                return;
            }

            Transform titleTemplate =
                optionsController
                    .anchoringGrp
                    ?.Find("OptionsTitle");

            TextMeshProUGUI source =
                titleTemplate
                    ?.GetComponent<TextMeshProUGUI>();

            if (source == null)
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


        private static Transform GetRegularButtonTemplate(
            OptionsControllerPanel optionsController)
        {
            Transform optionsRoot =
                optionsController
                    .anchoringGrp
                    ?.Find("OptionsPanelRoot");

            if (optionsRoot == null)
            {
                return null;
            }

            Transform buttonsLayout =
                optionsRoot.Find(
                    "ButtonsLayoutGrp"
                );

            if (buttonsLayout == null)
            {
                return null;
            }

            return buttonsLayout.Find(
                "ApplyChanges"
            );
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
                    -75f,
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
                    -7f
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
                    -7f
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
            Transform template =
                GetRegularButtonTemplate(
                    optionsController
                );

            if (template == null)
            {
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
                    248f,
                    0f
                )
            );

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
                    175f,
                    -40f
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


        private static bool CreateContinueButton(
            OptionsControllerPanel optionsController,
            Transform parent)
        {
            Transform template =
                GetRegularButtonTemplate(
                    optionsController
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
