// MagicMirrorSpoilerPatch.cs — independent overlay Canvas / exact 25%-5%-7.5% layout
// Updated: 2026-10-20 | Version: acknowledgement-text-fade
// Version: 20261020_095250_UTC
// Includes hidden prompt graphics; native confirmation remains active.
// Replace the old MagicMirrorSpoilerPatch.cs; do not compile both versions.

using System;
using System.Text;
using DG.Tweening;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Randomizer.CatQuest3
{
    // Replaces the cosmetic Magic Mirror interaction only while spells are randomized.
    [HarmonyPatch(typeof(ShopInteractableBehaviour), "OnInteract")]
    public static class MagicMirrorSpoilerPatch
    {
        private static readonly string[] SpellKeys =
        {
            "FLAMEPURR", "FIREPURRISM", "FREEZEPAW", "FURRY_SHIELD", "HEALPAW",
            "ICEPAW", "LIGHTNYAN", "LIGHTWHISKER", "MEWTEORS", "PAWER_UP"
        };

        private static readonly string[] SpellDisplayNames =
        {
            "Flamepurr", "Firepurrism", "Freezepaw", "Furry Shield", "Healpaw",
            "Icepaw", "Lightnyan", "Lightwhisker", "Mewteors", "Pawer Up"
        };

        private static GameObject overlayCanvasRoot;
        private static CanvasGroup overlayCanvasGroup;
        private static GameObject firstColumn;
        private static GameObject secondColumn;
        private static GameObject thirdColumn;
        private static FullScreenDialogueFocusPanel activePanel;
        private static bool showing;
        private static bool originalContentEnabled;
        private static Graphic[] promptGraphics;
        private static bool[] promptGraphicEnabled;

        private static bool IsEnabled()
        {
            return RandomizerRuntime.IsEnabled &&
                   RandomizerState.Settings != null &&
                   RandomizerState.Settings.RandomizeSpells;
        }

        [HarmonyPrefix]
        private static bool Prefix(ShopInteractableBehaviour __instance, GameEntity e)
        {
            if (!IsEnabled() || __instance == null ||
                __instance.upgradePanelType != UpgradePanel.UpgradePanelType.MagicMirror)
                return true;

            if (Contexts.sharedInstance.gUI.isUIInputBlocked ||
                showing || e == null || !e.isPlayerCharacter || !e.hasPlayerId)
                return false;

            FullScreenDialogueFocusPanel panel =
                Contexts.sharedInstance.gUI.fullScreenDialogueFocusPanel.value;

            if (panel == null || panel.content == null)
            {
                Plugin.Log.LogWarning(
                    "MIRROR SPOILER | Native dialogue panel unavailable; using vanilla mirror.");
                return true;
            }

            try
            {
                Show(panel);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("MIRROR SPOILER | Open failed: " + ex);
                Cleanup();
            }

            // Do not open the cosmetic mirror shop or run its camera/FSM sequence.
            return false;
        }

        // The game's native dialogue begins its fade when the player confirms.
        // Our text is on an independent Canvas, so fade it at the same moment
        // rather than keeping it fully opaque until the completion callback.
        [HarmonyPatch(typeof(FullScreenDialogueFocusPanel), "AdvanceByInput")]
        private static class FadeSpoilerOnAcknowledgement
        {
            [HarmonyPrefix]
            private static void Prefix(FullScreenDialogueFocusPanel __instance)
            {
                if (!showing || activePanel != __instance || overlayCanvasGroup == null)
                    return;

                overlayCanvasGroup.DOKill();
                DOTween.To(
    () => overlayCanvasGroup.alpha,
    value => overlayCanvasGroup.alpha = value,
    0f,
    0.12f
);
            }
        }

        private static void Show(FullScreenDialogueFocusPanel panel)
        {
            showing = true;
            activePanel = panel;

            FullScreenDialogueFocusUIInfo info = new FullScreenDialogueFocusUIInfo
            {
                contentTable = new[]
                {
                    new BasicLine { translateID = string.Empty, content = " " }
                }
            };

            Contexts.sharedInstance.input.CreateInputStateCommand(
                InputStateCommand.Push, InputState.UI);

            try
            {
                // The panel owns confirm/cancel handling and its normal fade out.
                panel.Show(info, () =>
                {
                    try
                    {
                        Cleanup();
                    }
                    finally
                    {
                        Contexts.sharedInstance.input.CreateInputStateCommand(
                            InputStateCommand.Pop);
                    }
                });

                CreateColumns(panel);
                HidePromptGraphics(panel);
                Plugin.Log.LogInfo("MIRROR SPOILER | Opened 20-lockbox spoiler (3/4/3).");
            }
            catch
            {
                Contexts.sharedInstance.input.CreateInputStateCommand(
                    InputStateCommand.Pop);
                throw;
            }
        }

        private static void CreateColumns(FullScreenDialogueFocusPanel panel)
        {
            TextMeshProUGUI template = panel.content;
            if (template == null)
                throw new InvalidOperationException("Full-screen dialogue text is unavailable.");

            // Use an independent overlay Canvas. The game's dialogue-panel
            // layout was changing our column RectTransforms after creation,
            // reducing their widths and placing them edge-to-edge.
            // This overlay is VISUAL ONLY: the native panel still owns input.
            overlayCanvasRoot = new GameObject(
                "RandomizerMirrorSpoilerOverlay",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(CanvasGroup));

            overlayCanvasGroup = overlayCanvasRoot.GetComponent<CanvasGroup>();
            overlayCanvasGroup.alpha = 1f;
            overlayCanvasGroup.interactable = false;
            overlayCanvasGroup.blocksRaycasts = false;

            Canvas overlay = overlayCanvasRoot.GetComponent<Canvas>();
            overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.overrideSorting = true;
            overlay.sortingOrder = 32000;

            CanvasScaler scaler = overlayCanvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform overlayRect = overlayCanvasRoot.GetComponent<RectTransform>();
            overlayRect.localScale = Vector3.one;
            overlayRect.localRotation = Quaternion.identity;

            // 25% width per column, 5% gaps, visually balanced right-shift.
            firstColumn = CreateColumn(template, overlayRect,
                "RandomizerMirrorSpoilerColumn1", 0.11625f, 0.36625f,
                BuildColumn(0, 3));

            secondColumn = CreateColumn(template, overlayRect,
                "RandomizerMirrorSpoilerColumn2", 0.41625f, 0.66625f,
                BuildColumn(3, 4));

            thirdColumn = CreateColumn(template, overlayRect,
                "RandomizerMirrorSpoilerColumn3", 0.71625f, 0.96625f,
                BuildColumn(7, 3));

            originalContentEnabled = template.enabled;
            template.enabled = false;

            Plugin.Log.LogInfo("MIRROR SPOILER | Independent overlay " +
                Screen.width + "x" + Screen.height +
                " | 25% columns / 5% gaps / 7.5% margins");
        }

        private static GameObject CreateColumn(TextMeshProUGUI template,
            Transform parent, string name, float minX, float maxX, string text)
        {
            GameObject clone = UnityEngine.Object.Instantiate(
                template.gameObject, parent, false);
            clone.name = name;
            clone.SetActive(true);

            TextMeshProUGUI tmp = clone.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
                throw new InvalidOperationException("Dialogue text clone is missing TMP.");

            tmp.enabled = true;
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 14f;
            tmp.fontSizeMax = 31f;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            RectTransform rect = clone.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(minX, 0.0425f);
            rect.anchorMax = new Vector2(maxX, 0.8725f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            return clone;
        }

        private static string BuildColumn(int startIndex, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if (i > startIndex)
                    sb.Append("\n\n");

                sb.Append("<b>").Append(SpellDisplayNames[i]).Append(" Lockbox</b>\n");
                sb.Append("#1 - ").Append(GetRewardName(SpellKeys[i], 2)).Append('\n');
                sb.Append("#2 - ").Append(GetRewardName(SpellKeys[i], 3));
            }
            return sb.ToString();
        }

        private static string GetRewardName(string spellKey, int tier)
        {
            string key = "RANDOMIZER|MAGIC_SHOP|" + spellKey + "|" + tier;
            RewardLocation location = RewardCatalog.Get(key);
            if (location == null || location.RandomizedRewards == null ||
                location.RandomizedRewards.Count == 0 ||
                location.RandomizedRewards[0] == null)
            {
                Plugin.Log.LogWarning("MIRROR SPOILER | Missing reward: " + key);
                return "Unknown";
            }
            return SpoilerLogWriter.FormatReward(location.RandomizedRewards[0]);
        }

        private static void HidePromptGraphics(FullScreenDialogueFocusPanel panel)
        {
            // Disable only the arrow's rendered Graphics, NOT the PageArrow
            // component/GameObject. Its Show() callback still enables input.
            if (panel.pageArrow == null)
                return;

            promptGraphics = panel.pageArrow.GetComponentsInChildren<Graphic>(true);
            promptGraphicEnabled = new bool[promptGraphics.Length];
            for (int i = 0; i < promptGraphics.Length; i++)
            {
                if (promptGraphics[i] == null)
                    continue;
                promptGraphicEnabled[i] = promptGraphics[i].enabled;
                promptGraphics[i].enabled = false;
            }
        }

        private static void Cleanup()
        {
            // Destroying the overlay destroys all three text columns.
            if (overlayCanvasGroup != null) overlayCanvasGroup.DOKill();
            overlayCanvasGroup = null;
            if (overlayCanvasRoot != null) UnityEngine.Object.Destroy(overlayCanvasRoot);
            overlayCanvasRoot = null;
            firstColumn = null;
            secondColumn = null;
            thirdColumn = null;

            if (promptGraphics != null && promptGraphicEnabled != null)
            {
                for (int i = 0; i < promptGraphics.Length; i++)
                {
                    if (promptGraphics[i] != null)
                        promptGraphics[i].enabled = promptGraphicEnabled[i];
                }
            }
            promptGraphics = null;
            promptGraphicEnabled = null;

            if (activePanel != null && activePanel.content != null)
                activePanel.content.enabled = originalContentEnabled;

            activePanel = null;
            showing = false;
            Plugin.Log.LogInfo("MIRROR SPOILER | Closed.");
        }
    }
}
