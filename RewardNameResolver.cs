using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Randomizer.CatQuest3
{
    public static class RewardNameResolver
    {
        private static readonly Dictionary<string, string> names =
            new Dictionary<string, string>(
                StringComparer.Ordinal
            );

        private static bool loaded;

        public static string GetName(
            Reward reward)
        {
            if (reward == null)
            {
                return "<null>";
            }

            if (reward.Type == RewardType.ManaCrystal)
            {
                return "Mana Crystal";
            }

            EnsureLoaded();

            string key =
                CreateKey(
                    reward.Type,
                    reward.Id
                );

            if (names.TryGetValue(
                key,
                out string name))
            {
                return name;
            }

            if (!string.IsNullOrWhiteSpace(reward.Id))
            {
                return reward.Id;
            }

            return reward.Type.ToString();
        }

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;

            string pluginDirectory =
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                );

            string namesPath =
                Path.Combine(
                    pluginDirectory,
                    "Data",
                    "RewardNames.txt"
                );

            if (!File.Exists(namesPath))
            {
                Plugin.Log.LogWarning(
                    $"Reward names file not found: {namesPath}"
                );

                return;
            }

            string[] lines =
                File.ReadAllLines(namesPath);

            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line) ||
                    line.StartsWith(
                        "#",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts =
                    line.Split(
                        new[] { '|' },
                        3
                    );

                if (parts.Length != 3 ||
                    !Enum.TryParse(
                        parts[0],
                        out RewardType rewardType))
                {
                    continue;
                }

                string rewardId =
                    parts[1].Trim();

                string name =
                    parts[2].Trim();

                if (string.IsNullOrWhiteSpace(rewardId) ||
                    string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                names[
                    CreateKey(
                        rewardType,
                        rewardId
                    )
                ] = name;
            }

            Plugin.Log.LogInfo(
                $"Loaded reward names: {names.Count}"
            );
        }

        private static string CreateKey(
            RewardType rewardType,
            string rewardId)
        {
            return
                rewardType.ToString() +
                "|" +
                (rewardId ?? string.Empty);
        }
    }
}
