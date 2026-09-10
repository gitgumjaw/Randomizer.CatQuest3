using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;

namespace Randomizer.CatQuest3
{
    public static class CatalogExporter
    {
        private const string ShipKeyGuid =
            "6726806ecdc575241a6d006d02344975";

        private const string InfinityKeyGuid =
            "0155432565248e64e8378122cbb41406";

        private const string NorthStarEssenceGuid =
            "57a1c1899c8b716478ee76b01f186fd4";

        private const string BirdPoopGuid =
            "321ebd701eff1264eb7f7296d033831c";

        private const string StarRuneGuid =
            "fd36ba1341aa4d24692cc3eedea20405";

        private const string EmptyGoldChaseChestGuid =
            "92f3c5e9cd46a684ea99a273b11a3507";


        /*
         * Minor quest-flow QuestItems which should remain vanilla
         * and should never enter the randomizer catalog.
         */
        private const string MeowgusMessageGuid =
            "0452a69a7109c824d9c129d41c76470f";

        private const string PurrseidonsTridentQuestItemGuid =
            "c72c63957f7aaca499df462cc0fede8c";

        private const string SiblingRivalryCombinedKeyGuid =
            "465282a2e0ff75143b1becfa19cecb29";


        /*
         * Lovepurr book QuestItem GUIDs.
         *
         * These rewards are based on chapter number,
         * not on which cave physically awarded them.
         */
        private const string LoveBookChapter1Guid =
            "9da86e0edf3a6f1449e64400a6a9096b";

        private const string LoveBookChapter2Guid =
            "a4f4a4f4891108b46b6fd1ef79ce9e0a";

        private const string LoveBookChapter3Guid =
            "092d5c5aeb3b19743a6f438818eb14ac";


        /*
         * Stable logical randomizer keys.
         *
         * These represent the three actual reward slots.
         * The physical cave/FSM paths appear beneath them
         * as TRIGGER lines.
         */
        private const string LoveBookChapter1Key =
            "LOGICAL|LoveBook|Chapter1";

        private const string LoveBookChapter2Key =
            "LOGICAL|LoveBook|Chapter2";

        private const string LoveBookChapter3Key =
            "LOGICAL|LoveBook|Chapter3";


        private static string CatalogPath =>
            Path.Combine(
                Paths.ConfigPath,
                "CatQuest3Randomizer_Catalog.txt"
            );


        /*
         * Safe to call on a fresh launch.
         *
         * Reads the full persisted catalog from disk,
         * applies structural cleanup,
         * refreshes metadata,
         * and writes everything back out.
         */
        public static void UpgradeExistingCatalogMetadata()
        {
            Dictionary<string, string> catalog =
                LoadExistingCatalog();

            if (catalog.Count == 0)
            {
                Plugin.Log.LogInfo(
                    "No persistent catalog found to upgrade."
                );

                return;
            }

            NormalizeCatalog(
                catalog
            );

            ApplyMetadataToCatalog(
                catalog
            );

            WriteCatalog(
                catalog
            );

            Plugin.Log.LogInfo(
                $"Updated persistent catalog: " +
                $"{catalog.Count} locations | " +
                $"Chest labels available: {ChestLabels.Count} | " +
                $"{CatalogPath}"
            );
        }


