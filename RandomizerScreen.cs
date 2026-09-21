using UnityEngine;

namespace Randomizer.CatQuest3
{
    public class RandomizerScreen : UIScreen
    {
        private UIButton continueButton;

        private UISlider enableRandomizerSlider;
        private UISlider equipmentSlider;
        private UISlider spellsSlider;

        private UIScreen nextScreen;

        private bool initialized;


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


            base.Show(
                activateBtn
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER FLOW | " +
                "Showing real RandomizerScreen."
            );
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


        private static void OnEnableRandomizerChanged(
            float value)
        {
            PendingRandomizerEnabled =
                value >= 0.5f;


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Enable Randomizer:" +
                PendingRandomizerEnabled
            );
        }


        private static void OnEquipmentChanged(
            float value)
        {
            PendingEquipmentEnabled =
                value >= 0.5f;


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Equipment & Blueprints:" +
                PendingEquipmentEnabled
            );
        }


        private static void OnSpellsChanged(
            float value)
        {
            PendingSpellsEnabled =
                value >= 0.5f;


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTING | " +
                "Spells:" +
                PendingSpellsEnabled
            );
        }


        private void Continue(
            UIButton button,
            int playerIndex)
        {
            button.DeselectConfirm();
            button.Deselect();


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
    }
}