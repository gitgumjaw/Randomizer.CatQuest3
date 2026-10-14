using DG.Tweening;
using Gentlebros;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class MagicShopUpgradePatch
    {
        private static readonly Dictionary<
            MagicUpgradeConfirmPanel,
            VirtualSpellState>
            virtualSpellStates =
                new Dictionary<
                    MagicUpgradeConfirmPanel,
                    VirtualSpellState
                >();


        private static readonly Dictionary<string, string>
            shopKeyNames =
                new Dictionary<string, string>
                {
                    {
                        "c0a5ef232225049c0b9b73926d8e1f0e",
                        "FLAMEPURR"
                    },

                    {
                        "149063cc94514124e828ad4ac945ca5f",
                        "FIREPURRISM"
                    },

                    {
                        "07e501cc6e150497c999080885dd0ae2",
                        "FREEZEPAW"
                    },

                    {
                        "6c43417d9de5ddf4c8256d456e866aab",
                        "FURRY_SHIELD"
                    },

                    {
                        "51dda95ff2f6198488ec4e9b935196f7",
                        "HEALPAW"
                    },

                    {
                        "6c5cca708f40ac348b547f292de31d46",
                        "ICEPAW"
                    },

                    {
                        "9fb4828dd524549b0a4677d2453a19ab",
                        "LIGHTNYAN"
                    },

                    {
                        "1fafd3cbec8754e359dbb8ef5e643196",
                        "LIGHTWHISKER"
                    },

                    {
                        "d55a7ad9dc89ae649834882278fc0ef5",
                        "MEWTEORS"
                    },

                    {
                        "7a4e22db244714a6682b58f11e45b26c",
                        "PAWER_UP"
                    }
                };



        // ============================================================
        // GRID SELECTION
        // ============================================================

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(MagicUpgradeGridCell),
            "CanSelectConfirm"
        )]
        public static void CanSelectConfirmPostfix(
            MagicUpgradeGridCell __instance,
            ref bool __result)
        {
            if (!IsActive())
            {
                return;
            }


            if (
                __instance == null ||
                __instance.cachedSpell == null ||
                __instance.cachedSpell.spellConfig == null
            )
            {
                return;
            }


            Spell spell =
                __instance.cachedSpell;


            int nextTier =
                MagicShopProgress.GetNextTier(
                    spell.spellConfig.Guid
                );


            if (nextTier == 0)
            {
                __result = false;

                return;
            }


            int crystalCost =
                GetUpgradeCost(
                    spell,
                    nextTier
                );


            int currentCrystals =
                Contexts.sharedInstance
                    .gameState
                    .crystal
                    .value
                    .currValue;


            __result =
                currentCrystals >= crystalCost;
        }


        // ============================================================
        // HOVER / GRID PREVIEW
        // ============================================================

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UpgradeStatsPanelBase),
            "PreviewGridCellContent"
        )]
        public static bool PreviewGridCellContentPrefix(
            UpgradeStatsPanelBase __instance,
            UIGridCell UIGridCellRef,
            ref bool __result)
        {
            if (
                !IsActive() ||
                !(__instance is MagicUpgradeStatsPanel) ||
                !(UIGridCellRef is MagicUpgradeGridCell
                    magicCell)
            )
            {
                return true;
            }


            Spell spell =
                magicCell.cachedSpell;


            if (
                spell == null ||
                spell.spellConfig == null
            )
            {
                return true;
            }


            ShowLockboxPreview(
                __instance,
                spell
            );


            __result = true;

            return false;
        }


        private static void ShowLockboxPreview(
            UpgradeStatsPanelBase panel,
            Spell spell)
        {
            string spellName =
                spell.GetItemName();


            int nextTier =
                MagicShopProgress.GetNextTier(
                    spell.spellConfig.Guid
                );


            // Keep vanilla spell icon.
            panel.equipmentSprite.sprite =
                spell.GetPreviewItemUIIcon(
                    spell.level.currValue
                );

            panel.equipmentSprite
                .SetNativeSize();


            // Keep the spell name in its vanilla title position.
            panel.equipmentTitle.text =
                spellName;


            HideMagicStatLine(
                panel
            );


            // ========================================================
            // BOTH LOCKBOXES ACQUIRED
            // ========================================================

            if (nextTier == 0)
            {
                if (panel.equipmentLevel != null)
                {
                    panel.equipmentLevel.text =
                        "Lockboxes Complete!";
                }


                panel.passiveDescription.text =
                    "All " +
                    spellName +
                    " lockboxes have been acquired.";


                panel.upgradeCostRoot
                    .SetActive(
                        false
                    );

                panel.secondaryUpgradeCostRoot
                    .SetActive(
                        false
                    );


                ShowContent(
                    panel
                );

                return;
            }


            // ========================================================
            // AVAILABLE LOCKBOX
            // ========================================================

            int lockboxNumber =
                nextTier - 1;


            if (panel.equipmentLevel != null)
            {
                panel.equipmentLevel.text =
                    lockboxNumber == 1
                        ? "Lockbox #1"
                        : "Lockbox #2";
            }


            Reward reward =
                GetShopReward(
                    spell.spellConfig.Guid,
                    nextTier
                );


            string rewardName =
                reward != null
                    ? RewardNameResolver.GetName(
                        reward
                    )
                    : "Unknown";


            // Only the randomized reward appears
            // between the white separator lines.
            panel.passiveDescription.text =
                rewardName;


            int crystalCost =
                GetUpgradeCost(
                    spell,
                    nextTier
                );


            panel.upgradeCostRoot
                .SetActive(
                    false
                );

            panel.secondaryUpgradeCostRoot
                .SetActive(
                    true
                );

            panel.secondaryUpgradeCost.text =
                crystalCost.ToString();


            ShowContent(
                panel
            );
        }


        private static void HideMagicStatLine(
            UpgradeStatsPanelBase panel)
        {
            MagicUpgradeStatsPanel magicPanel =
                panel as MagicUpgradeStatsPanel;


            if (
                magicPanel == null ||
                magicPanel.magicMultiplierStatBonusInfo == null ||
                magicPanel
                    .magicMultiplierStatBonusInfo
                    .displayRoot == null
            )
            {
                return;
            }


            magicPanel
                .magicMultiplierStatBonusInfo
                .displayRoot
                .SetActive(
                    false
                );
        }


        private static void ShowContent(
    UpgradeStatsPanelBase panel)
        {
            if (panel.contentRoot != null)
            {
                panel.contentRoot.DOKill();

                panel.contentRoot.alpha =
                    1f;
            }
        }


        // ============================================================
        // CONFIRMATION PANEL VIRTUALIZATION
        // ============================================================

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(MagicUpgradeConfirmPanel),
            "Show"
        )]
        public static void ShowPrefix(
            MagicUpgradeConfirmPanel __instance)
        {
            if (
                !IsActive() ||
                __instance == null
            )
            {
                return;
            }


            InventoryItemBase previewItem =
                Traverse.Create(
                        __instance
                    )
                    .Field<InventoryItemBase>(
                        "previewItem"
                    )
                    .Value;


            if (
                !(previewItem is Spell spell) ||
                spell.spellConfig == null
            )
            {
                return;
            }


            int nextTier =
                MagicShopProgress.GetNextTier(
                    spell.spellConfig.Guid
                );


            if (nextTier == 0)
            {
                return;
            }


            RestoreVirtualLevel(
                __instance
            );


            int realLevel =
                spell.level.currValue;

            int virtualCurrentLevel =
                nextTier - 1;


            virtualSpellStates[
                __instance
            ] =
                new VirtualSpellState(
                    spell,
                    realLevel
                );


            spell.level.currValue =
                virtualCurrentLevel;


            Plugin.Log.LogInfo(
                "MAGIC SHOP VIRTUAL PREVIEW | " +
                "Spell:" +
                spell.GetItemName() +
                " | RealLevel:" +
                realLevel +
                " | VirtualCurrentLevel:" +
                virtualCurrentLevel +
                " | OfferedTier:" +
                nextTier
            );
        }


        // ============================================================
        // PURCHASE
        // ============================================================

        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(UpgradeConfirmationPanel),
            "OnUpgradeConfirmed"
        )]
        public static bool OnUpgradeConfirmedPrefix(
            UpgradeConfirmationPanel __instance,
            UIControllerPanel ___controllerPanel,
            InventoryItemBase ___previewItem)
        {
            if (!IsActive())
            {
                return true;
            }


            if (
                !(__instance is MagicUpgradeConfirmPanel
                    magicPanel)
            )
            {
                return true;
            }


            if (
                !(___previewItem is Spell spell) ||
                spell.spellConfig == null
            )
            {
                return true;
            }


            string runtimeSpellGuid =
                spell.spellConfig.Guid;


            int nextTier =
                MagicShopProgress.GetNextTier(
                    runtimeSpellGuid
                );


            if (nextTier == 0)
            {
                RestoreVirtualLevel(
                    magicPanel
                );

                return false;
            }


            string locationKey =
                GetLocationKey(
                    runtimeSpellGuid,
                    nextTier
                );


            if (string.IsNullOrEmpty(
                    locationKey))
            {
                Plugin.Log.LogError(
                    "MAGIC SHOP PURCHASE | " +
                    "Could not resolve randomized " +
                    "shop location | RuntimeSpellGuid:" +
                    runtimeSpellGuid +
                    " | Tier:" +
                    nextTier
                );

                RestoreVirtualLevel(
                    magicPanel
                );

                return false;
            }


            RewardLocation location =
                RewardCatalog.Get(
                    locationKey
                );


            if (
                location == null ||
                location.RandomizedRewards == null ||
                location.RandomizedRewards.Count == 0 ||
                location.RandomizedRewards[0] == null
            )
            {
                Plugin.Log.LogError(
                    "MAGIC SHOP PURCHASE | " +
                    "Randomized reward location was missing | " +
                    "Location:" +
                    locationKey
                );

                RestoreVirtualLevel(
                    magicPanel
                );

                return false;
            }


            int crystalCost =
                GetUpgradeCost(
                    spell,
                    nextTier
                );


            int currentCrystals =
                Contexts.sharedInstance
                    .gameState
                    .crystal
                    .value
                    .currValue;


            if (currentCrystals < crystalCost)
            {
                return false;
            }


            Reward randomizedReward =
                location.RandomizedRewards[0];


            Plugin.Log.LogInfo(
                "MAGIC SHOP PURCHASE | " +
                "Location:" +
                location.Label +
                " | Cost:" +
                crystalCost +
                " | Randomized:" +
                randomizedReward.Type +
                ":" +
                randomizedReward.Id
            );


            Contexts.sharedInstance
                .gameState
                .crystal
                .value
                .currValue -=
                    crystalCost;


            RestoreVirtualLevel(
                magicPanel
            );


            Vector3 rewardPosition =
                RewardPositionResolver
                    .PlayerOrFallback(
                        magicPanel.transform.position
                    );


            Contexts.sharedInstance
                .gUI
                .isUIMouseInteractionMode =
                    false;


            magicPanel.Hide(
                true,
                delegate
                {
                    Traverse.Create(
                            magicPanel
                        )
                        .Field(
                            "previewItem"
                        )
                        .SetValue(
                            null
                        );


                    Traverse.Create(
                            magicPanel
                        )
                        .Field(
                            "previewLevel"
                        )
                        .SetValue(
                            0
                        );


                    RewardGrantQueue.Enqueue(
                        location,
                        0,
                        randomizedReward,
                        -1,
                        rewardPosition,
                        delegate
                        {
                            MagicShopProgress
                                .MarkPurchased(
                                    runtimeSpellGuid,
                                    nextTier
                                );


                            Plugin.Log.LogInfo(
                                "MAGIC SHOP PURCHASE | " +
                                "Completed | " +
                                "Location:" +
                                location.Label +
                                " | Tier:" +
                                nextTier
                            );


                            // Do not restore the shop selection until
                            // the randomized reward acknowledgement has
                            // finished. Waiting one additional frame
                            // prevents the confirm input that dismissed
                            // the reward panel from also confirming the
                            // next lockbox underneath it.
                            SingletonMonoBehaviour<
                                SaveGameManager
                            >
                                .Instance
                                .StartCoroutine(
                                    RestoreShopNavigationNextFrame(
                                        ___controllerPanel
                                    )
                                );
                        }
                    );
                }
            );


            return false;
        }


        // ============================================================
        // CONFIRMATION CLEANUP
        // ============================================================

        [HarmonyPostfix]
        [HarmonyPatch(
            typeof(UpgradeConfirmationPanel),
            "Hide"
        )]
        public static void HidePostfix(
            UpgradeConfirmationPanel __instance)
        {
            if (
                __instance is
                    MagicUpgradeConfirmPanel magicPanel
            )
            {
                RestoreVirtualLevel(
                    magicPanel
                );
            }
        }


        private static void RestoreVirtualLevel(
            MagicUpgradeConfirmPanel panel)
        {
            if (panel == null)
            {
                return;
            }


            if (!virtualSpellStates.TryGetValue(
                    panel,
                    out VirtualSpellState state))
            {
                return;
            }


            if (state.Spell != null)
            {
                state.Spell.level.currValue =
                    state.RealLevel;


                Plugin.Log.LogInfo(
                    "MAGIC SHOP VIRTUAL PREVIEW | " +
                    "Restored real spell level | " +
                    "Guid:" +
                    (
                        state.Spell.spellConfig != null
                            ? state.Spell
                                .spellConfig
                                .Guid
                            : "NULL"
                    ) +
                    " | Level:" +
                    state.RealLevel
                );
            }


            virtualSpellStates.Remove(
                panel
            );
        }


        // ============================================================
        // SHOP LOCATION / REWARD HELPERS
        // ============================================================

        private static Reward GetShopReward(
            string runtimeSpellGuid,
            int tier)
        {
            string locationKey =
                GetLocationKey(
                    runtimeSpellGuid,
                    tier
                );


            if (string.IsNullOrEmpty(
                    locationKey))
            {
                return null;
            }


            RewardLocation location =
                RewardCatalog.Get(
                    locationKey
                );


            if (
                location == null ||
                location.RandomizedRewards == null ||
                location.RandomizedRewards.Count == 0
            )
            {
                return null;
            }


            return location.RandomizedRewards[0];
        }


        private static string GetLocationKey(
            string runtimeSpellGuid,
            int tier)
        {
            if (
                string.IsNullOrEmpty(
                    runtimeSpellGuid) ||
                (tier != 2 &&
                 tier != 3)
            )
            {
                return null;
            }


            if (!shopKeyNames.TryGetValue(
                    runtimeSpellGuid,
                    out string shopKeyName))
            {
                return null;
            }


            return
                "RANDOMIZER|MAGIC_SHOP|" +
                shopKeyName +
                "|" +
                tier;
        }


        private static int GetUpgradeCost(
            Spell spell,
            int tier)
        {
            if (
                spell == null ||
                spell.spellConfig == null
            )
            {
                return int.MaxValue;
            }


            int virtualCurrentLevel =
                tier - 1;


            return UpgradeCostFormula
                .GetSpellUpgradeCost(
                    virtualCurrentLevel,
                    spell.spellConfig
                        .baseUpgradeCost,
                    1,
                    spell.spellConfig
                        .incrementBaseUpgradeCostMultiplier
                );
        }


        private static IEnumerator
            RestoreShopNavigationNextFrame(
                UIControllerPanel controllerPanel)
        {
            yield return null;


            BacktrackNavigation(
                controllerPanel
            );
        }


        private static void BacktrackNavigation(
            UIControllerPanel controllerPanel)
        {
            if (controllerPanel == null)
            {
                return;
            }


            foreach (
                GameEntity player
                in Contexts.sharedInstance
                    .game
                    .GetGroup(
                        GameMatcher.PlayerId
                    ))
            {
                controllerPanel
                    .navigationTracker
                    .BacktrackNavigation(
                        player.playerId.value
                    );
            }
        }


        private static bool IsActive()
        {
            return
                RandomizerRuntime.IsEnabled &&
                RandomizerState.Settings != null &&
                RandomizerState.Settings
                    .RandomizeSpells;
        }


        // ============================================================
        // STATE
        // ============================================================

        private sealed class VirtualSpellState
        {
            public Spell Spell
            {
                get;
            }

            public int RealLevel
            {
                get;
            }


            public VirtualSpellState(
                Spell spell,
                int realLevel)
            {
                Spell =
                    spell;

                RealLevel =
                    realLevel;
            }
        }
    }
}