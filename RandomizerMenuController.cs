using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Randomizer.CatQuest3.RandomizerRunConfiguration;

namespace Randomizer.CatQuest3
{
    public class RandomizerMenuController : UIScreen
    {
        private UIButton continueButton;

        private UIButton enableRandomizerButton;

        private UIButton equipmentButton;
        private UIButton spellsButton;
        private UIButton questItemsButton;
        private UIButton manaCrystalsButton;
        private UIButton resourcesCollectiblesButton;
        private UIButton tavernTalesParrotsButton;

        private UIButton shipKeyButton;
        private UIButton infinityKeyButton;
        private UIButton birdPoopButton;
        private UIButton matchPlayerLevelButton;

        // UI-only placeholder for future enemy randomizer work.
        private UIButton rufferEnemiesButton;

        private TMP_InputField seedInput;
        private UIButton generateSeedButton;

        private UIScreen nextScreen;
        private bool initialized;

        private bool seedInputWasFocused;
        private bool previousUIInputBlocked;

        private UIStartScreen titleStartScreen;
        private GameObject hiddenTitleKeyArt;
        private bool titleKeyArtWasActive;
        private bool titleKeyArtHidden;


        public static bool PendingRandomizerEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingEquipmentEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingSpellsEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingQuestItemsEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingManaCrystalsEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingResourcesCollectiblesEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingTavernTalesParrotsEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingShipKeyEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingInfinityKeyEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingBirdPoopEnabled
        {
            get;
            private set;
        } = false;

        public static bool PendingMatchPlayerLevelEnabled
        {
            get;
            private set;
        } = false;

        public static EnemySpawnMode PendingEnemyMode
        {
            get;
            private set;
        } = EnemySpawnMode.Normal;

        public static bool PendingRufferEnemiesEnabled =>
            PendingEnemyMode == EnemySpawnMode.Ruffer;


        public static string PendingSeedText
        {
            get;
            private set;
        } = "";

        public static int PendingSeedValue
        {
            get
            {
                return HashSeedText(
                    PendingSeedText
                );
            }
        }

        public static RandomizerRunConfiguration PendingConfiguration
        {
            get;
            private set;
        }


        protected override void Awake()
        {
            canvas =
                GetComponent<Canvas>();

            displayObject =
                transform.Find("Display")
                    ?.gameObject;

            backButton =
                transform.Find("BackButton")
                    ?.GetComponent<UIButton>();

            continueButton =
                FindButton("Continue");

            enableRandomizerButton =
                FindButton("EnableRandomizer");

            equipmentButton =
                FindButton("EquipmentAndBlueprints");

            spellsButton =
                FindButton("Spells");

            questItemsButton =
                FindButton("QuestItems");

            manaCrystalsButton =
                FindButton("ManaCrystals");

            resourcesCollectiblesButton =
                FindButton("ResourcesCollectibles");

            tavernTalesParrotsButton =
                FindButton("TavernTalesParrots");

            shipKeyButton =
                FindButton("ShipKey");

            infinityKeyButton =
                FindButton("InfinityKey");

            birdPoopButton =
                FindButton("BirdPoop");

            matchPlayerLevelButton =
                FindButton("MatchPlayerLevel");

            rufferEnemiesButton =
                FindButton("RufferEnemies");

            seedInput =
                transform.Find(
                    "Display/SeedInput/Input"
                )
                ?.GetComponent<TMP_InputField>();

            generateSeedButton =
                transform.Find(
                    "Display/SeedInput/Generate"
                )
                ?.GetComponent<UIButton>();

            shownOnStart = false;
            selectableOnShow = true;
            navigationalSelectable = true;

            base.Awake();
        }


        private UIButton FindButton(
            string objectName)
        {
            return transform.Find(
                    "Display/" + objectName
                )
                ?.GetComponent<UIButton>();
        }


        public void Configure(
            UIScreen next)
        {
            nextScreen = next;
        }


        public override void InitializeScreen()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            RestoreControls();
            RefreshAllToggleDisplays();

            EnsureSeedExists();
            RestoreSeedText();

            defaultButtonSelection =
                enableRandomizerButton != null
                    ? enableRandomizerButton
                    : continueButton;

            base.InitializeScreen();

            ConfigureGridNavigation();

            ConfigureToggleButton(
                enableRandomizerButton,
                ToggleEnableRandomizer
            );

