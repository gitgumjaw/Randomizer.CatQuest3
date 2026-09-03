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
                rewards.Add(slot.VanillaReward);
            }

            Random random = new Random(seed);

            for (int i = rewards.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                Reward temp = rewards[i];
                rewards[i] = rewards[j];
                rewards[j] = temp;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].RandomizedReward = rewards[i];
            }
        }
    }
}