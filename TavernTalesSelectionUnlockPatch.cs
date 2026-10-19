using HarmonyLib;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace Randomizer.CatQuest3
{
    // Tavern Tales selection uses the randomizer's independent claim state.
    // Does not alter vanilla progress, fight rewards, or save files.
    [HarmonyPatch(typeof(GameplayHelper), "IsTavernTalesBossUnlocked")]
    internal static class TavernTalesBossUnlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref bool __result)
        {
            if (!TavernTalesCompanionAccess.Enabled)
                return true;

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(UITavernTalesDetailsSubPanel), "RefreshContent")]
    internal static class TavernTalesDifficultySelectionPatch
    {
        // Vanilla uses this number to lock higher difficulty buttons.
        // Two unlocks difficulty three without altering actual save records.
        [HarmonyPrefix]
        private static void Prefix(ref int highestExaggerationValue)
        {
            if (TavernTalesCompanionAccess.Enabled)
                highestExaggerationValue = 2;
        }

        [HarmonyPostfix]
        private static void Postfix(
            UITavernTalesDetailsSubPanel __instance,
            TavernTalesEncounterDataAsset encounterData)
        {
            if (!TavernTalesCompanionAccess.Enabled ||
                __instance == null || encounterData == null)
                return;

            var panel = Traverse.Create(__instance);
            int difficulty = panel.Field<int>("difficultyLevel").Value;
            Image completionIcon = panel.Field<Image>("completionIcon").Value;

            if (completionIcon != null)
            {
                bool claimed = TavernTalesProgress.IsClaimed(
                    encounterData.name, difficulty);
                completionIcon.gameObject.SetActive(claimed);
            }

            // Preview the shuffled reward rather than the vanilla bird.
            // Keep vanilla text unchanged if this seed lacks a matching entry.
            string key = TavernTalesProgress.LocationKey(
                encounterData.name, difficulty);
            if (key == null)
                return;

            RewardLocation location = RewardCatalog.Get(key);
            if (location == null || location.RandomizedRewards == null ||
                location.RandomizedRewards.Count != 1 ||
                location.RandomizedRewards[0] == null)
                return;

            // Works with either Unity UI Text or TextMeshPro text controls.
            object rewardNameText = panel.Field("rewardCompanionName").GetValue();
            var textProperty = rewardNameText == null ? null :
                rewardNameText.GetType().GetProperty("text");
            if (textProperty == null || !textProperty.CanWrite)
            {
                Plugin.Log.LogWarning(
                    "TAVERN SPOILER | rewardCompanionName text field unavailable | Key:" + key);
                return;
            }

            string name = RewardNameResolver.GetName(location.RandomizedRewards[0]);

            // Use the existing "Reward" label for the entire line, rather
            // than positioning two independent text controls beside each other.
            Component nameComponent = rewardNameText as Component;
            if (nameComponent != null && nameComponent.transform.parent != null)
            {
                Transform group = nameComponent.transform.parent;
                bool labelUpdated = SetRewardLabel(group, nameComponent, name);
                if (labelUpdated)
                {
                    // Hide the now-redundant, separate reward-name control.
                    // Do not disable its GameObject: that could also hide
                    // other child UI elements owned by the game.
                    Behaviour nameBehaviour = nameComponent as Behaviour;
                    if (nameBehaviour != null)
                        nameBehaviour.enabled = false;
                }

                bool iconHidden = HideRewardIcon(panel, group, nameComponent.transform);
                Plugin.Log.LogInfo("TAVERN SPOILER | UI | IconHidden:" + iconHidden +
                    " | CombinedLabel:" + labelUpdated + " | Group:" + group.name);
            }

            Plugin.Log.LogInfo("TAVERN SPOILER | Key:" + key + " | Reward:" + name);
        }

        private static bool HideRewardIcon(Traverse panel, Transform group, Transform nameTransform)
        {
            // Prefer the panel's named portrait field, if present.
            string[] fields = {
                "rewardCompanionIcon", "rewardCompanionImage",
                "companionRewardIcon", "companionRewardImage"
            };
            foreach (string fieldName in fields)
            {
                try
                {
                    object value = panel.Field(fieldName).GetValue();
                    Image icon = value as Image;
                    if (icon != null && icon.transform != nameTransform)
                    {
                        icon.enabled = false;
                        return true;
                    }
                }
                catch (Exception) { /* Field not present in this game build. */ }
            }

            // Fallback: search the local reward row for a distinct bird Image.
            // Do not hide unnamed images, stars, borders, or other UI art.
            Image[] images = group.GetComponentsInChildren<Image>(true);
            foreach (Image icon in images)
            {
                if (icon == null || icon.transform == nameTransform) continue;
                string id = icon.gameObject.name.ToLowerInvariant();
                if ((id.Contains("companion") || id.Contains("parrot") ||
                     id.Contains("bird")) && !id.Contains("star"))
                {
                    icon.enabled = false;
                    return true;
                }
            }
            return false;
        }

        private static void ExpandRewardLabel(Component label)
        {
            // The original "Reward" field was only wide enough for one word.
            // Expand it to use the available horizontal space without moving
            // its left edge. The star images remain untouched.
            RectTransform rect = label.GetComponent<RectTransform>();
            if (rect != null)
            {
                float width = Mathf.Max(rect.rect.width, 420f);
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }

            // Unity UI Text defaults can clip or wrap when the label is long.
            Text legacyText = label as Text;
            if (legacyText != null)
            {
                return;
            }

            // TextMeshPro: set overflow (rather than truncate/ellipsis) and
            // disable wrapping without taking a compile-time TMP dependency.
            try
            {
                var type = label.GetType();
                var overflow = type.GetProperty("overflowMode");
                if (overflow != null && overflow.CanWrite && overflow.PropertyType.IsEnum)
                {
                    object value = Enum.Parse(overflow.PropertyType, "Overflow");
                    overflow.SetValue(label, value, null);
                }
                var wrapping = type.GetProperty("enableWordWrapping");
                if (wrapping != null && wrapping.CanWrite && wrapping.PropertyType == typeof(bool))
                    wrapping.SetValue(label, false, null);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("TAVERN SPOILER | Label overflow settings unavailable: " + ex.Message);
            }
        }

        private static bool SetRewardLabel(Transform group, Component rewardNameComponent, string rewardName)
        {
            // RefreshContent can run repeatedly. Accept the original Reward
            // label and labels already set by this patch on prior refreshes.
            Component[] components = group.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                if (component == null || component == rewardNameComponent)
                    continue;

                var property = component.GetType().GetProperty("text");
                if (property == null || !property.CanRead || !property.CanWrite ||
                    property.PropertyType != typeof(string))
                    continue;

                try
                {
                    string value = property.GetValue(component, null) as string;
                    if (value != null &&
                        (string.Equals(value.Trim(), "Reward", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(value.Trim(), "Reward:", StringComparison.OrdinalIgnoreCase) ||
                         value.TrimStart().StartsWith("Reward: ", StringComparison.OrdinalIgnoreCase)))
                    {
                        property.SetValue(component, "Reward: " + rewardName, null);
                        ExpandRewardLabel(component);
                        return true;
                    }
                }
                catch (Exception) { /* Ignore unrelated text components. */ }
            }
            return false;
        }
    }
}
