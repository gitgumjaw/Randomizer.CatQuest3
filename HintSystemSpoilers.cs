
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class HintSystemSpoilers
    {
        private const long MinimumCollectibleValue = 50000;

        private static readonly HashSet<string>
            ExcludedAreas = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "Catuga",
                "Zero Dimension",
                "Code Isle",
                "Code Island",
                "Sandy Isle",
                "Sandy Islands",
                "Starting Loadout"
            };


        public static List<RewardSlot> GetEligibleSlots(
            List<RewardSlot> shuffledSlots)
        {
            List<RewardSlot> result =
                new List<RewardSlot>();

            if (shuffledSlots == null)
            {
                return result;
            }

            foreach (RewardSlot slot in shuffledSlots)
            {
                if (slot == null ||
                    slot.Location == null ||
                    slot.RandomizedReward == null)
                {
                    continue;
                }

                if (IsExcludedLocation(slot.Location))
                {
                    continue;
                }

                Reward reward = slot.RandomizedReward;

                if (reward.Type == RewardType.Collectible)
                {
                    if (reward.CollectibleData == null)
                    {
                        continue;
                    }

                    SpoilerLogWriter.GetCollectibleAmountRange(
                        reward.CollectibleData,
                        out long minimum,
                        out long maximum
                    );

                    if (minimum < MinimumCollectibleValue)
                    {
                        continue;
                    }
                }

                result.Add(slot);
            }

            result.Sort((a, b) =>
            {
                int locationComparison =
                    StringComparer.Ordinal.Compare(
                        a.Location.Key,
                        b.Location.Key
                    );

                if (locationComparison != 0)
                {
                    return locationComparison;
                }

                return a.RewardIndex.CompareTo(b.RewardIndex);
            });

            return result;
        }


        private static bool IsExcludedLocation(
            RewardLocation location)
        {
            if (location.Key.StartsWith(
                "RANDOMIZER|STARTING|",
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(location.Area))
            {
                return false;
            }

            foreach (string rawTag in location.Area.Split(';'))
            {
                if (ExcludedAreas.Contains(rawTag.Trim()))
                {
                    return true;
                }
            }

            return false;
        }


        public static List<RewardSlot> GetShuffledSlots(
            List<RewardSlot> shuffledSlots,
            int seed)
        {
            List<RewardSlot> spoilers =
                GetEligibleSlots(shuffledSlots);

            // Independent from the reward shuffle and area clues.
            Random random = new Random(
                unchecked(seed ^ 0x2C71A5E3)
            );

            // Fisher-Yates shuffle.
            for (int i = spoilers.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                RewardSlot temporary = spoilers[i];
                spoilers[i] = spoilers[j];
                spoilers[j] = temporary;
            }

            return spoilers;
        }

        public static void LogEligibleSlots(
            List<RewardSlot> shuffledSlots)
        {
            List<RewardSlot> slots =
                GetEligibleSlots(shuffledSlots);

            int collectibles = 0;

            foreach (RewardSlot slot in slots)
            {
                if (slot.RandomizedReward.Type ==
                    RewardType.Collectible)
                {
                    collectibles++;
                }
            }

            Plugin.Log.LogInfo(
                "HINT SPOILER | Eligible Slots: " +
                slots.Count +
                " | Collectibles: " +
                collectibles +
                " | Other Rewards: " +
                (slots.Count - collectibles)
            );

            // Show the first ten candidates for inspection.
            int previewCount = Math.Min(10, slots.Count);

            for (int i = 0; i < previewCount; i++)
            {
                RewardSlot slot = slots[i];

                Plugin.Log.LogInfo(
                    "HINT SPOILER SAMPLE | " +
                    SpoilerLogWriter.FormatReward(
                        slot.RandomizedReward
                    ) +
                    " | Location: " +
                    (slot.Location.Label ?? slot.Location.Key) +
                    " | Area: " +
                    slot.Location.Area +
                    " | Slot: " +
                    slot.RewardIndex
                );
            }
        }


        public static void LogSpoilerSelection(
            List<RewardSlot> shuffledSlots,
            int seed)
        {
            List<RewardSlot> spoilers =
                GetShuffledSlots(shuffledSlots, seed);

            int count = Math.Min(12, spoilers.Count);

            for (int i = 0; i < count; i++)
            {
                RewardSlot slot = spoilers[i];

                string area = "";

                if (!string.IsNullOrWhiteSpace(
                    slot.Location.Area))
                {
                    area = slot.Location.Area
                        .Split(';')[0]
                        .Trim();
                }

                Plugin.Log.LogInfo(
                    "HINT SPOILER SELECTION | " +
                    (i + 1) +
                    " | " +
                    SpoilerLogWriter.FormatReward(
                        slot.RandomizedReward
                    ) +
                    " | Area: " + area +
                    " | Location: " +
                    (slot.Location.Label ?? slot.Location.Key) +
                    " | Slot: " + slot.RewardIndex
                );
            }

            Plugin.Log.LogInfo(
                "HINT SPOILER SELECTION | " +
                "Available unique slots: " +
                spoilers.Count
            );
        }

    }
}
