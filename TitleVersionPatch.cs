using HarmonyLib;
using TMPro;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(TMP_Text),
        "set_text"
    )]
    public static class TitleVersionPatch
    {
        private const string
            GameVersionObjectName =
                "PatchVersion";

        private const string
            RandomizerVersionObjectName =
                "RandomizerVersion";

        private const float
            FontScale =
                0.50f;

        private const float
            VerticalGap =
                6f;

        private const float
            TextBoxWidth =
                700f;

        private const float
            TextBoxHeight =
                60f;


        public static void Postfix(
            TMP_Text __instance)
        {
            if (!IsGameVersionText(
                    __instance))
            {
                return;
            }


            EnsureRandomizerVersion(
                __instance
            );
        }


        private static bool IsGameVersionText(
            TMP_Text textComponent)
        {
            if (textComponent == null ||
                textComponent.gameObject == null)
            {
                return false;
            }


            if (textComponent.gameObject.name !=
                GameVersionObjectName)
            {
                return false;
            }


            Transform parent =
                textComponent.transform.parent;


            if (parent == null ||
                parent.name != "Backdrop")
            {
                return false;
            }


            Transform grandParent =
                parent.parent;


            if (grandParent == null ||
                grandParent.name != "PatchCanvas")
            {
                return false;
            }


            return true;
        }


        private static void
            EnsureRandomizerVersion(
                TMP_Text gameVersion)
        {
            Transform parent =
                gameVersion.transform.parent;


            if (parent == null)
            {
                return;
            }


            Transform existing =
                parent.Find(
                    RandomizerVersionObjectName
                );


            TextMeshProUGUI randomizerVersion;


            if (existing != null)
            {
                randomizerVersion =
                    existing.GetComponent<
                        TextMeshProUGUI
                    >();


                if (randomizerVersion == null)
                {
                    return;
                }
            }
            else
            {
                GameObject versionObject =
                    new GameObject(
                        RandomizerVersionObjectName,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(TextMeshProUGUI)
                    );


                versionObject.layer =
                    gameVersion.gameObject.layer;


                versionObject.transform.SetParent(
                    parent,
                    false
                );


                randomizerVersion =
                    versionObject.GetComponent<
                        TextMeshProUGUI
                    >();


                CopyAppearance(
                    gameVersion,
                    randomizerVersion
                );


                PositionVersion(
                    gameVersion,
                    randomizerVersion
                );
            }


            randomizerVersion.text =
                "Randomizer Mod v" +
                Plugin.PluginVersion;
        }


        private static void CopyAppearance(
            TMP_Text source,
            TextMeshProUGUI destination)
        {
            destination.font =
                source.font;


            destination.fontSharedMaterial =
                source.fontSharedMaterial;


            destination.color =
                source.color;


            destination.fontStyle =
                source.fontStyle;


            destination.fontSize =
                source.fontSize *
                FontScale;


            destination.enableAutoSizing =
                false;


            destination.enableWordWrapping =
                false;


            destination.overflowMode =
                TextOverflowModes.Overflow;


            destination.alignment =
                TextAlignmentOptions.TopLeft;


            destination.raycastTarget =
                false;
        }


        private static void PositionVersion(
            TMP_Text gameVersion,
            TextMeshProUGUI randomizerVersion)
        {
            RectTransform sourceRect =
                gameVersion.rectTransform;


            RectTransform destinationRect =
                randomizerVersion.rectTransform;


            destinationRect.anchorMin =
                sourceRect.anchorMin;


            destinationRect.anchorMax =
                sourceRect.anchorMax;


            destinationRect.pivot =
                new Vector2(
                    0f,
                    1f
                );


            destinationRect.localScale =
                sourceRect.localScale;


            destinationRect.sizeDelta =
                new Vector2(
                    TextBoxWidth,
                    TextBoxHeight
                );


            float sourceLeft =
                sourceRect.anchoredPosition.x -
                sourceRect.sizeDelta.x *
                sourceRect.pivot.x;


            float sourceBottom =
                sourceRect.anchoredPosition.y -
                sourceRect.sizeDelta.y *
                sourceRect.pivot.y;


            destinationRect.anchoredPosition =
                new Vector2(
                    sourceLeft,
                    sourceBottom -
                    VerticalGap
                );
        }
    }
}