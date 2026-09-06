using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RewardShuffler
    {
        public static void Shuffle(
            List<RewardSlot> slots,
            int seed)
        {
            if (slots == null || slots.Count <= 1)
            {
                return;
            }

            List<Reward> rewards =
                new List<Reward>();

            foreach (RewardSlot slot in slots)
            {
                // Reset first in case a seed is generated more than once.
                slot.RandomizedReward = slot.VanillaReward;

                rewards.Add(slot.VanillaReward);
            }

            Random random = new Random(seed);

            // Randomize the order in which we consider rewards.
            List<int> rewardOrder = new List<int>();

            for (int i = 0; i < rewards.Count; i++)
            {
                rewardOrder.Add(i);
            }

            ShuffleList(rewardOrder, random);

            // Randomize slot processing order too.
            List<int> slotOrder = new List<int>();

            for (int i = 0; i < slots.Count; i++)
            {
                slotOrder.Add(i);
            }

            ShuffleList(slotOrder, random);

            // For each reward, remember which slot currently owns it.
            int[] assignedSlotByReward =
                new int[rewards.Count];

            // For each slot, remember which reward it received.
            int[] assignedRewardBySlot =
                new int[slots.Count];

            for (int i = 0; i < assignedSlotByReward.Length; i++)
            {
                assignedSlotByReward[i] = -1;
            }

            for (int i = 0; i < assignedRewardBySlot.Length; i++)
            {
                assignedRewardBySlot[i] = -1;
            }

            foreach (int slotIndex in slotOrder)
            {
                bool[] visitedRewards =
                    new bool[rewards.Count];

                bool success =
                    TryAssignReward(
                        slotIndex,
                        slots,
                        rewards,
                        rewardOrder,
                        assignedSlotByReward,
                        assignedRewardBySlot,
                        visitedRewards
                    );

                if (!success)
                {
                    Plugin.Log.LogError(
                        "Reward shuffle failed: no compatible " +
                        "assignment exists for all reward slots."
                    );

                    // Leave everything vanilla rather than producing
                    // an incomplete or invalid randomization.
                    foreach (RewardSlot slot in slots)
                    {
                        slot.RandomizedReward =
                            slot.VanillaReward;
                    }

                    return;
                }
            }

            // Apply the completed assignment only after we know
            // every slot has a valid reward.
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].RandomizedReward =
                    rewards[assignedRewardBySlot[i]];
            }
        }

        private static bool TryAssignReward(
            int slotIndex,
            List<RewardSlot> slots,
            List<Reward> rewards,
            List<int> rewardOrder,
            int[] assignedSlotByReward,
            int[] assignedRewardBySlot,
            bool[] visitedRewards)
        {
            foreach (int rewardIndex in rewardOrder)
            {
                if (visitedRewards[rewardIndex])
                {
                    continue;
                }

                Reward reward =
                    rewards[rewardIndex];

                if (!slots[slotIndex].CanAccept(reward))
                {
                    continue;
                }

                visitedRewards[rewardIndex] = true;

                int previousSlot =
                    assignedSlotByReward[rewardIndex];

                // Reward is unused, or we can move its current
                // owner onto another compatible reward.
                if (previousSlot == -1 ||
                    TryAssignReward(
                        previousSlot,
                        slots,
                        rewards,
                        rewardOrder,
                        assignedSlotByReward,
                        assignedRewardBySlot,
                        visitedRewards))
                {
                    assignedSlotByReward[rewardIndex] =
                        slotIndex;

                    assignedRewardBySlot[slotIndex] =
                        rewardIndex;

                    return true;
                }
            }

            return false;
        }

        private static void ShuffleList(
            List<int> list,
            Random random)
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