            ConfigureToggleButton(
                equipmentButton,
                ToggleEquipment
            );

            ConfigureToggleButton(
                spellsButton,
                ToggleSpells
            );

            ConfigureToggleButton(
                questItemsButton,
                ToggleQuestItems
            );

            ConfigureToggleButton(
                manaCrystalsButton,
                ToggleManaCrystals
            );

            ConfigureToggleButton(
                resourcesCollectiblesButton,
                ToggleResourcesCollectibles
            );

            ConfigureToggleButton(
                tavernTalesParrotsButton,
                ToggleTavernTalesParrots
            );

            ConfigureToggleButton(
                shipKeyButton,
                ToggleShipKey
            );

            ConfigureToggleButton(
                infinityKeyButton,
                ToggleInfinityKey
            );

            ConfigureToggleButton(
                birdPoopButton,
                ToggleBirdPoop
            );

            ConfigureToggleButton(
                matchPlayerLevelButton,
                ToggleMatchPlayerLevel
            );

            ConfigureToggleButton(
                rufferEnemiesButton,
                ToggleRufferEnemies
            );

            if (seedInput != null)
            {
                seedInput
                    .onValueChanged
                    .AddListener(
                        OnSeedTextChanged
                    );
            }

            if (generateSeedButton != null)
            {
                generateSeedButton
                    .OnSelectionConfirmed
                    .RemoveAll();

                generateSeedButton
                    .OnReselectionConfirmed
                    .RemoveAll();

                generateSeedButton
                    .OnSelectionConfirmed
                    .AddListener(
                        GenerateSeed
                    );

                generateSeedButton
                    .OnReselectionConfirmed
                    .AddListener(
                        GenerateSeedReselected
                    );
            }

            if (continueButton != null)
            {
                continueButton
                    .OnSelectionConfirmed
                    .RemoveAll();

                continueButton
                    .OnReselectionConfirmed
                    .RemoveAll();

                continueButton
                    .OnSelectionConfirmed
                    .AddListener(
                        Continue
                    );
            }

            if (backButton != null)
            {
                backButton
                    .OnSelectionConfirmed
                    .RemoveAll();

                backButton
                    .OnReselectionConfirmed
                    .RemoveAll();

                backButton
                    .OnSelectionConfirmed
                    .AddListener(
                        Back
                    );
            }
        }


        private static void ConfigureToggleButton(
            UIButton button,
            Action<UIButton, int> callback)
        {
            if (button == null)
            {
                return;
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
                    callback
                );

            button
                .OnReselectionConfirmed
                .AddListener(
                    delegate (
                        UIButton selectedButton,
                        int playerIndex,
                        bool isMouseInput)
                    {
                        callback(
                            selectedButton,
                            playerIndex
                        );
                    }
                );
        }


        // ============================================================
        // TOGGLE INPUT
        // ============================================================

