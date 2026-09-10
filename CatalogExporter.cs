using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;

namespace Randomizer.CatQuest3
{
    public static class CatalogExporter
    {
        private static string CatalogPath =>
            Path.Combine(
                Paths.ConfigPath,
                "CatQuest3Randomizer_Catalog.txt"
            );

        public static void Export()
        {
            Dictionary<string, string> catalog =
                LoadExistingCatalog();

            List<CatalogRewardLocation> currentLocations =
                CatalogScanResults.GetAll();

            foreach (CatalogRewardLocation location in currentLocations)
            {
                string newBlock =
                    SerializeLocation(location);

                /*
                 * Some dynamically-created rewards can initially be
                 * observed before all of their slots have been populated.
                 *
                 * If an older persisted version of the same location has
                 * MORE slots than the current observation, preserve the
                 * more complete version.
                 */
                if (catalog.TryGetValue(
                    location.Key,
                    out string existingBlock))
                {
                    int existingSlots =
                        CountSlots(existingBlock);

                    int newSlots =
                        location.RewardSlots.Count;

                    if (existingSlots > newSlots)
                    {
                        continue;
                    }
                }

                catalog[location.Key] =
                    newBlock;
            }

            List<string> keys =
                new List<string>(catalog.Keys);

            keys.Sort(
                StringComparer.Ordinal
            );

            StringBuilder output =
                new StringBuilder();

            output.AppendLine(
                $"Catalog Locations: {keys.Count}"
            );

            output.AppendLine();

            foreach (string key in keys)
            {
                output.Append(
                    catalog[key]
                );

                if (!catalog[key].EndsWith(
                    Environment.NewLine))
                {
                    output.AppendLine();
                }

                output.AppendLine();
            }

            File.WriteAllText(
                CatalogPath,
                output.ToString()
            );

            CatalogCoverage.WriteReport();

            CatalogScanResults.MarkClean();

            Plugin.Log.LogInfo(
                $"Exported persistent catalog: " +
                $"{keys.Count} locations | " +
                $"{CatalogPath}"
            );
        }

        private static Dictionary<string, string>
            LoadExistingCatalog()
        {
            Dictionary<string, string> locations =
                new Dictionary<string, string>();

            if (!File.Exists(CatalogPath))
            {
                return locations;
            }

            string[] lines =
                File.ReadAllLines(
                    CatalogPath
                );

            string currentKey = null;

            StringBuilder currentBlock =
                null;

            foreach (string line in lines)
            {
                if (line.StartsWith(
                    "LOCATION|",
                    StringComparison.Ordinal))
                {
                    SaveBlock(
                        locations,
                        currentKey,
                        currentBlock
                    );

                    currentKey =
                        line.Substring(
                            "LOCATION|".Length
                        );

                    currentBlock =
                        new StringBuilder();

                    currentBlock.AppendLine(
                        line
                    );

                    continue;
                }

                if (currentBlock != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    currentBlock.AppendLine(
                        line
                    );
                }
            }

            SaveBlock(
                locations,
                currentKey,
                currentBlock
            );

            return locations;
        }

        private static void SaveBlock(
            Dictionary<string, string> locations,
            string key,
            StringBuilder block)
        {
            if (key == null ||
                block == null)
            {
                return;
            }

            locations[key] =
                block.ToString();
        }

        private static int CountSlots(
            string block)
        {
            if (string.IsNullOrEmpty(block))
            {
                return 0;
            }

            int count = 0;

            using (StringReader reader =
                   new StringReader(block))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith(
                        "SLOT|",
                        StringComparison.Ordinal))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static string SerializeLocation(
            CatalogRewardLocation location)
        {
            StringBuilder output =
                new StringBuilder();

            output.AppendLine(
                $"LOCATION|{location.Key}"
            );

            for (int slotIndex = 0;
                 slotIndex < location.RewardSlots.Count;
                 slotIndex++)
            {
                WeightedRewardSlot slot =
                    location.RewardSlots[slotIndex];

                output.AppendLine(
                    $"SLOT|{slotIndex}|" +
                    $"AllowCollectibles:{slot.AllowCollectibles}|" +
                    $"Options:{slot.Options.Count}"
                );

                foreach (WeightedRewardOption option in slot.Options)
                {
                    if (option?.Reward == null)
                    {
                        continue;
                    }

                    Reward reward =
                        option.Reward;

                    if (reward.Type ==
                            RewardType.Collectible &&
                        reward.CollectibleData != null)
                    {
                        CollectibleRewardData data =
                            reward.CollectibleData;

                        output.AppendLine(
                            $"OPTION|" +
                            $"{reward.Type}|" +
                            $"{reward.Id}|" +
                            $"Weight:{option.Weight}|" +
                            $"PrefabName:{data.PrefabName}|" +
                            $"Quantity:{data.Quantity}|" +
                            $"Value:{data.Value}|" +
                            $"RandomQuantity:{data.RandomQuantity}|" +
                            $"QuantityMin:{data.QuantityMin}|" +
                            $"QuantityMax:{data.QuantityMax}|" +
                            $"RandomValue:{data.RandomValue}|" +
                            $"ValueMin:{data.ValueMin}|" +
                            $"ValueMax:{data.ValueMax}|" +
                            $"IgnoreCollectibleMultiplier:" +
                            $"{data.IgnoreCollectibleMultiplier}|" +
                            $"QuantityMultiplier:" +
                            $"{data.QuantityMultiplier}|" +
                            $"ValueMultiplier:" +
                            $"{data.ValueMultiplier}"
                        );
                    }
                    else
                    {
                        output.AppendLine(
                            $"OPTION|" +
                            $"{reward.Type}|" +
                            $"{reward.Id}|" +
                            $"Weight:{option.Weight}"
                        );
                    }
                }
            }

            return output.ToString();
        }
    }
}