        public static void Export()
        {
            Dictionary<string, string> catalog =
                LoadExistingCatalog();

            List<CatalogRewardLocation> currentLocations =
                CatalogScanResults.GetAll();

            foreach (CatalogRewardLocation location in currentLocations)
            {
                string newBlock =
                    SerializeLocation(
                        location
                    );

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
                        CountSlots(
                            existingBlock
                        );

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

            /*
             * Structural cleanup happens AFTER runtime observations
             * are merged.
             *
             * This means excluded rewards can safely be observed again
             * without being allowed back into the persistent catalog.
             */
            NormalizeCatalog(
                catalog
            );

            ApplyMetadataToCatalog(
                catalog
            );

            WriteCatalog(
                catalog
            );

            CatalogCoverage.WriteReport();

            CatalogScanResults.MarkClean();

            Plugin.Log.LogInfo(
                $"Exported persistent catalog: " +
                $"{catalog.Count} locations | " +
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

            string currentKey =
                null;

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
                    if (string.IsNullOrWhiteSpace(
                        line))
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


        /*
         * Performs changes that affect which logical reward
         * locations exist in the release catalog.
         */
        private static void NormalizeCatalog(
            Dictionary<string, string> catalog)
        {
            RemoveExcludedLocations(
                catalog
            );

            NormalizeLoveBookLocations(
                catalog
            );

            /*
             * Run exclusions again after normalization in case
             * future cleanup rules ever operate on logical blocks.
             */
            RemoveExcludedLocations(
                catalog
            );
        }


        private static void RemoveExcludedLocations(
            Dictionary<string, string> catalog)
        {
            List<string> keys =
                new List<string>(
                    catalog.Keys
                );

            foreach (string key in keys)
            {
                string block =
                    catalog[key];

                if (ShouldExcludeLocation(
                    key,
                    block))
                {
                    catalog.Remove(
                        key
                    );
                }
            }
        }


        private static bool ShouldExcludeLocation(
            string key,
            string block)
        {
            /*
             * Intentional empty chest created after catching
             * the roaming gold coin.
             */
            if (string.Equals(
                key,
                EmptyGoldChaseChestGuid,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }


            /*
             * Star Rune remains vanilla.
             */
            if (BlockContainsReward(
                block,
                "QuestItem",
                StarRuneGuid))
            {
                return true;
            }


            /*
             * Minor quest-flow QuestItems which should remain
             * completely vanilla and never enter the shuffle pool.
             */
            if (BlockContainsReward(
                    block,
                    "QuestItem",
                    MeowgusMessageGuid) ||
                BlockContainsReward(
                    block,
                    "QuestItem",
                    PurrseidonsTridentQuestItemGuid) ||
                BlockContainsReward(
                    block,
                    "QuestItem",
                    SiblingRivalryCombinedKeyGuid))
            {
                return true;
            }


            /*
             * Gentlebros speech fragments are minor intermediate
             * quest-state items and stay completely vanilla.
             *
             * The final Gentlebros equipment reward is NOT removed.
             */
            if (key.IndexOf(
                    "|SQ_GentlebrosSpeech_Note01|",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                key.IndexOf(
                    "|SQ_GentlebrosSpeech_Note02|",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                key.IndexOf(
                    "|SQ_GentlebrosSpeech_Note03|",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }


        private static bool BlockContainsReward(
            string block,
            string rewardType,
            string rewardId)
        {
            if (string.IsNullOrEmpty(
                block))
            {
                return false;
            }

            string target =
                $"OPTION|{rewardType}|{rewardId}|";

            return block.IndexOf(
                target,
                StringComparison.OrdinalIgnoreCase
            ) >= 0;
        }


        /*
         * Converts physical LoveBook award paths into three
         * logical randomizer locations:
         *
         * Chapter 1
         * Chapter 2
         * Chapter 3
         *
         * Each logical location retains every known physical
         * FSM delivery path as a TRIGGER line.
         */
        private static void NormalizeLoveBookLocations(
            Dictionary<string, string> catalog)
        {
            NormalizeLoveBookChapter(
                catalog,
                LoveBookChapter1Key,
                LoveBookChapter1Guid
            );

            NormalizeLoveBookChapter(
                catalog,
                LoveBookChapter2Key,
                LoveBookChapter2Guid
            );

            NormalizeLoveBookChapter(
                catalog,
                LoveBookChapter3Key,
                LoveBookChapter3Guid
            );
        }


        private static void NormalizeLoveBookChapter(
            Dictionary<string, string> catalog,
            string logicalKey,
            string chapterGuid)
        {
            List<string> physicalKeys =
                new List<string>();

            HashSet<string> triggers =
                new HashSet<string>(
                    StringComparer.Ordinal
                );

            string sourceBlock =
                null;


            /*
             * If a logical block already exists from a previous
             * upgrade, preserve its reward data and triggers.
             */
            if (catalog.TryGetValue(
                logicalKey,
                out string existingLogicalBlock))
            {
                sourceBlock =
                    existingLogicalBlock;

                AddTriggersFromBlock(
                    existingLogicalBlock,
                    triggers
                );
            }


            /*
             * Find every physical FSM location which awards
             * this chapter.
             */
            foreach (KeyValuePair<string, string> pair
                     in catalog)
            {
                string key =
                    pair.Key;

                string block =
                    pair.Value;

                if (string.Equals(
                    key,
                    logicalKey,
                    StringComparison.Ordinal))
                {
                    continue;
                }

                if (!IsLoveBookPhysicalLocation(
                    key))
                {
                    continue;
                }

                if (!BlockContainsReward(
                    block,
                    "QuestItem",
                    chapterGuid))
                {
                    continue;
                }

                physicalKeys.Add(
                    key
                );

                triggers.Add(
                    key
                );

                if (sourceBlock == null)
                {
                    sourceBlock =
                        block;
                }
            }


            /*
             * Nothing to normalize.
             */
            if (sourceBlock == null)
            {
                return;
            }


            /*
             * Remove all physical representations of this
             * logical reward.
             */
            foreach (string physicalKey
                     in physicalKeys)
            {
                catalog.Remove(
                    physicalKey
                );
            }

            catalog.Remove(
                logicalKey
            );


            /*
             * Build one logical location which retains the
             * actual runtime delivery paths.
             */
            catalog[logicalKey] =
                BuildLogicalLocationBlock(
                    logicalKey,
                    sourceBlock,
                    triggers
                );
        }


        private static bool IsLoveBookPhysicalLocation(
            string key)
        {
            if (string.IsNullOrEmpty(
                key))
            {
                return false;
            }

            return
                key.IndexOf(
                    "LoveBook",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }


        private static void AddTriggersFromBlock(
            string block,
            HashSet<string> triggers)
        {
            if (string.IsNullOrEmpty(
                block))
            {
                return;
            }

            using (StringReader reader =
                   new StringReader(
                       block
                   ))
            {
                string line;

                while ((line =
                    reader.ReadLine()) != null)
                {
                    if (!line.StartsWith(
                        "TRIGGER|",
                        StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string trigger =
                        line.Substring(
                            "TRIGGER|".Length
                        );

                    if (!string.IsNullOrWhiteSpace(
                        trigger))
                    {
                        triggers.Add(
                            trigger
                        );
                    }
                }
            }
        }


        private static string BuildLogicalLocationBlock(
            string logicalKey,
            string sourceBlock,
            HashSet<string> triggers)
        {
            StringBuilder output =
                new StringBuilder();

            output.AppendLine(
                $"LOCATION|{logicalKey}"
            );


            List<string> sortedTriggers =
                new List<string>(
                    triggers
                );

            sortedTriggers.Sort(
                StringComparer.Ordinal
            );

            foreach (string trigger
                     in sortedTriggers)
            {
                output.AppendLine(
                    $"TRIGGER|{trigger}"
                );
            }


            /*
             * Copy the reward-slot data from one of the physical
             * observations.
             *
             * LOCATION/LABEL/CATEGORY/TRIGGER lines are rebuilt.
             */
            using (StringReader reader =
                   new StringReader(
                       sourceBlock
                   ))
            {
                string line;

                while ((line =
                    reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(
                        line))
                    {
                        continue;
                    }

                    if (line.StartsWith(
                            "LOCATION|",
                            StringComparison.Ordinal) ||
                        line.StartsWith(
                            "LABEL|",
                            StringComparison.Ordinal) ||
                        line.StartsWith(
                            "CATEGORY|",
                            StringComparison.Ordinal) ||
                        line.StartsWith(
                            "TRIGGER|",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    output.AppendLine(
                        line
                    );
                }
            }

            return output.ToString();
        }


        private static void ApplyMetadataToCatalog(
            Dictionary<string, string> catalog)
        {
            List<string> keys =
                new List<string>(
                    catalog.Keys
                );

            foreach (string key in keys)
            {
                catalog[key] =
                    AddMetadataToBlock(
                        key,
                        catalog[key]
                    );
            }
        }


        private static string AddMetadataToBlock(
            string key,
            string block)
        {
            if (string.IsNullOrWhiteSpace(
                block))
            {
                return block;
            }

            string[] lines =
                block.Replace(
                    "\r\n",
                    "\n"
                )
                .Replace(
                    '\r',
                    '\n'
                )
                .Split(
                    new[] { '\n' },
                    StringSplitOptions.RemoveEmptyEntries
                );

            HashSet<string> categories =
                GetCategoriesFromLines(
                    lines
                );

            StringBuilder output =
                new StringBuilder();

            output.AppendLine(
                $"LOCATION|{key}"
            );


            string label =
                GetLocationLabel(
                    key
                );

            if (!string.IsNullOrWhiteSpace(
                label))
            {
                output.AppendLine(
                    $"LABEL|{label}"
                );
            }


            if (categories.Count > 0)
            {
                List<string> sortedCategories =
                    new List<string>(
                        categories
                    );

                sortedCategories.Sort(
                    StringComparer.Ordinal
                );

                output.AppendLine(
                    $"CATEGORY|" +
                    string.Join(
                        ";",
                        sortedCategories
                    )
                );
            }


            foreach (string line in lines)
            {
                if (line.StartsWith(
                        "LOCATION|",
                        StringComparison.Ordinal) ||
                    line.StartsWith(
                        "LABEL|",
                        StringComparison.Ordinal) ||
                    line.StartsWith(
                        "CATEGORY|",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                output.AppendLine(
                    line
                );
            }

            return output.ToString();
        }


        private static string GetLocationLabel(
    string key)
        {
            if (ChestLabels.TryGetLabel(
                key,
                out string chestLabel))
            {
                return chestLabel;
            }


            if (ScriptedLocationLabels.TryGetLabel(
                key,
                out string scriptedLabel))
            {
                return scriptedLabel;
            }


            if (string.Equals(
                key,
                LoveBookChapter1Key,
                StringComparison.Ordinal))
            {
                return "Lovepurr Book — Chapter 1";
            }

            if (string.Equals(
                key,
                LoveBookChapter2Key,
                StringComparison.Ordinal))
            {
                return "Lovepurr Book — Chapter 2";
            }

            if (string.Equals(
                key,
                LoveBookChapter3Key,
                StringComparison.Ordinal))
            {
                return "Lovepurr Book — Chapter 3";
            }

            return null;
        }


        private static HashSet<string>
            GetCategoriesFromLines(
                string[] lines)
        {
            HashSet<string> categories =
                new HashSet<string>(
                    StringComparer.Ordinal
                );

            foreach (string line in lines)
            {
                if (!line.StartsWith(
                    "OPTION|",
                    StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts =
                    line.Split('|');

                if (parts.Length < 3)
                {
                    continue;
                }

                string rewardType =
                    parts[1];

                string rewardId =
                    parts[2];

                AddMainCategory(
                    categories,
                    rewardType
                );

                AddSpecialCategories(
                    categories,
                    rewardId
                );
            }

            return categories;
        }


        private static void AddMainCategory(
            HashSet<string> categories,
            string rewardType)
        {
            switch (rewardType)
            {
                case "Equipment":
                case "Blueprint":
                case "ShipSpell":
                    categories.Add(
                        "Equipment & Blueprints"
                    );
                    break;

                case "Spell":
                    categories.Add(
                        "Spells"
                    );
                    break;

                case "QuestItem":
                    categories.Add(
                        "Quest Items"
                    );
                    break;

                case "ManaCrystal":
                    categories.Add(
                        "Mana Crystals"
                    );
                    break;

                case "Collectible":
                    categories.Add(
                        "Resources/Collectibles"
                    );
                    break;
            }
        }


        private static void AddSpecialCategories(
            HashSet<string> categories,
            string rewardId)
        {
            if (string.Equals(
                rewardId,
                ShipKeyGuid,
                StringComparison.OrdinalIgnoreCase))
            {
                categories.Add(
                    "Ship Key"
                );
            }

            if (string.Equals(
                rewardId,
                InfinityKeyGuid,
                StringComparison.OrdinalIgnoreCase))
            {
                categories.Add(
                    "Infinity Key"
                );
            }

            if (string.Equals(
                rewardId,
                NorthStarEssenceGuid,
                StringComparison.OrdinalIgnoreCase))
            {
                categories.Add(
                    "North Star Essence"
                );
            }

            if (string.Equals(
                rewardId,
                BirdPoopGuid,
                StringComparison.OrdinalIgnoreCase))
            {
                categories.Add(
                    "Bird Poop"
                );
            }
        }


        private static void WriteCatalog(
            Dictionary<string, string> catalog)
        {
            List<string> keys =
                new List<string>(
                    catalog.Keys
                );

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
        }


        private static int CountSlots(
            string block)
        {
            if (string.IsNullOrEmpty(
                block))
            {
                return 0;
            }

            int count =
                0;

            using (StringReader reader =
                   new StringReader(
                       block
                   ))
            {
                string line;

                while ((line =
                    reader.ReadLine()) != null)
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
                    location.RewardSlots[
                        slotIndex
                    ];

                output.AppendLine(
                    $"SLOT|{slotIndex}|" +
                    $"AllowCollectibles:{slot.AllowCollectibles}|" +
                    $"Options:{slot.Options.Count}"
                );


                foreach (WeightedRewardOption option
                         in slot.Options)
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