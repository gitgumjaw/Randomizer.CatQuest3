using System.Collections.Generic;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(InfinityTowerSpawnBatchReward),
        "OnEnter"
    )]
    public static class
        InfinityTowerRewardChestPatch
    {
        private static readonly Dictionary<
            int,
            string
        >
            locationKeyByChestInstance =
                new Dictionary<int, string>();


        [HarmonyPrefix]
        public static void Prefix(
            InfinityTowerSpawnBatchReward __instance,
            InfinityTowerSpawnerManager ___spawnerManager,
            out string __state)
        {
            __state =
                null;


            if (!RandomizerRuntime.IsEnabled)
            {
                return;
            }


            if (
                __instance == null ||
                ___spawnerManager == null
            )
            {
                return;
            }


            string baseKey =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );


            int wave =
                ___spawnerManager.currWave;


            __state =
                baseKey +
                "|Wave:" +
                wave;
        }


        [HarmonyPostfix]
        public static void Postfix(
            FsmGameObject ___storeSpawnedChest,
            string __state)
        {
            if (string.IsNullOrEmpty(
                    __state))
            {
                return;
            }


            RewardLocation location =
                RewardCatalog.Get(
                    __state
                );


            if (location == null)
            {
                Plugin.Log.LogWarning(
                    "INFINITY TOWER CHEST | " +
                    "Reason:LocationNotFound | " +
                    "Key:" +
                    __state
                );

                return;
            }


            if (
                ___storeSpawnedChest == null ||
                ___storeSpawnedChest.Value == null
            )
            {
                Plugin.Log.LogWarning(
                    "INFINITY TOWER CHEST | " +
                    "Reason:SpawnedChestNotFound | " +
                    "Key:" +
                    __state
                );

                return;
            }


            GameObject chestObject =
                ___storeSpawnedChest.Value;


            ChestBehaviour chest =
                chestObject.GetComponent<
                    ChestBehaviour
                >();


            if (chest == null)
            {
                chest =
                    chestObject
                        .GetComponentInChildren<
                            ChestBehaviour
                        >(
                            true
                        );
            }


            if (chest == null)
            {
                Plugin.Log.LogWarning(
                    "INFINITY TOWER CHEST | " +
                    "Reason:ChestBehaviourNotFound | " +
                    "Key:" +
                    __state +
                    " | Object:" +
                    chestObject.name
                );

                return;
            }


            int instanceId =
                chest.GetInstanceID();


            locationKeyByChestInstance[
                instanceId
            ] =
                __state;


            Plugin.Log.LogInfo(
                "INFINITY TOWER CHEST | " +
                "Registered | " +
                "Location:" +
                location.Label +
                " | Key:" +
                __state +
                " | Instance:" +
                instanceId
            );
        }


        public static bool TryGetLocation(
            ChestBehaviour chest,
            out RewardLocation location)
        {
            location =
                null;


            if (chest == null)
            {
                return false;
            }


            int instanceId =
                chest.GetInstanceID();


            if (!locationKeyByChestInstance
                .TryGetValue(
                    instanceId,
                    out string key))
            {
                return false;
            }


            location =
                RewardCatalog.Get(
                    key
                );


            return location != null;
        }


        public static bool IsRegistered(
            ChestBehaviour chest)
        {
            if (chest == null)
            {
                return false;
            }


            return
                locationKeyByChestInstance
                    .ContainsKey(
                        chest.GetInstanceID()
                    );
        }


        public static void Forget(
            ChestBehaviour chest)
        {
            if (chest == null)
            {
                return;
            }


            locationKeyByChestInstance
                .Remove(
                    chest.GetInstanceID()
                );
        }
    }
}