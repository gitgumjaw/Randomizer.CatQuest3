using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(AwardSpellToPlayer),
        "Award"
    )]
    public static class SpellAwardRandomizerPatch
    {
        public static bool Prefix(
            AwardSpellToPlayer __instance)
        {
            if (__instance == null)
            {
                Plugin.Log.LogWarning(
                    "SPELL RANDOMIZER FALLBACK | " +
                    "Reason:InstanceNull"
                );

                return true;
            }


            if (__instance.Fsm == null)
            {
                Plugin.Log.LogWarning(
                    "SPELL RANDOMIZER FALLBACK | " +
                    "Reason:FsmNull"
                );

                return true;
            }


            string key =
                PlayMakerLocation.GetKey(
                    __instance.Fsm
                );


            if (string.IsNullOrEmpty(key))
            {
                Plugin.Log.LogWarning(
                    "SPELL RANDOMIZER FALLBACK | " +
                    "Reason:LocationKeyEmpty"
                );

                return true;
            }


            RewardLocation location =
                RewardCatalog.Get(
                    key
                );


            if (location == null)
            {
                Plugin.Log.LogWarning(
                    $"SPELL RANDOMIZER FALLBACK | " +
                    $"Reason:LocationNotFound | " +
                    $"Key:{key}"
                );

                return true;
            }


            if (__instance.spellConfig == null)
            {
                Plugin.Log.LogWarning(
                    $"SPELL RANDOMIZER FALLBACK | " +
                    $"Reason:SpellConfigNull | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label}"
                );

                return true;
            }


            string spellGuid =
                __instance.spellConfig.Guid;


            int rewardIndex =
                location.FindVanillaRewardIndex(
                    RewardType.Spell,
                    spellGuid
                );


            if (rewardIndex < 0)
            {
                Plugin.Log.LogWarning(
                    $"SPELL RANDOMIZER FALLBACK | " +
                    $"Reason:VanillaSpellNotFound | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label} | " +
                    $"Spell:{spellGuid}"
                );

                return true;
            }


            Reward randomizedReward =
                location.RandomizedRewards[
                    rewardIndex
                ];


            if (randomizedReward == null)
            {
                Plugin.Log.LogWarning(
                    $"SPELL RANDOMIZER FALLBACK | " +
                    $"Reason:RandomizedRewardNull | " +
                    $"Key:{key} | " +
                    $"Location:{location.Label} | " +
                    $"Slot:{rewardIndex} | " +
                    $"Spell:{spellGuid}"
                );

                return true;
            }


            if (!__instance.dontRaiseCutsceneFlag)
            {
                Contexts.sharedInstance.game
                    .isInCutscene = true;

                Contexts.sharedInstance.game
                    .cutsceneOwner.value =
                        __instance.Fsm.GameObject;
            }


            Vector3 position =
                RewardPositionResolver.PlayerOrFallback(
                    __instance.Fsm.GameObject
                        .transform.position
                );


            Plugin.Log.LogInfo(
                $"Queueing spell reward | " +
                $"Location:{location.Label} | " +
                $"Slot:{rewardIndex} | " +
                $"Vanilla:Spell:{spellGuid} | " +
                $"Randomized:{randomizedReward.Type}:" +
                $"{randomizedReward.Id}"
            );


            RewardGrantQueue.Enqueue(
                location,
                rewardIndex,
                randomizedReward,
                -1,
                position,
                delegate
                {
                    __instance.Finish();
                }
            );


            return false;
        }
    }
}