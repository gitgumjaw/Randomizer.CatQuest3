using System.Collections.Generic;
using HarmonyLib;
using HutongGames.PlayMaker;
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


        private static bool hasRun;


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
                    "Auto Save")
            {
                return;
            }


            if (!RandomizerRuntime.IsEnabled)
            {
                return;
            }


            hasRun = true;


            Plugin.Log.LogInfo(
                "STARTING LOADOUT | " +
                "Intro reached Auto Save state."
            );


            QueueStartingLoadout();
        }


        private static void QueueStartingLoadout()
        {
            string[] locationKeys =
            {
                MeleeLocationKey,
                BodyLocationKey,
                RangedLocationKey,
                SpellLocationKey,
                ShipSpellLocationKey
            };


            List<Reward> rewards =
                new List<Reward>();


            for (int i = 0;
                 i < locationKeys.Length;
                 i++)
            {
                RewardLocation location =
                    RewardCatalog.Get(
                        locationKeys[i]
                    );


                if (location == null ||
                    location.VanillaRewards.Count == 0 ||
                    location.RandomizedRewards.Count == 0)
                {
                    Plugin.Log.LogWarning(
                        "STARTING LOADOUT | " +
                        $"Location unavailable: {locationKeys[i]}"
                    );

                    continue;
                }


                Reward vanilla =
                    location.VanillaRewards[0];

                Reward randomized =
                    location.RandomizedRewards[0];


                if (vanilla == null ||
                    randomized == null)
                {
                    Plugin.Log.LogWarning(
                        "STARTING LOADOUT | " +
                        $"Reward unavailable: {locationKeys[i]}"
                    );

                    continue;
                }


                // If this slot remained vanilla, the player
                // already owns it from normal game setup.
                if (vanilla.Type == randomized.Type &&
                    vanilla.Id == randomized.Id)
                {
                    Plugin.Log.LogInfo(
                        "STARTING LOADOUT | " +
                        $"Unchanged: {location.Label}"
                    );

                    continue;
                }


                Plugin.Log.LogInfo(
                    "STARTING LOADOUT | " +
                    $"Queueing {location.Label} | " +
                    $"Vanilla:{vanilla.Type}:{vanilla.Id} | " +
                    $"Randomized:{randomized.Type}:{randomized.Id}"
                );


                rewards.Add(
                    randomized
                );
            }


            if (rewards.Count == 0)
            {
                Plugin.Log.LogInfo(
                    "STARTING LOADOUT | " +
                    "No replacement rewards required."
                );

                return;
            }


            // RewardGrantQueue groups rewards by location key.
            // Use one temporary shared location so all five
            // starting rewards are processed sequentially.
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
                RewardGrantQueue.Enqueue(
                    queueLocation,
                    i,
                    rewards[i],
                    1,
                    position,
                    null
                );
            }
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