using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;

namespace Randomizer.CatQuest3
{
    public static class SpoilerLogWriter
    {
        public static void Write(
            int profileIndex,
            RandomizerRunConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            string directory =
                Path.Combine(
                    Paths.ConfigPath,
                    "Randomizer.CatQuest3",
                    "SaveSlots"
                );

            Directory.CreateDirectory(directory);

            string path =
                Path.Combine(
                    directory,
                    $"profile_{profileIndex}_spoiler.txt"
                );

            List<RewardLocation> locations =
                RewardCatalog.GetAll();

            locations.Sort(
                (a, b) =>
                {
                    string aLabel =
                        string.IsNullOrWhiteSpace(a.Label)
                            ? a.Key
                            : a.Label;

                    string bLabel =
                        string.IsNullOrWhiteSpace(b.Label)
                            ? b.Key
                            : b.Label;

                    int labelComparison =
                        string.Compare(
                            aLabel,
                            bLabel,
                            StringComparison.Ordinal
                        );

                    if (labelComparison != 0)
                    {
                        return labelComparison;
                    }

                    return string.Compare(
                        a.Key,
                        b.Key,
                        StringComparison.Ordinal
                    );
                }
            );

            using (
                StreamWriter writer =
                    new StreamWriter(
                        path,
                        false
                    )
            )
            {
                writer.WriteLine(
                    "Cat Quest 3 Randomizer Spoiler Log"
                );
                writer.WriteLine(
                    $"Profile: {profileIndex}"
                );
                writer.WriteLine(
                    $"Seed: {configuration.SeedText}"
                );
                writer.WriteLine(
                    $"Hash: {configuration.SeedValue}"
                );
                writer.WriteLine();

                foreach (
                    RewardLocation location
                    in locations)
                {
                    string label =
                        string.IsNullOrWhiteSpace(location.Label)
                            ? location.Key
                            : location.Label;

                    writer.WriteLine(
                        $"[{label}]"
                    );

                    for (
                        int slotIndex = 0;
                        slotIndex <
                            location.RandomizedRewards.Count;
                        slotIndex++)
                    {
                        Reward randomizedReward =
                            location.RandomizedRewards[slotIndex];

                        writer.WriteLine(
                            $"Slot {slotIndex} - " +
                            FormatReward(randomizedReward)
                        );
                    }

                    writer.WriteLine();
                }
            }

            Plugin.Log.LogInfo(
                "RANDOMIZER SPOILER | " +
                $"Wrote profile {profileIndex} spoiler log."
            );
        }

        private static string FormatReward(
            Reward reward)
        {
            if (reward == null)
            {
                return "<null>";
            }

            if (reward.Type == RewardType.Collectible)
            {
                return FormatCollectible(reward);
            }

            return RewardNameResolver.GetName(reward);
        }

        private static string FormatCollectible(
            Reward reward)
        {
            CollectibleRewardData data =
                reward.CollectibleData;

            if (data == null)
            {
                return RewardNameResolver.GetName(reward);
            }

            string collectibleName =
                GetCollectibleName(reward.Id);

            GetCollectibleAmountRange(
                data,
                out long minimum,
                out long maximum
            );

            if (minimum == maximum)
            {
                return $"{minimum} {collectibleName}";
            }

            return
                $"{minimum}-{maximum} {collectibleName}";
        }

        private static string GetCollectibleName(
            string rewardId)
        {
            switch (rewardId)
            {
                case "Collectible_Gold":
                    return "Gold";

                case "Collectible_Exp":
                    return "Experience";

                case "Collectible_Crystal":
                    return "Magic";

                default:
                    return string.IsNullOrWhiteSpace(rewardId)
                        ? "Collectible"
                        : rewardId;
            }
        }

        private static void GetCollectibleAmountRange(
            CollectibleRewardData data,
            out long minimum,
            out long maximum)
        {
            int minimumQuantity =
                data.RandomQuantity
                    ? data.QuantityMin
                    : data.Quantity;

            int maximumQuantity =
                data.RandomQuantity
                    ? data.QuantityMax
                    : data.Quantity;

            minimumQuantity =
                Math.Max(
                    1,
                    minimumQuantity *
                    data.QuantityMultiplier
                );

            maximumQuantity =
                Math.Max(
                    minimumQuantity,
                    maximumQuantity *
                    data.QuantityMultiplier
                );

            int minimumValue =
                data.RandomValue
                    ? data.ValueMin
                    : data.Value;

            int maximumValue =
                data.RandomValue
                    ? data.ValueMax
                    : data.Value;

            minimum = long.MaxValue;
            maximum = long.MinValue;

            for (
                int quantity = minimumQuantity;
                quantity <= maximumQuantity;
                quantity++)
            {
                long low =
                    CalculateCollectibleTotal(
                        data,
                        quantity,
                        minimumValue
                    );

                long high =
                    CalculateCollectibleTotal(
                        data,
                        quantity,
                        maximumValue
                    );

                minimum =
                    Math.Min(
                        minimum,
                        low
                    );

                maximum =
                    Math.Max(
                        maximum,
                        high
                    );
            }
        }

        private static long CalculateCollectibleTotal(
            CollectibleRewardData data,
            int quantity,
            int value)
        {
            long adjustedValue =
                value;

            if (!data.IgnoreCollectibleMultiplier)
            {
                adjustedValue *=
                    data.ValueMultiplier;
            }

            long valuePerCollectible =
                (adjustedValue +
                 quantity - 1) /
                quantity;

            return
                valuePerCollectible *
                quantity;
        }
    }
}
