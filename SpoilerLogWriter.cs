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
                    writer.WriteLine(
                        $"Key: {location.Key}"
                    );

                    int slotCount =
                        Math.Min(
                            location.VanillaRewards.Count,
                            location.RandomizedRewards.Count
                        );

                    for (
                        int slotIndex = 0;
                        slotIndex < slotCount;
                        slotIndex++)
                    {
                        Reward vanillaReward =
                            location.VanillaRewards[slotIndex];

                        Reward randomizedReward =
                            location.RandomizedRewards[slotIndex];

                        writer.WriteLine(
                            $"Slot {slotIndex}"
                        );
                        writer.WriteLine(
                            $"Vanilla: {FormatReward(vanillaReward)}"
                        );
                        writer.WriteLine(
                            $"Randomized: {FormatReward(randomizedReward)}"
                        );
                        writer.WriteLine();
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

            return $"{reward.Type}:{reward.Id}";
        }
    }
}
