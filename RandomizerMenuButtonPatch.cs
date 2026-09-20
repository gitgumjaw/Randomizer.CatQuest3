using HarmonyLib;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(UITitleMenuScreen),
        "InitializeScreen"
    )]
    public static class RandomizerMenuButtonPatch
    {
        private static GameObject
            randomizerButtonObject;

        private static readonly MethodInfo
            SetupNavigationForButtonsMethod =
                AccessTools.Method(
                    typeof(UIScreen),
                    "SetupNavigationForButtons"
                );

        private static readonly FieldInfo
            CustomNavigationSetupField =
                AccessTools.Field(
                    typeof(UIScreen),
                    "customNavigationSetup"
                );


        public static void Postfix(
            UITitleMenuScreen __instance)
        {
            if (__instance == null ||
                __instance.optionsBtn == null)
            {
                return;
            }

            if (randomizerButtonObject != null)
            {
                return;
            }


            GameObject template =
                __instance.optionsBtn.gameObject;

            Transform parent =
                template.transform.parent;

            if (parent == null)
            {
                return;
            }


            randomizerButtonObject =
                Object.Instantiate(
                    template,
                    parent
                );

            randomizerButtonObject.name =
                "Randomizer";


            int optionsIndex =
                template.transform.GetSiblingIndex();

            randomizerButtonObject
                .transform
                .SetSiblingIndex(
                    optionsIndex + 1
                );


            UIButton randomizerButton =
                randomizerButtonObject
                    .GetComponent<UIButton>();

            if (randomizerButton == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER MENU | " +
                    "Cloned button has no UIButton."
                );

                return;
            }


            TextMeshProUGUI text =
                randomizerButtonObject
                    .GetComponentInChildren<TextMeshProUGUI>(
                        true
                    );

            if (text != null)
            {
                text.text =
                    "Randomizer";
            }


            randomizerButton
                .OnSelectionConfirmed
                .RemoveAll();

            randomizerButton
                .OnSelectionConfirmed
                .AddListener(
                    delegate (
                        UIButton button,
                        int playerIndex)
                    {
                        RandomizerPanel.Open(
                            __instance,
                            button
                        );
                    }
                );


            randomizerButtonObject.SetActive(
                true
            );


            if (SetupNavigationForButtonsMethod != null)
            {
                bool customNavigation =
                    CustomNavigationSetupField != null &&
                    (bool)CustomNavigationSetupField
                        .GetValue(
                            __instance
                        );

                SetupNavigationForButtonsMethod.Invoke(
                    __instance,
                    new object[]
                    {
                        customNavigation
                    }
                );
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER MENU | " +
                "Added title-screen Randomizer button."
            );
        }
    }
}