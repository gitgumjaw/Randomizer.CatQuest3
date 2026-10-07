using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using ProjectStar.Data;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(QuestBehaviour),
        "OnFsmStateChange"
    )]
    public static class StartingLoadoutPatch
    {
        private const string
            MeleeLocationKey =
                "RANDOMIZER|STARTING|MELEE";

        private const string
            BodyLocationKey =
                "RANDOMIZER|STARTING|BODY";

        private const string
            RangedLocationKey =
                "RANDOMIZER|STARTING|RANGED";

        private const string
            SpellLocationKey =
                "RANDOMIZER|STARTING|SPELL";

        private const string
            ShipSpellLocationKey =
                "RANDOMIZER|STARTING|SHIP_SPELL";

        private const string
            QueueLocationKey =
                "RANDOMIZER|STARTING|GRANT_QUEUE";


        private const string
            PirateCutlassGuid =
                "f14e4bbc80f3e6f4f8c6992049be567e";

        private const string
            PirateShirtGuid =
                "a9a12b1d652518c4c833750e965ad108";

        private const string
            BlunderpussGuid =
                "be1a97b87da865e4ba66bd2a2e67b3f9";

        private const string
            FlamepurrGuid =
                "f82e802b773c84c768e5bfa2150324f5";

        private const string
            SuperCannonpawsGuid =
                "2e86f70211bd95e449475ce0ccb01dfc";


        private static bool hasRun;

        public static void Reset()
        {
            hasRun = false;
        }


        public static void Postfix(
            QuestBehaviour __instance,
            FsmState fsmState)
        {
            if (hasRun ||
                __instance == null ||
                __instance.gameObject == null ||
                fsmState == null)
            {
                return;
            }


            if (__instance.gameObject.name !=
                    "MainQuest_Intro" ||
                fsmState.Name !=
                    "Quest Complete")
            {
                return;
            }


            if (!RandomizerRuntime.IsEnabled ||
                RandomizerState.Settings == null)
            {
                return;
            }


            hasRun = true;


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                "Intro reached Quest Complete state."
            );


            GameEntity player =
                FindPlayer();


            if (player == null)
            {
                Plugin.Log.LogError(
                    "STARTING LOADOUT | " +
                    "Could not find player entity."
                );

                return;
            }


            CleanupVanillaLoadout(
                player
            );


            QueueStartingLoadout(
                player
            );
        }


        private static GameEntity FindPlayer()
        {
            foreach (
                GameEntity player
                in Contexts.sharedInstance.game
                    .GetGroup(
                        GameMatcher.PlayerId
                    ))
            {
                if (player != null &&
                    player.hasEquippedItems &&
                    player.hasEquippedSpells)
                {
                    return player;
                }
            }


            return null;
        }


        private static void CleanupVanillaLoadout(
            GameEntity player)
        {
            if (RandomizerState.Settings
                .RandomizeEquipment)
            {
                RemoveVanillaEquipment(
                    player,
                    PirateCutlassGuid
                );

                RemoveVanillaEquipment(
                    player,
                    PirateShirtGuid
                );

                RemoveVanillaEquipment(
                    player,
                    BlunderpussGuid
                );


                RemoveVanillaShipSpell();
            }


            if (RandomizerState.Settings
                .RandomizeSpells)
            {
                RemoveVanillaSpell(
                    player
                );
            }
        }


        private static void RemoveVanillaEquipment(
            GameEntity player,
            string guid)
        {
            Equipment equipment =
                Contexts.sharedInstance.game
                    .GetEquipment(
                        guid
                    );


            if (equipment == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    $"Vanilla equipment not found:{guid}"
                );

                return;
            }


            if (player.hasEquippedItems &&
                player.equippedItems.value != null)
            {
                player.equippedItems.value
                    .UnequipItem(
                        player,
                        equipment
                    );
            }


            if (Contexts.sharedInstance.game
                    .hasUnlockedEquipmentList &&
                Contexts.sharedInstance.game
                    .unlockedEquipmentList
                    .value != null)
            {
                Contexts.sharedInstance.game
                    .unlockedEquipmentList
                    .value
                    .Remove(
                        equipment
                    );
            }


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                $"Removed vanilla equipment:{guid}"
            );
        }


        private static void RemoveVanillaSpell(
            GameEntity player)
        {
            if (!player.hasEquippedSpells ||
                player.equippedSpells.value == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    "Player equipped spell container unavailable."
                );

                return;
            }


            Spell spell =
                GetFirstEquippedSpell(
                    player.equippedSpells.value
                ) as Spell;


            if (spell == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    "Vanilla starting spell not found."
                );

                return;
            }


            SpellConfig config =
                GetSpellConfig(
                    spell
                ) as SpellConfig;


            if (config != null)
            {
                RewardDataResolver
                    .RegisterSpellFallback(
                        FlamepurrGuid,
                        config
                    );


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT | " +
                    $"Captured vanilla spell config:{config.Guid}"
                );
            }
            else
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    "Could not capture vanilla spell config."
                );
            }


            player.equippedSpells.value
                .UnequipSpell(
                    spell
                );


            if (config != null &&
                Contexts.sharedInstance.game
                    .hasUnlockedSpellTable &&
                Contexts.sharedInstance.game
                    .unlockedSpellTable
                    .value != null)
            {
                bool removed =
                    Contexts.sharedInstance.game
                        .unlockedSpellTable
                        .value
                        .Remove(
                            config.Guid
                        );


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT | " +
                    "Removed vanilla spell ownership:" +
                    removed +
                    $" | Guid:{config.Guid}"
                );
            }


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                "Removed vanilla starting spell."
            );
        }


        private static void RemoveVanillaShipSpell()
        {
            if (!Contexts.sharedInstance.game
                    .hasEquippedPlayerShipSpells ||
                Contexts.sharedInstance.game
                    .equippedPlayerShipSpells
                    .value == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    "Ship equipped spell container unavailable."
                );

                return;
            }


            ShipSpell spell =
                GetFirstEquippedSpell(
                    Contexts.sharedInstance.game
                        .equippedPlayerShipSpells
                        .value
                ) as ShipSpell;


            if (spell == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    "Vanilla starting ship spell not found."
                );

                return;
            }


            ShipSpellConfig config =
                GetSpellConfig(
                    spell
                ) as ShipSpellConfig;


            if (config != null)
            {
                RewardDataResolver
                    .RegisterShipSpellFallback(
                        SuperCannonpawsGuid,
                        config
                    );


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT | " +
                    $"Captured vanilla ship spell config:{config.Guid}"
                );
            }
            else
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    "Could not capture vanilla ship spell config."
                );
            }


            Contexts.sharedInstance.game
                .equippedPlayerShipSpells
                .value
                .UnequipSpell(
                    spell
                );


            if (config != null &&
                Contexts.sharedInstance.game
                    .hasUnlockedShipSpellTable &&
                Contexts.sharedInstance.game
                    .unlockedShipSpellTable
                    .value != null)
            {
                bool removed =
                    Contexts.sharedInstance.game
                        .unlockedShipSpellTable
                        .value
                        .Remove(
                            config.Guid
                        );


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT | " +
                    "Removed vanilla ship spell ownership:" +
                    removed +
                    $" | Guid:{config.Guid}"
                );
            }


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                "Removed vanilla starting ship spell."
            );
        }


        private static SpellBase GetFirstEquippedSpell(
            EquippedSpells equippedSpells)
        {
            FieldInfo tableField =
                typeof(EquippedSpells)
                    .GetField(
                        "equippedSpellInfoTable",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );


            if (!(tableField?.GetValue(
                    equippedSpells
                ) is IEnumerable table))
            {
                return null;
            }


            foreach (
                object slot
                in table)
            {
                if (slot == null)
                {
                    continue;
                }


                FieldInfo spellField =
                    slot.GetType()
                        .GetField(
                            "spell",
                            BindingFlags.Instance |
                            BindingFlags.Public |
                            BindingFlags.NonPublic
                        );


                if (spellField?.GetValue(
                        slot
                    ) is SpellBase spell)
                {
                    return spell;
                }
            }


            return null;
        }


        private static SpellConfigBase GetSpellConfig(
            SpellBase spell)
        {
            if (spell == null)
            {
                return null;
            }


            Type type =
                spell.GetType();


            while (type != null)
            {
                FieldInfo field =
                    type.GetField(
                        "spellConfig",
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic
                    );


                if (field != null)
                {
                    return
                        field.GetValue(
                            spell
                        ) as SpellConfigBase;
                }


                type =
                    type.BaseType;
            }


            return null;
        }


        private static void QueueStartingLoadout(
            GameEntity player)
        {
            List<Reward> rewards =
                new List<Reward>();


            if (RandomizerState.Settings
                .RandomizeEquipment)
            {
                AddStartingReward(
                    rewards,
                    MeleeLocationKey
                );

                AddStartingReward(
                    rewards,
                    BodyLocationKey
                );

                AddStartingReward(
                    rewards,
                    RangedLocationKey
                );
            }


            if (RandomizerState.Settings
                .RandomizeSpells)
            {
                AddStartingReward(
                    rewards,
                    SpellLocationKey
                );
            }


            if (RandomizerState.Settings
                .RandomizeEquipment)
            {
                AddStartingReward(
                    rewards,
                    ShipSpellLocationKey
                );
            }


            if (rewards.Count == 0)
            {
                Plugin.Log.LogInfo(
                    "STARTING LOADOUT | " +
                    "No starting rewards queued."
                );

                return;
            }


            RewardLocation queueLocation =
                new RewardLocation(
                    QueueLocationKey,
                    rewards,
                    CreateCollectibleFlags(
                        rewards.Count
                    ),
                    null,
                    "Starting Loadout"
                );


            Vector3 position =
                RewardPositionResolver.PlayerOrFallback(
                    Vector3.zero
                );


            for (int i = 0;
                 i < rewards.Count;
                 i++)
            {
                Reward reward =
                    rewards[i];


                RewardGrantQueue.Enqueue(
                    queueLocation,
                    i,
                    reward,
                    1,
                    position,
                    delegate
                    {
                        EquipRandomizedReward(
                            player,
                            reward
                        );
                    }
                );
            }
        }


        private static void AddStartingReward(
            List<Reward> rewards,
            string locationKey)
        {
            RewardLocation location =
                RewardCatalog.Get(
                    locationKey
                );


            if (location == null ||
                location.RandomizedRewards.Count == 0 ||
                location.RandomizedRewards[0] == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    $"Randomized reward unavailable:{locationKey}"
                );

                return;
            }


            Reward reward =
                location.RandomizedRewards[0];


            rewards.Add(
                reward
            );


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                $"Queueing {location.Label} | " +
                $"{reward.Type}:{reward.Id}"
            );
        }


        private static void EquipRandomizedReward(
            GameEntity player,
            Reward reward)
        {
            if (reward == null)
            {
                return;
            }


            switch (reward.Type)
            {
                case RewardType.Equipment:
                    EquipEquipment(
                        player,
                        reward.Id
                    );
                    return;


                case RewardType.Spell:
                    EquipSpell(
                        player,
                        reward.Id
                    );
                    return;


                case RewardType.ShipSpell:
                    EquipShipSpell(
                        reward.Id
                    );
                    return;
            }
        }


        private static void EquipEquipment(
            GameEntity player,
            string guid)
        {
            Equipment equipment =
                Contexts.sharedInstance.game
                    .GetEquipment(
                        guid
                    );


            if (equipment == null ||
                !player.hasEquippedItems ||
                player.equippedItems.value == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    $"Could not equip equipment:{guid}"
                );

                return;
            }


            player.equippedItems.value
                .EquipItem(
                    player,
                    equipment,
                    false
                );


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                $"Equipped randomized equipment:{guid}"
            );
        }


        private static void EquipSpell(
            GameEntity player,
            string guid)
        {
            SpellConfig config =
                RewardDataResolver.GetSpell(
                    guid
                );


            if (config == null ||
                !Contexts.sharedInstance.game
                    .hasUnlockedSpellTable ||
                Contexts.sharedInstance.game
                    .unlockedSpellTable
                    .value == null ||
                !Contexts.sharedInstance.game
                    .unlockedSpellTable
                    .value
                    .TryGetValue(
                        config.Guid,
                        out Spell spell
                    ) ||
                !player.hasEquippedSpells ||
                player.equippedSpells.value == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    $"Could not equip spell:{guid}"
                );

                return;
            }


            player.equippedSpells.value
                .EquipSpell(
                    spell
                );


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                $"Equipped randomized spell:{guid}"
            );
        }


        private static void EquipShipSpell(
            string guid)
        {
            ShipSpellConfig config =
                RewardDataResolver.GetShipSpell(
                    guid
                );


            if (config == null ||
                !Contexts.sharedInstance.game
                    .hasUnlockedShipSpellTable ||
                Contexts.sharedInstance.game
                    .unlockedShipSpellTable
                    .value == null ||
                !Contexts.sharedInstance.game
                    .unlockedShipSpellTable
                    .value
                    .TryGetValue(
                        config.Guid,
                        out ShipSpell spell
                    ) ||
                !Contexts.sharedInstance.game
                    .hasEquippedPlayerShipSpells ||
                Contexts.sharedInstance.game
                    .equippedPlayerShipSpells
                    .value == null)
            {
                Plugin.Log.LogWarning(
                    "STARTING LOADOUT | " +
                    $"Could not equip ship spell:{guid}"
                );

                return;
            }


            Contexts.sharedInstance.game
                .equippedPlayerShipSpells
                .value
                .EquipSpell(
                    spell
                );


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                $"Equipped randomized ship spell:{guid}"
            );
        }


        private static IEnumerable<bool>
            CreateCollectibleFlags(
                int count)
        {
            List<bool> flags =
                new List<bool>();


            for (int i = 0;
                 i < count;
                 i++)
            {
                flags.Add(
                    false
                );
            }


            return flags;
        }
    }
}