        private void ToggleEnableRandomizer(
            UIButton button,
            int playerIndex)
        {
            PendingRandomizerEnabled =
                !PendingRandomizerEnabled;

            if (!PendingRandomizerEnabled)
            {
                SetAllRandomizerOptionsOff();
            }

            RefreshAllToggleDisplays();

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Enable Randomizer:" +
                PendingRandomizerEnabled
            );
        }


        private void ToggleEquipment(
            UIButton button,
            int playerIndex)
        {
            PendingEquipmentEnabled =
                !PendingEquipmentEnabled;

            FinishOptionToggle(
                equipmentButton,
                PendingEquipmentEnabled
            );
        }


        private void ToggleSpells(
            UIButton button,
            int playerIndex)
        {
            PendingSpellsEnabled =
                !PendingSpellsEnabled;

            FinishOptionToggle(
                spellsButton,
                PendingSpellsEnabled
            );
        }


        private void ToggleQuestItems(
            UIButton button,
            int playerIndex)
        {
            PendingQuestItemsEnabled =
                !PendingQuestItemsEnabled;

            FinishOptionToggle(
                questItemsButton,
                PendingQuestItemsEnabled
            );
        }


        private void ToggleManaCrystals(
            UIButton button,
            int playerIndex)
        {
            PendingManaCrystalsEnabled =
                !PendingManaCrystalsEnabled;

            FinishOptionToggle(
                manaCrystalsButton,
                PendingManaCrystalsEnabled
            );
        }


        private void ToggleResourcesCollectibles(
            UIButton button,
            int playerIndex)
        {
            PendingResourcesCollectiblesEnabled =
                !PendingResourcesCollectiblesEnabled;

            FinishOptionToggle(
                resourcesCollectiblesButton,
                PendingResourcesCollectiblesEnabled
            );
        }


        private void ToggleTavernTalesParrots(
            UIButton button,
            int playerIndex)
        {
            PendingTavernTalesParrotsEnabled =
                !PendingTavernTalesParrotsEnabled;

            FinishOptionToggle(
                tavernTalesParrotsButton,
                PendingTavernTalesParrotsEnabled
            );
        }


        private void ToggleShipKey(
            UIButton button,
            int playerIndex)
        {
            PendingShipKeyEnabled =
                !PendingShipKeyEnabled;

            FinishOptionToggle(
                shipKeyButton,
                PendingShipKeyEnabled
            );
        }


        private void ToggleInfinityKey(
            UIButton button,
            int playerIndex)
        {
            PendingInfinityKeyEnabled =
                !PendingInfinityKeyEnabled;

            FinishOptionToggle(
                infinityKeyButton,
                PendingInfinityKeyEnabled
            );
        }


        private void ToggleBirdPoop(
            UIButton button,
            int playerIndex)
        {
            PendingBirdPoopEnabled =
                !PendingBirdPoopEnabled;

            FinishOptionToggle(
                birdPoopButton,
                PendingBirdPoopEnabled
            );
        }


        private void ToggleMatchPlayerLevel(
            UIButton button,
            int playerIndex)
        {
            PendingMatchPlayerLevelEnabled =
                !PendingMatchPlayerLevelEnabled;

            FinishOptionToggle(
                matchPlayerLevelButton,
                PendingMatchPlayerLevelEnabled
            );
        }



        private void ToggleRufferEnemies(
            UIButton button,
            int playerIndex)
        {
            switch (PendingEnemyMode)
            {
                case EnemySpawnMode.Normal:
                    PendingEnemyMode = EnemySpawnMode.Ruffer;
                    break;

                case EnemySpawnMode.Ruffer:
                    PendingEnemyMode = EnemySpawnMode.Random;
                    break;

                default:
                    PendingEnemyMode = EnemySpawnMode.Normal;
                    break;
            }

            if (PendingEnemyMode != EnemySpawnMode.Normal)
            {
                EnsureRandomizerEnabled();
            }

            RefreshAllToggleDisplays();

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | Enemy Mode:" +
                PendingEnemyMode
            );
        }



        private void FinishOptionToggle(
            UIButton button,
            bool enabled)
        {
            if (enabled)
            {
                EnsureRandomizerEnabled();
            }

            RefreshAllToggleDisplays();

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                GetToggleLabel(button) +
                ":" +
                enabled
            );
        }


        private void EnsureRandomizerEnabled()
        {
            if (PendingRandomizerEnabled)
            {
                return;
            }

            PendingRandomizerEnabled = true;

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Enable Randomizer:True (automatic)"
            );
        }


        private void SetAllRandomizerOptionsOff()
        {
            PendingEquipmentEnabled = false;
            PendingSpellsEnabled = false;
            PendingQuestItemsEnabled = false;
            PendingManaCrystalsEnabled = false;
            PendingResourcesCollectiblesEnabled = false;
            PendingTavernTalesParrotsEnabled = false;

            PendingShipKeyEnabled = false;
            PendingInfinityKeyEnabled = false;
            PendingBirdPoopEnabled = false;
            PendingMatchPlayerLevelEnabled = false;

            PendingEnemyMode = EnemySpawnMode.Normal;
        }


        // ============================================================
        // TOGGLE DISPLAY
        // ============================================================

        private void RefreshAllToggleDisplays()
        {
            SetToggleState(
                enableRandomizerButton,
                PendingRandomizerEnabled
            );

            SetToggleState(
                equipmentButton,
                PendingEquipmentEnabled
            );

            SetToggleState(
                spellsButton,
                PendingSpellsEnabled
            );

            SetToggleState(
                questItemsButton,
                PendingQuestItemsEnabled
            );

            SetToggleState(
                manaCrystalsButton,
                PendingManaCrystalsEnabled
            );

            SetToggleState(
                resourcesCollectiblesButton,
                PendingResourcesCollectiblesEnabled
            );

            SetToggleState(
                tavernTalesParrotsButton,
                PendingTavernTalesParrotsEnabled
            );

            SetToggleState(
                shipKeyButton,
                PendingShipKeyEnabled
            );

            SetToggleState(
                infinityKeyButton,
                PendingInfinityKeyEnabled
            );

            SetToggleState(
                birdPoopButton,
                PendingBirdPoopEnabled
            );

            SetToggleState(
                matchPlayerLevelButton,
                PendingMatchPlayerLevelEnabled
            );


            SetToggleState(
                rufferEnemiesButton,
                PendingEnemyMode != EnemySpawnMode.Normal
            );

            if (rufferEnemiesButton != null)
            {
                TextMeshProUGUI enemyModeText =
                    rufferEnemiesButton.GetComponentInChildren
                        <TextMeshProUGUI>(true);

                if (enemyModeText != null)
                {
                    enemyModeText.text =
                        "Enemies: " + PendingEnemyMode;
                }
            }

        }


        private static void SetToggleState(
            UIButton button,
            bool enabled)
        {
            if (button == null)
            {
                return;
            }

            TextMeshProUGUI text =
                button.GetComponentInChildren
                    <TextMeshProUGUI>(
                        true
                    );

            if (text == null)
            {
                return;
            }

            string label =
                GetToggleLabel(
                    button
                );

            string value;

            if (label == "Randomizer")
            {
                value =
                    enabled
                        ? "On"
                        : "Off";
            }
            else
            {
                value =
                    enabled
                        ? "ON"
                        : "OFF";
            }

            text.text =
                label + ": " + value;

            // Keep the title's inherited font styling intact, but give
            // normal option rows a stronger weight when enabled.
            if (label != "Randomizer")
            {
                text.fontStyle =
                    enabled
                        ? FontStyles.Bold
                        : FontStyles.Normal;
            }

            Color stateColor =
                text.color;

            stateColor.a =
                enabled
                    ? 1f
                    : RandomizerMenuBuilder.OffToggleAlpha;

            text.color =
                stateColor;

            UpdateToggleHandPosition(
                button,
                text
            );
        }


        private void UpdateToggleHandPosition(
            UIButton button,
            TextMeshProUGUI text)
        {
            if (button == rufferEnemiesButton)
                return;

            if (button == null ||
                text == null ||
                button.handIconOverridePos == null)
            {
                return;
            }

            RectTransform textRect =
                text.GetComponent<RectTransform>();

            RectTransform handAnchor =
                button.handIconOverridePos
                    as RectTransform;

            if (textRect == null ||
                handAnchor == null)
            {
                return;
            }

            text.ForceMeshUpdate();

            float renderedWidth =
                text.preferredWidth;

            float firstCharacterX;

            if (text.alignment ==
                    TextAlignmentOptions.Midline ||
                text.alignment ==
                    TextAlignmentOptions.Center)
            {
                float textCenterX =
                    textRect.anchoredPosition.x +
                    (
                        textRect.rect.xMin +
                        textRect.rect.xMax
                    ) * 0.5f;

                firstCharacterX =
                    textCenterX -
                    renderedWidth * 0.5f;
            }
            else
            {
                float textRightX =
                    textRect.anchoredPosition.x +
                    textRect.rect.xMax;

                firstCharacterX =
                    textRightX -
                    renderedWidth;
            }

            handAnchor.anchoredPosition =
                new Vector2(
                    firstCharacterX -
                        RandomizerMenuBuilder.ToggleHandPadding,
                    RandomizerMenuBuilder.ToggleHandYOffset
                );
        }


        private static string GetToggleLabel(
            UIButton button)
        {
            if (button == null)
            {
                return "Toggle";
            }

            TextMeshProUGUI text =
                button.GetComponentInChildren
                    <TextMeshProUGUI>(
                        true
                    );

            if (text == null ||
                string.IsNullOrEmpty(text.text))
            {
                return button.name;
            }

            int separatorIndex =
                text.text.LastIndexOf(
                    ':'
                );

            if (separatorIndex <= 0)
            {
                return text.text;
            }

            return text.text
                .Substring(
                    0,
                    separatorIndex
                )
                .TrimEnd();
        }


        // ============================================================
        // NAVIGATION
        // ============================================================

        private void ConfigureGridNavigation()
        {
            SetNavigation(
                enableRandomizerButton,
                null,
                null,
                null,
                equipmentButton
            );

            // Column 1
            SetNavigation(
                equipmentButton,
                null,
                shipKeyButton,
                enableRandomizerButton,
                spellsButton
            );

            SetNavigation(
                spellsButton,
                null,
                infinityKeyButton,
                equipmentButton,
                questItemsButton
            );

            SetNavigation(
                questItemsButton,
                null,
                birdPoopButton,
                spellsButton,
                manaCrystalsButton
            );

            SetNavigation(
                manaCrystalsButton,
                null,
                birdPoopButton,
                questItemsButton,
                resourcesCollectiblesButton
            );

            SetNavigation(
                resourcesCollectiblesButton,
                null,
                birdPoopButton,
                manaCrystalsButton,
                tavernTalesParrotsButton
            );

            SetNavigation(
                tavernTalesParrotsButton,
                null,
                birdPoopButton,
                resourcesCollectiblesButton,
                matchPlayerLevelButton
            );

            SetNavigation(
                matchPlayerLevelButton,
                null,
                birdPoopButton,
                tavernTalesParrotsButton,
                generateSeedButton
            );

            // Column 2
            SetNavigation(
                shipKeyButton,
                equipmentButton,
                rufferEnemiesButton,
                enableRandomizerButton,
                infinityKeyButton
            );

            SetNavigation(
                infinityKeyButton,
                spellsButton,
                rufferEnemiesButton,
                shipKeyButton,
                birdPoopButton
            );

            SetNavigation(
                birdPoopButton,
                questItemsButton,
                rufferEnemiesButton,
                infinityKeyButton,
                generateSeedButton
            );

            // Column 3
            SetNavigation(
                rufferEnemiesButton,
                shipKeyButton,
                null,
                enableRandomizerButton,
                generateSeedButton
            );

            // Bottom controls
            SetNavigation(
                generateSeedButton,
                matchPlayerLevelButton,
                backButton,
                birdPoopButton,
                continueButton
            );

            SetNavigation(
                continueButton,
                null,
                backButton,
                generateSeedButton,
                null
            );

            SetNavigation(
                backButton,
                continueButton,
                null,
                rufferEnemiesButton,
                null
            );
        }


        private static void SetNavigation(
            UIButton button,
            UIButton left,
            UIButton right,
            UIButton up,
            UIButton down)
        {
            if (button == null)
            {
                return;
            }

            Navigation navigation =
                button.navigation;

            navigation.mode =
                Navigation.Mode.Explicit;

            navigation.selectOnLeft =
                left;

            navigation.selectOnRight =
                right;

            navigation.selectOnUp =
                up;

            navigation.selectOnDown =
                down;

            button.navigation =
                navigation;
        }


        // ============================================================
        // SCREEN SHOW / HIDE
        // ============================================================

        public override void Show(
            UIButton activateBtn)
        {
            SaveLoadPanel saveLoadPanel =
                GetSaveLoadPanel();

            if (saveLoadPanel == null ||
                saveLoadPanel.startingGameMode !=
                SaveLoadPanel.StartingGameMode.NEW)
            {
                if (nextScreen != null)
                {
                    nextScreen.Show(
                        activateBtn
                    );
                }

                return;
            }

            RestoreControls();
            RefreshAllToggleDisplays();

            EnsureSeedExists();
            RestoreSeedText();

            HideTitleKeyArt();

            base.Show(
                activateBtn
            );

            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "Showing real RandomizerScreen."
            );
        }


        private void RestoreControls()
        {
            UIButton[] buttons =
            {
                enableRandomizerButton,
                equipmentButton,
                spellsButton,
                questItemsButton,
                manaCrystalsButton,
                resourcesCollectiblesButton,
                tavernTalesParrotsButton,
                shipKeyButton,
                infinityKeyButton,
                birdPoopButton,
                matchPlayerLevelButton,
                rufferEnemiesButton
            };

            for (int i = 0;
                 i < buttons.Length;
                 i++)
            {
                UIButton button =
                    buttons[i];

                if (button == null)
                {
                    continue;
                }

                button.enabled = true;

                CanvasGroup canvasGroup =
                    button.GetComponent<CanvasGroup>();

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }

            if (seedInput != null)
            {
                seedInput.interactable = true;
            }

            if (generateSeedButton != null)
            {
                generateSeedButton.enabled = true;
            }

            Transform seedRow =
                transform.Find(
                    "Display/SeedInput"
                );

            CanvasGroup seedCanvasGroup =
                seedRow
                    ?.GetComponent<CanvasGroup>();

            if (seedCanvasGroup != null)
            {
                seedCanvasGroup.alpha = 1f;
                seedCanvasGroup.interactable = true;
                seedCanvasGroup.blocksRaycasts = true;
            }
        }


        // ============================================================
        // SEED
        // ============================================================

        private static string CreateRandomSeedText()
        {
            return Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpperInvariant();
        }


        private static int HashSeedText(
            string text)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(
                    text ?? ""
                );

            unchecked
            {
                uint hash =
                    2166136261u;

                for (int i = 0;
                     i < bytes.Length;
                     i++)
                {
                    hash ^=
                        bytes[i];

                    hash *=
                        16777619u;
                }

                return (int)(
                    hash &
                    0x7FFFFFFFu
                );
            }
        }


        private static void EnsureSeedExists()
        {
            if (!string.IsNullOrWhiteSpace(
                PendingSeedText))
            {
                return;
            }

            PendingSeedText =
                CreateRandomSeedText();

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Generated Initial Seed:" +
                PendingSeedText
            );
        }


        private void RestoreSeedText()
        {
            if (seedInput == null)
            {
                return;
            }

            seedInput.SetTextWithoutNotify(
                PendingSeedText
            );
        }


        private static void OnSeedTextChanged(
            string value)
        {
            PendingSeedText =
                value ?? "";

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Seed:" +
                PendingSeedText
            );
        }


        private void GenerateSeedReselected(
            UIButton button,
            int playerIndex,
            bool isMouseInput)
        {
            GenerateSeed(
                button,
                playerIndex
            );
        }


        private void GenerateSeed(
            UIButton button,
            int playerIndex)
        {
            PendingSeedText =
                CreateRandomSeedText();

            if (seedInput != null)
            {
                seedInput.SetTextWithoutNotify(
                    PendingSeedText
                );
            }

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Generated Seed:" +
                PendingSeedText +
                " | Hash:" +
                PendingSeedValue
            );
        }


        // ============================================================
        // CONTINUE / BACK
        // ============================================================

        private void Continue(
            UIButton button,
            int playerIndex)
        {
            button.DeselectConfirm();
            button.Deselect();

            EnsureSeedExists();
            RestoreSeedText();


            PendingConfiguration =
                new RandomizerRunConfiguration(
                    PendingRandomizerEnabled,
                    PendingEquipmentEnabled,
                    PendingSpellsEnabled,
                    PendingQuestItemsEnabled,
                    PendingManaCrystalsEnabled,
                    PendingResourcesCollectiblesEnabled,
                    PendingTavernTalesParrotsEnabled,
                    PendingShipKeyEnabled,
                    PendingInfinityKeyEnabled,
                    PendingBirdPoopEnabled,
                    PendingMatchPlayerLevelEnabled,
                    PendingSeedText,
                    PendingSeedValue,
                    PendingRufferEnemiesEnabled
                );

            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Final Seed:" +
                PendingConfiguration.SeedText +
                " | Hash:" +
                PendingConfiguration.SeedValue
            );

            Plugin.Log.LogInfo(
                "RANDOMIZER CONFIG | " +
                "Enabled:" + PendingConfiguration.Enabled +
                " | Equipment:" + PendingConfiguration.Equipment +
                " | Spells:" + PendingConfiguration.Spells +
                " | QuestItems:" + PendingConfiguration.QuestItems +
                " | ManaCrystals:" + PendingConfiguration.ManaCrystals +
                " | ResourcesCollectibles:" + PendingConfiguration.ResourcesCollectibles +
                " | TavernTalesParrots:" + PendingConfiguration.TavernTalesParrots +
                " | ShipKey:" + PendingConfiguration.ShipKey +
                " | InfinityKey:" + PendingConfiguration.InfinityKey +
                " | BirdPoop:" + PendingConfiguration.BirdPoop +
                " | MatchPlayerLevel:" + PendingConfiguration.MatchPlayerLevel +
                " | RufferEnemies:" + PendingConfiguration.RufferEnemies
            );


            RestoreTitleKeyArt();
            Hide();

            if (nextScreen != null)
            {
                nextScreen
                    .onPanelShowCompleted
                    .AddOnce(
                        delegate
                        {
                            UIButton nextDefault =
                                nextScreen
                                    .GetDefaultButtonSelection();

                            if (nextDefault == null)
                            {
                                return;
                            }

                            nextDefault.Select(
                                playerIndex,
                                isMouseInput: false,
                                playAudio: false
                            );

                            UIControllerPanel controller =
                                nextScreen
                                    .GetComponentInParent
                                        <UIControllerPanel>();

                            if (controller == null ||
                                controller.navigationTracker == null)
                            {
                                return;
                            }

                            controller
                                .navigationTracker
                                .SnapPlayerHandIcon(
                                    playerIndex,
                                    nextDefault
                                );

                            controller
                                .navigationTracker
                                .ShowHandIcons();
                        }
                    );

                nextScreen.Show(
                    null
                );
            }

            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "Randomizer -> Difficulty."
            );
        }


        private void Back(
            UIButton button,
            int playerIndex)
        {
            RestoreTitleKeyArt();
            Hide();

            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "Randomizer Back."
            );
        }


        // ============================================================
        // TITLE KEY ART
        // ============================================================

        private void HideTitleKeyArt()
        {
            if (titleKeyArtHidden)
            {
                return;
            }

            titleStartScreen =
                FindTitleStartScreen();

            if (titleStartScreen == null ||
                titleStartScreen.keyArt == null)
            {
                Plugin.Log.LogWarning(
                    "RANDOMIZER UI | " +
                    "Could not find UIStartScreen key art."
                );

                return;
            }

            hiddenTitleKeyArt =
                titleStartScreen
                    .keyArt
                    .gameObject;

            titleKeyArtWasActive =
                hiddenTitleKeyArt.activeSelf;

            hiddenTitleKeyArt.SetActive(
                false
            );

            titleKeyArtHidden = true;

            Plugin.Log.LogInfo(
                "RANDOMIZER UI | " +
                "Title key art hidden."
            );
        }


        private void RestoreTitleKeyArt()
        {
            if (!titleKeyArtHidden)
            {
                return;
            }

            if (hiddenTitleKeyArt != null)
            {
                hiddenTitleKeyArt.SetActive(
                    titleKeyArtWasActive
                );
            }

            hiddenTitleKeyArt = null;
            titleStartScreen = null;
            titleKeyArtHidden = false;

            Plugin.Log.LogInfo(
                "RANDOMIZER UI | " +
                "Title key art restored."
            );
        }


        private static UIStartScreen
            FindTitleStartScreen()
        {
            UIStartScreen[] startScreens =
                Resources
                    .FindObjectsOfTypeAll
                        <UIStartScreen>();

            for (int i = 0;
                 i < startScreens.Length;
                 i++)
            {
                UIStartScreen startScreen =
                    startScreens[i];

                if (startScreen == null ||
                    startScreen.gameObject == null ||
                    !startScreen.gameObject.scene.IsValid())
                {
                    continue;
                }

                if (startScreen.keyArt != null)
                {
                    return startScreen;
                }
            }

            return null;
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


        private void Update()
        {
            bool seedFocused =
                seedInput != null &&
                seedInput.isFocused;

            if (seedFocused ==
                seedInputWasFocused)
            {
                return;
            }

            if (seedFocused)
            {
                previousUIInputBlocked =
                    Contexts.sharedInstance
                        .gUI
                        .isUIInputBlocked;

                Contexts.sharedInstance
                    .gUI
                    .isUIInputBlocked =
                    true;
            }
            else
            {
                Contexts.sharedInstance
                    .gUI
                    .isUIInputBlocked =
                    previousUIInputBlocked;
            }

            seedInputWasFocused =
                seedFocused;
        }


        protected override void OnDisable()
        {
            RestoreTitleKeyArt();

            base.OnDisable();

            if (!seedInputWasFocused)
            {
                return;
            }

            if (Contexts.sharedInstance != null &&
                Contexts.sharedInstance.gUI != null)
            {
                Contexts.sharedInstance
                    .gUI
                    .isUIInputBlocked =
                    previousUIInputBlocked;
            }

            seedInputWasFocused = false;
        }
    }
}
