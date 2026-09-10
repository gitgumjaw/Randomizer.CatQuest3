using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Randomizer.CatQuest3
{
    public static class CatalogLoader
    {
        public static List<CatalogRewardLocation> Load()
        {
            string pluginDirectory =
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                );

            string catalogPath =
                Path.Combine(
                    pluginDirectory,
                    "Data",
                    "RewardCatalog.txt"
                );

            if (!File.Exists(catalogPath))
            {
                Plugin.Log.LogError(
                    $"Reward catalog not found: {catalogPath}"
                );

                return new List<CatalogRewardLocation>();
            }

            string[] lines =
                File.ReadAllLines(catalogPath);

            List<CatalogRewardLocation> locations =
                new List<CatalogRewardLocation>();

            CatalogRewardLocation currentLocation =
                null;

            WeightedRewardSlot currentSlot =
                null;

            foreach (string rawLine in lines)
            {
                string line =
                    rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith(
                    "Catalog Locations:",
                    StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.StartsWith(
                    "LOCATION|",
                    StringComparison.Ordinal))
                {
                    string key =
                        line.Substring(
                            "LOCATION|".Length
                        );

                    currentLocation =
                        new CatalogRewardLocation(
                            key
                        );

                    locations.Add(
                        currentLocation
                    );

                    currentSlot =
                        null;

                    continue;
                }

                if (line.StartsWith(
                    "LABEL|",
                    StringComparison.Ordinal))
                {
                    if (currentLocation == null)
                    {
                        continue;
                    }

                    currentLocation.Label =
                        line.Substring(
                            "LABEL|".Length
                        );

                    continue;
                }

                if (line.StartsWith(
                    "TRIGGER|",
                    StringComparison.Ordinal))
                {
                    if (currentLocation == null)
                    {
                        continue;
                    }

                    string trigger =
                        line.Substring(
                            "TRIGGER|".Length
                        );

                    currentLocation.AddTrigger(
                        trigger
                    );

                    continue;
                }

                if (line.StartsWith(
                    "SLOT|",
                    StringComparison.Ordinal))
                {
                    if (currentLocation == null)
                    {
                        continue;
                    }

                    bool allowCollectibles =
                        ParseBoolField(
                            line,
                            "AllowCollectibles",
                            true
                        );

                    currentSlot =
                        new WeightedRewardSlot(
                            new List<WeightedRewardOption>(),
                            allowCollectibles
                        );

                    currentLocation.AddSlot(
                        currentSlot
                    );

                    continue;
                }

                if (line.StartsWith(
                    "OPTION|",
                    StringComparison.Ordinal))
                {
                    if (currentSlot == null)
                    {
                        continue;
                    }

                    WeightedRewardOption option =
                        ParseOption(
                            line
                        );

                    if (option != null)
                    {
                        currentSlot.Options.Add(
                            option
                        );
                    }
                }
            }

            int slotCount = 0;

            foreach (CatalogRewardLocation location in locations)
            {
                slotCount +=
                    location.RewardSlots.Count;
            }

            Plugin.Log.LogInfo(
                $"Loaded reward catalog: " +
                $"{locations.Count} locations | " +
                $"{slotCount} slots"
            );

            return locations;
        }

        private static WeightedRewardOption ParseOption(
            string line)
        {
            string[] parts =
                line.Split('|');

            if (parts.Length < 4)
            {
                return null;
            }

            if (!Enum.TryParse(
                parts[1],
                out RewardType rewardType))
            {
                return null;
            }

            string rewardId =
                parts[2];

            int weight =
                ParseIntField(
                    line,
                    "Weight",
                    1
                );

            CollectibleRewardData collectibleData =
                null;

            if (rewardType ==
                RewardType.Collectible)
            {
                collectibleData =
                    ParseCollectibleData(
                        line,
                        rewardId
                    );
            }

            Reward reward =
                new Reward(
                    rewardType,
                    rewardId,
                    collectibleData
                );

            return new WeightedRewardOption(
                reward,
                weight
            );
        }

        private static CollectibleRewardData
            ParseCollectibleData(
                string line,
                string rewardId)
        {
            string prefabName =
                ParseStringField(
                    line,
                    "PrefabName",
                    rewardId
                );

            int quantity =
                ParseIntField(
                    line,
                    "Quantity",
                    1
                );

            int value =
                ParseIntField(
                    line,
                    "Value",
                    1
                );

            bool randomQuantity =
                ParseBoolField(
                    line,
                    "RandomQuantity",
                    false
                );

            int quantityMin =
                ParseIntField(
                    line,
                    "QuantityMin",
                    1
                );

            int quantityMax =
                ParseIntField(
                    line,
                    "QuantityMax",
                    1
                );

            bool randomValue =
                ParseBoolField(
                    line,
                    "RandomValue",
                    false
                );

            int valueMin =
                ParseIntField(
                    line,
                    "ValueMin",
                    1
                );

            int valueMax =
                ParseIntField(
                    line,
                    "ValueMax",
                    1
                );

            bool ignoreCollectibleMultiplier =
                ParseBoolField(
                    line,
                    "IgnoreCollectibleMultiplier",
                    false
                );

            int quantityMultiplier =
                ParseIntField(
                    line,
                    "QuantityMultiplier",
                    1
                );

            int valueMultiplier =
                ParseIntField(
                    line,
                    "ValueMultiplier",
                    1
                );

            return new CollectibleRewardData(
                prefabName,
                quantity,
                value,
                randomQuantity,
                quantityMin,
                quantityMax,
                randomValue,
                valueMin,
                valueMax,
                ignoreCollectibleMultiplier,
                quantityMultiplier,
                valueMultiplier
            );
        }

        private static string ParseStringField(
            string line,
            string fieldName,
            string defaultValue)
        {
            string[] parts =
                line.Split('|');

            string prefix =
                fieldName + ":";

            foreach (string part in parts)
            {
                if (!part.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                return part.Substring(
                    prefix.Length
                );
            }

            return defaultValue;
        }

        private static int ParseIntField(
            string line,
            string fieldName,
            int defaultValue)
        {
            string value =
                ParseStringField(
                    line,
                    fieldName,
                    null
                );

            if (value != null &&
                int.TryParse(
                    value,
                    out int result))
            {
                return result;
            }

            return defaultValue;
        }

        private static bool ParseBoolField(
            string line,
            string fieldName,
            bool defaultValue)
        {
            string value =
                ParseStringField(
                    line,
                    fieldName,
                    null
                );

            if (value != null &&
                bool.TryParse(
                    value,
                    out bool result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}