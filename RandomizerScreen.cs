using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public class RandomizerScreen : UIScreen
    {
        private UIButton continueButton;

        private UISlider enableRandomizerSlider;

        private UISlider equipmentSlider;
        private UISlider spellsSlider;
        private UISlider questItemsSlider;
        private UISlider manaCrystalsSlider;
        private UISlider resourcesCollectiblesSlider;

        private UISlider shipKeySlider;
        private UISlider infinityKeySlider;
        private UISlider birdPoopSlider;

        private UISlider matchPlayerLevelSlider;

        private TMP_InputField seedInput;
        private UIButton generateSeedButton;

        private UIScreen nextScreen;

        private bool initialized;

        private bool seedInputWasFocused;
        private bool previousUIInputBlocked;


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
                transform.Find("Display/Continue")
                    ?.GetComponent<UIButton>();


            enableRandomizerSlider =
                transform.Find(
                    "Display/EnableRandomizer"
                )
                ?.GetComponent<UISlider>();


            equipmentSlider =
                transform.Find(
                    "Display/EquipmentAndBlueprints"
                )
                ?.GetComponent<UISlider>();

            spellsSlider =
                transform.Find(
                    "Display/Spells"
                )
                ?.GetComponent<UISlider>();

            questItemsSlider =
                transform.Find(
                    "Display/QuestItems"
                )
                ?.GetComponent<UISlider>();

            manaCrystalsSlider =
                transform.Find(
                    "Display/ManaCrystals"
                )
                ?.GetComponent<UISlider>();

            resourcesCollectiblesSlider =
                transform.Find(
                    "Display/ResourcesCollectibles"
                )
                ?.GetComponent<UISlider>();


            shipKeySlider =
                transform.Find(
                    "Display/ShipKey"
                )
                ?.GetComponent<UISlider>();

            infinityKeySlider =
                transform.Find(
                    "Display/InfinityKey"
                )
                ?.GetComponent<UISlider>();

            birdPoopSlider =
                transform.Find(
                    "Display/BirdPoop"
                )
                ?.GetComponent<UISlider>();


            matchPlayerLevelSlider =
                transform.Find(
                    "Display/MatchPlayerLevel"
                )
                ?.GetComponent<UISlider>();


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


        public void Configure(
            UIScreen next)
        {
            nextScreen =
                next;
        }


        public override void InitializeScreen()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;


            RestoreControlsAfterDimmingExperiment();


            ConfigureBooleanSlider(
                enableRandomizerSlider,
                PendingRandomizerEnabled
            );


            ConfigureBooleanSlider(
                equipmentSlider,
                PendingEquipmentEnabled
            );

            ConfigureBooleanSlider(
                spellsSlider,
                PendingSpellsEnabled
            );

            ConfigureBooleanSlider(
                questItemsSlider,
                PendingQuestItemsEnabled
            );

            ConfigureBooleanSlider(
                manaCrystalsSlider,
                PendingManaCrystalsEnabled
            );

            ConfigureBooleanSlider(
                resourcesCollectiblesSlider,
                PendingResourcesCollectiblesEnabled
            );


            ConfigureBooleanSlider(
                shipKeySlider,
                PendingShipKeyEnabled
            );

            ConfigureBooleanSlider(
                infinityKeySlider,
                PendingInfinityKeyEnabled
            );

            ConfigureBooleanSlider(
                birdPoopSlider,
                PendingBirdPoopEnabled
            );


            ConfigureBooleanSlider(
                matchPlayerLevelSlider,
                PendingMatchPlayerLevelEnabled
            );


            EnsureSeedExists();
            RestoreSeedText();


            defaultButtonSelection =
                enableRandomizerSlider != null
                    ? (UIButton)enableRandomizerSlider
                    : continueButton;


            base.InitializeScreen();


            if (enableRandomizerSlider != null)
            {
                enableRandomizerSlider
                    .OnValueChanged
                    .AddListener(
                        OnEnableRandomizerChanged
                    );
            }


            if (equipmentSlider != null)
            {
                equipmentSlider
                    .OnValueChanged
                    .AddListener(
                        OnEquipmentChanged
                    );
            }

            if (spellsSlider != null)
            {
                spellsSlider
                    .OnValueChanged
                    .AddListener(
                        OnSpellsChanged
                    );
            }

            if (questItemsSlider != null)
            {
                questItemsSlider
                    .OnValueChanged
                    .AddListener(
                        OnQuestItemsChanged
                    );
            }

            if (manaCrystalsSlider != null)
            {
                manaCrystalsSlider
                    .OnValueChanged
                    .AddListener(
                        OnManaCrystalsChanged
                    );
            }

            if (resourcesCollectiblesSlider != null)
            {
                resourcesCollectiblesSlider
                    .OnValueChanged
                    .AddListener(
                        OnResourcesCollectiblesChanged
                    );
            }


            if (shipKeySlider != null)
            {
                shipKeySlider
                    .OnValueChanged
                    .AddListener(
                        OnShipKeyChanged
                    );
            }

            if (infinityKeySlider != null)
            {
                infinityKeySlider
                    .OnValueChanged
                    .AddListener(
                        OnInfinityKeyChanged
                    );
            }

            if (birdPoopSlider != null)
            {
                birdPoopSlider
                    .OnValueChanged
                    .AddListener(
                        OnBirdPoopChanged
                    );
            }


            if (matchPlayerLevelSlider != null)
            {
                matchPlayerLevelSlider
                    .OnValueChanged
                    .AddListener(
                        OnMatchPlayerLevelChanged
                    );
            }


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


            RestoreControlsAfterDimmingExperiment();


            RestoreBooleanSlider(
                enableRandomizerSlider,
                PendingRandomizerEnabled
            );


            RestoreBooleanSlider(
                equipmentSlider,
                PendingEquipmentEnabled
            );

            RestoreBooleanSlider(
                spellsSlider,
                PendingSpellsEnabled
            );

            RestoreBooleanSlider(
                questItemsSlider,
                PendingQuestItemsEnabled
            );

            RestoreBooleanSlider(
                manaCrystalsSlider,
                PendingManaCrystalsEnabled
            );

            RestoreBooleanSlider(
                resourcesCollectiblesSlider,
                PendingResourcesCollectiblesEnabled
            );


            RestoreBooleanSlider(
                shipKeySlider,
                PendingShipKeyEnabled
            );

            RestoreBooleanSlider(
                infinityKeySlider,
                PendingInfinityKeyEnabled
            );

            RestoreBooleanSlider(
                birdPoopSlider,
                PendingBirdPoopEnabled
            );


            RestoreBooleanSlider(
                matchPlayerLevelSlider,
                PendingMatchPlayerLevelEnabled
            );


            EnsureSeedExists();
            RestoreSeedText();


            base.Show(
                activateBtn
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "Showing real RandomizerScreen."
            );
        }


        private void RestoreControlsAfterDimmingExperiment()
        {
            UISlider[] sliders =
            {
                equipmentSlider,
                spellsSlider,
                questItemsSlider,
                manaCrystalsSlider,
                resourcesCollectiblesSlider,
                shipKeySlider,
                infinityKeySlider,
                birdPoopSlider,
                matchPlayerLevelSlider
            };


            for (int i = 0;
                 i < sliders.Length;
                 i++)
            {
                UISlider slider =
                    sliders[i];


                if (slider == null)
                {
                    continue;
                }


                slider.enabled =
                    true;


                CanvasGroup canvasGroup =
                    slider.GetComponent<CanvasGroup>();


                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }


            if (seedInput != null)
            {
                seedInput.interactable =
                    true;
            }


            if (generateSeedButton != null)
            {
                generateSeedButton.enabled =
                    true;
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


        private static void ConfigureBooleanSlider(
            UISlider slider,
            bool value)
        {
            if (slider == null)
            {
                return;
            }


            slider.isTextSlider = true;
            slider.wrapValue = false;

            slider.UpdateTextArray(
                new string[]
                {
                    "Off",
                    "On"
                }
            );

            slider.ValueWithoutNotify =
                value
                    ? 1f
                    : 0f;
        }


        private static void RestoreBooleanSlider(
            UISlider slider,
            bool value)
        {
            if (slider == null)
            {
                return;
            }


            slider.ValueWithoutNotify =
                value
                    ? 1f
                    : 0f;
        }


        private void OnEnableRandomizerChanged(
            float value)
        {
            PendingRandomizerEnabled =
                value >= 0.5f;


            if (!PendingRandomizerEnabled)
            {
                SetAllRandomizerOptionsOff();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Enable Randomizer:" +
                PendingRandomizerEnabled
            );
        }


        private void OnEquipmentChanged(
            float value)
        {
            PendingEquipmentEnabled =
                value >= 0.5f;

            if (PendingEquipmentEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Equipment:" +
                PendingEquipmentEnabled
            );
        }


        private void OnSpellsChanged(
            float value)
        {
            PendingSpellsEnabled =
                value >= 0.5f;

            if (PendingSpellsEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Spells:" +
                PendingSpellsEnabled
            );
        }


        private void OnQuestItemsChanged(
            float value)
        {
            PendingQuestItemsEnabled =
                value >= 0.5f;

            if (PendingQuestItemsEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Quest Items:" +
                PendingQuestItemsEnabled
            );
        }


        private void OnManaCrystalsChanged(
            float value)
        {
            PendingManaCrystalsEnabled =
                value >= 0.5f;

            if (PendingManaCrystalsEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Mana Crystals:" +
                PendingManaCrystalsEnabled
            );
        }


        private void OnResourcesCollectiblesChanged(
            float value)
        {
            PendingResourcesCollectiblesEnabled =
                value >= 0.5f;

            if (PendingResourcesCollectiblesEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Resources / Collectibles:" +
                PendingResourcesCollectiblesEnabled
            );
        }


        private void OnShipKeyChanged(
            float value)
        {
            PendingShipKeyEnabled =
                value >= 0.5f;

            if (PendingShipKeyEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Ship Key:" +
                PendingShipKeyEnabled
            );
        }


        private void OnInfinityKeyChanged(
            float value)
        {
            PendingInfinityKeyEnabled =
                value >= 0.5f;

            if (PendingInfinityKeyEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Infinity Key:" +
                PendingInfinityKeyEnabled
            );
        }


        private void OnBirdPoopChanged(
            float value)
        {
            PendingBirdPoopEnabled =
                value >= 0.5f;

            if (PendingBirdPoopEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Bird Poop:" +
                PendingBirdPoopEnabled
            );
        }


        private void OnMatchPlayerLevelChanged(
            float value)
        {
            PendingMatchPlayerLevelEnabled =
                value >= 0.5f;

            if (PendingMatchPlayerLevelEnabled)
            {
                EnsureRandomizerEnabled();
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Match Player Level:" +
                PendingMatchPlayerLevelEnabled
            );
        }


        private void EnsureRandomizerEnabled()
        {
            if (PendingRandomizerEnabled)
            {
                return;
            }


            PendingRandomizerEnabled =
                true;


            RestoreBooleanSlider(
                enableRandomizerSlider,
                true
            );


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
            PendingShipKeyEnabled = false;
            PendingInfinityKeyEnabled = false;
            PendingBirdPoopEnabled = false;
            PendingMatchPlayerLevelEnabled = false;


            RestoreBooleanSlider(
                equipmentSlider,
                false
            );

            RestoreBooleanSlider(
                spellsSlider,
                false
            );

            RestoreBooleanSlider(
                questItemsSlider,
                false
            );

            RestoreBooleanSlider(
                manaCrystalsSlider,
                false
            );

            RestoreBooleanSlider(
                resourcesCollectiblesSlider,
                false
            );

            RestoreBooleanSlider(
                shipKeySlider,
                false
            );

            RestoreBooleanSlider(
                infinityKeySlider,
                false
            );

            RestoreBooleanSlider(
                birdPoopSlider,
                false
            );

            RestoreBooleanSlider(
                matchPlayerLevelSlider,
                false
            );
        }


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
                    PendingShipKeyEnabled,
                    PendingInfinityKeyEnabled,
                    PendingBirdPoopEnabled,
                    PendingMatchPlayerLevelEnabled,
                    PendingSeedText,
                    PendingSeedValue
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
                " | ShipKey:" + PendingConfiguration.ShipKey +
                " | InfinityKey:" + PendingConfiguration.InfinityKey +
                " | BirdPoop:" + PendingConfiguration.BirdPoop +
                " | MatchPlayerLevel:" + PendingConfiguration.MatchPlayerLevel
            );


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
            Hide();


            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "Randomizer Back."
            );
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


            seedInputWasFocused =
                false;
        }
    }
}