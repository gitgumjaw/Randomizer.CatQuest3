using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RewardShuffler
    {
        public static void Shuffle(List<RewardSlot> slots, int seed)
        {
            if (slots == null || slots.Count <= 1) return;

            List<Reward> rewards = new List<Reward>();
            foreach (RewardSlot slot in slots)
            {
                slot.RandomizedReward = slot.VanillaReward;
                rewards.Add(slot.VanillaReward);
            }

            InjectSupplementalEquipmentRewards(slots, rewards, seed);

            Random random = new Random(seed);
            List<int> rewardOrder = new List<int>();
            for (int i = 0; i < rewards.Count; i++) rewardOrder.Add(i);
            ShuffleList(rewardOrder, random);

            List<int> slotOrder = new List<int>();
            for (int i = 0; i < slots.Count; i++) slotOrder.Add(i);
            ShuffleList(slotOrder, random);

            int[] assignedSlotByReward = new int[rewards.Count];
            int[] assignedRewardBySlot = new int[slots.Count];
            for (int i = 0; i < assignedSlotByReward.Length; i++)
                assignedSlotByReward[i] = -1;
            for (int i = 0; i < assignedRewardBySlot.Length; i++)
                assignedRewardBySlot[i] = -1;

            foreach (int slotIndex in slotOrder)
            {
                bool[] visitedRewards = new bool[rewards.Count];
                bool success = TryAssignReward(
                    slotIndex, slots, rewards, rewardOrder,
                    assignedSlotByReward, assignedRewardBySlot, visitedRewards);
                if (!success)
                {
                    Plugin.Log.LogError(
                        "Reward shuffle failed: no compatible assignment exists for all reward slots.");
                    foreach (RewardSlot slot in slots)
                        slot.RandomizedReward = slot.VanillaReward;
                    return;
                }
            }

            for (int i = 0; i < slots.Count; i++)
                slots[i].RandomizedReward = rewards[assignedRewardBySlot[i]];
        }

        private static void InjectSupplementalEquipmentRewards(
            List<RewardSlot> slots, List<Reward> rewards, int seed)
        {
            List<Reward> supplementalRewards = new List<Reward>();
            List<Reward> mewRewards = MewGameRewards.GetAll();
            if (mewRewards != null) supplementalRewards.AddRange(mewRewards);
            List<Reward> bagOnlyRewards = BagOnlyRewards.GetAll();
            if (bagOnlyRewards != null) supplementalRewards.AddRange(bagOnlyRewards);

            List<int> candidates = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                RewardSlot slot = slots[i];
                if (!slot.IsVanillaRandom || slot.VanillaReward == null ||
                    slot.VanillaReward.Type != RewardType.Equipment)
                    continue;
                candidates.Add(i);
            }
            if (candidates.Count == 0) return;

            if (candidates.Count < supplementalRewards.Count)
            {
                Plugin.Log.LogError(
                    "SUPPLEMENTAL EQUIPMENT INJECTION | Need " +
                    supplementalRewards.Count + " vanilla-random equipment slots, " +
                    "but only found " + candidates.Count + ". " +
                    "No supplemental equipment rewards were injected.");
                return;
            }

            // Stable slot ordering, then the same seeded shuffle used before.
            candidates.Sort(delegate (int a, int b)
            {
                int comparison = string.CompareOrdinal(
                    slots[a].Location.Key, slots[b].Location.Key);
                return comparison != 0 ? comparison :
                    slots[a].RewardIndex.CompareTo(slots[b].RewardIndex);
            });
            Random injectionRandom = new Random(seed);
            ShuffleList(candidates, injectionRandom);

            // Preserve the existing 1-per-item Mew Game and bag-only injection.
            for (int i = 0; i < supplementalRewards.Count; i++)
                rewards[candidates[i]] = supplementalRewards[i];

            // Every remaining vanilla-random equipment slot draws from the
            // exact same validated whitelist as Bird Poop / normal bag drops.
            // Independent seeded RNG: repeatable seed, no influence on the
            // supplemental slot selection or the main shuffler RNG stream.
            List<Reward> bagPool = RandomEquipmentDropPool.GetShuffleCandidateRewards();
            int remaining = candidates.Count - supplementalRewards.Count;
            if (remaining == 0) return;
            if (bagPool.Count == 0)
            {
                Plugin.Log.LogError(
                    "VANILLA RANDOM EQUIPMENT | Bag-drop pool empty; " +
                    "leaving " + remaining + " slots on their original vanilla resolved rewards.");
                return;
            }

            Random poolRandom = new Random(unchecked(seed ^ 0x42A69317));
            for (int i = supplementalRewards.Count; i < candidates.Count; i++)
            {
                // Draw WITH replacement, exactly like a repeatable bag drop.
                // These rewards enter the normal one-to-one shuffle afterward.
                rewards[candidates[i]] = bagPool[poolRandom.Next(bagPool.Count)];
            }

            Plugin.Log.LogInfo(
                "VANILLA RANDOM EQUIPMENT | Supplemental:" + supplementalRewards.Count +
                " | Bag-pool substitutions:" + remaining +
                " | Pool size:" + bagPool.Count +
                " | Attempt seed:" + seed);
        }

        private static bool TryAssignReward(
            int slotIndex, List<RewardSlot> slots, List<Reward> rewards,
            List<int> rewardOrder, int[] assignedSlotByReward,
            int[] assignedRewardBySlot, bool[] visitedRewards)
        {
            foreach (int rewardIndex in rewardOrder)
            {
                if (visitedRewards[rewardIndex]) continue;
                Reward reward = rewards[rewardIndex];
                if (!slots[slotIndex].CanAccept(reward)) continue;

                visitedRewards[rewardIndex] = true;
                int previousSlot = assignedSlotByReward[rewardIndex];
                if (previousSlot == -1 || TryAssignReward(
                    previousSlot, slots, rewards, rewardOrder,
                    assignedSlotByReward, assignedRewardBySlot, visitedRewards))
                {
                    assignedSlotByReward[rewardIndex] = slotIndex;
                    assignedRewardBySlot[slotIndex] = rewardIndex;
                    return true;
                }
            }
            return false;
        }

        private static void ShuffleList(List<int> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
