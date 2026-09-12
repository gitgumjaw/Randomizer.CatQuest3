using System;
using Entitas;
using Gentlebros;
using ProjectStar.Data;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class RewardGranter
    {
        private static Sprite manaCrystalSprite;

        public static void Grant(
            Reward reward,
            int vanillaAwardLevel,
            Vector3 position,
            Action callback = null)
        {
            if (reward == null)
            {
                Plugin.Log.LogError(
                    "RewardGranter received a null reward."
                );

                callback?.Invoke();
                return;
            }

            switch (reward.Type)
            {
                case RewardType.Equipment:
                    GrantEquipment(
                        reward,
                        vanillaAwardLevel,
                        callback
                    );
                    return;

                case RewardType.Blueprint:
                    GrantBlueprint(
                        reward,
                        callback
                    );
                    return;

                case RewardType.Spell:
                    GrantSpell(
                        reward,
                        callback
                    );
                    return;

                case RewardType.ShipSpell:
                    GrantShipSpell(
                        reward,
                        callback
                    );
                    return;

                case RewardType.QuestItem:
                    GrantQuestItem(
                        reward,
                        callback
                    );
                    return;

                case RewardType.ManaCrystal:
                    GrantManaCrystal(
                        callback
                    );
                    return;

                case RewardType.Collectible:
                    CollectibleGranter.Spawn(
                        reward,
                        position,
                        1f
                    );

                    callback?.Invoke();
                    return;

                default:
                    Plugin.Log.LogError(
                        $"RewardGranter does not yet support " +
                        $"{reward.Type}."
                    );

                    callback?.Invoke();
                    return;
            }
        }

        private static void GrantEquipment(
    Reward reward,
    int vanillaAwardLevel,
    Action callback)
        {
            EquipmentItemData equipment =
                RewardDataResolver.GetEquipment(
                    reward.Id
                );

            if (equipment == null)
            {
                callback?.Invoke();
                return;
            }

            int awardLevel =
                vanillaAwardLevel;

            bool usePlayerLevel =
                vanillaAwardLevel < 0 ||
                (RandomizerState.Settings != null &&
                 RandomizerState.Settings
                     .MatchEquipmentLevelToPlayer);

            if (usePlayerLevel)
            {
                awardLevel =
                    Contexts.sharedInstance
                        .gameState
                        .level
                        .value;
            }

            Contexts.sharedInstance.input
                .CreateInputStateCommand(
                    InputStateCommand.Push,
                    InputState.UI
                );

            Contexts.sharedInstance.game
                .CreateAddEquipmentCommand(
                    equipment,
                    awardLevel,
                    out int prevLevel,
                    out int newLevel
                );

            Contexts.sharedInstance.gUI
                .ShowEquipmentAwardedUIPanel(
                    equipment,
                    prevLevel,
                    newLevel,
                    delegate
                    {
                        Contexts.sharedInstance.input
                            .CreateInputStateCommand(
                                InputStateCommand.Pop,
                                InputState.UI
                            );

                        callback?.Invoke();
                    }
                );
        }

        private static void GrantBlueprint(
            Reward reward,
            Action callback)
        {
            ShipBlueprintItemData blueprint =
                RewardDataResolver.GetBlueprint(
                    reward.Id
                );

            if (blueprint == null)
            {
                callback?.Invoke();
                return;
            }

            Contexts.sharedInstance.game
                .CreateAddShipBlueprintCommand(
                    blueprint,
                    out var _
                );

            Contexts.sharedInstance.gUI
                .ShowShipBlueprintAwardedUIPanel(
                    blueprint,
                    delegate
                    {
                        callback?.Invoke();
                    }
                );
        }

        private static void GrantSpell(
            Reward reward,
            Action callback)
        {
            SpellConfig spell =
                RewardDataResolver.GetSpell(
                    reward.Id
                );

            if (spell == null)
            {
                callback?.Invoke();
                return;
            }

            Spell existingSpell;

            if ((Contexts.sharedInstance.game
                    .CreateAddNewSpellCommand(
                        spell,
                        out var _,
                        out var success
                    ) != null) &&
                success)
            {
                Contexts.sharedInstance.gUI
                    .ShowSpellAwardedUIPanel(
                        spell,
                        0,
                        1,
                        delegate
                        {
                            callback?.Invoke();
                        }
                    );
            }
            else if (Contexts.sharedInstance.game
                .unlockedSpellTable
                .value
                .TryGetValue(
                    spell.Guid,
                    out existingSpell
                ))
            {
                int previousLevel =
                    existingSpell.level;

                int newLevel =
                    existingSpell.level;

                Contexts.sharedInstance.gUI
                    .ShowSpellAwardedUIPanel(
                        spell,
                        previousLevel,
                        newLevel,
                        delegate
                        {
                            callback?.Invoke();
                        }
                    );
            }
            else
            {
                callback?.Invoke();
            }
        }

        private static void GrantShipSpell(
            Reward reward,
            Action callback)
        {
            ShipSpellConfig shipSpell =
                RewardDataResolver.GetShipSpell(
                    reward.Id
                );

            if (shipSpell == null)
            {
                callback?.Invoke();
                return;
            }

            Contexts.sharedInstance.game
                .CreateAddNewShipSpellCommand(
                    shipSpell,
                    out var _,
                    out var prevLevel,
                    out var newLevel,
                    out var success
                );

            if (!success &&
                Contexts.sharedInstance.game
                    .unlockedShipSpellTable
                    .value
                    .TryGetValue(
                        shipSpell.Guid,
                        out var existingSpell
                    ))
            {
                prevLevel =
                    existingSpell.level;

                newLevel =
                    existingSpell.level;
            }

            Contexts.sharedInstance.gUI
                .ShowSpellAwardedUIPanel(
                    shipSpell,
                    prevLevel,
                    newLevel,
                    delegate
                    {
                        callback?.Invoke();
                    }
                );
        }

        private static void GrantQuestItem(
            Reward reward,
            Action callback)
        {
            QuestItem questItem =
                RewardDataResolver.GetQuestItem(
                    reward.Id
                );

            if (questItem == null)
            {
                callback?.Invoke();
                return;
            }

            GameplayHelper.AwardQuestItem(
                questItem,
                delegate
                {
                    callback?.Invoke();
                }
            );
        }

        private static void GrantManaCrystal(
            Action callback)
        {
            MessageUIInfo messageInfo =
                new MessageUIInfo
                {
                    displaySprite =
                        GetManaCrystalSprite(),

                    contentMessage =
                        new Line
                        {
                            content =
                                "<b><color=#5500ad>Mana</color> increased!</b>\n" +
                                "<color=#99715d><i>\"This will let ye cast more spells!\"</i></color>",

                            translateID =
                                "SideQuest/SQ_Aelius_ZeroDimension_01_S14_Message_1481353870",

                            duration = 1f,
                            textVibrationEffect = false
                        },

                    showChoicePrompts = false,
                    showOnlyCancelPrompt = false,
                    allowMouseInput = true,
                    initiatorPlayerID = -1,
                    title = null,
                    displaySpriteSizeFactor = 1f
                };

            Contexts.sharedInstance.input
                .CreateInputStateCommand(
                    InputStateCommand.Push,
                    InputState.UI
                );

            Contexts.sharedInstance.gUI
                .messagePanel
                .value
                .Show(
                    messageInfo,
                    delegate
                    {
                        Contexts.sharedInstance.input
                            .CreateInputStateCommand(
                                InputStateCommand.Pop
                            );

                        IGroup<GameEntity> players =
                            Contexts.sharedInstance.game
                                .GetGroup(
                                    GameMatcher.PlayerCharacter
                                );

                        SingletonMonoBehaviour<SaveGameManager>
                            .Instance
                            .currSaveSlot
                            .savedAcquiredManaCrystals++;

                        GameStateHelper
                            .UpdatePlayersMaxMana(
                                players
                            );

                        callback?.Invoke();
                    }
                );
        }

        private static Sprite GetManaCrystalSprite()
        {
            if (manaCrystalSprite != null)
            {
                return manaCrystalSprite;
            }

            Sprite[] sprites =
                Resources.FindObjectsOfTypeAll<Sprite>();

            foreach (Sprite sprite in sprites)
            {
                if (sprite != null &&
                    sprite.name ==
                    "UI_message_manaincrease")
                {
                    manaCrystalSprite = sprite;
                    break;
                }
            }

            if (manaCrystalSprite == null)
            {
                Plugin.Log.LogWarning(
                    "Could not find Mana Crystal message sprite: " +
                    "UI_message_manaincrease"
                );
            }

            return manaCrystalSprite;
        }
    }
}