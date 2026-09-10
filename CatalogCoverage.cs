using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;

namespace Randomizer.CatQuest3
{
    public static class CatalogCoverage
    {
        private class ReferenceEntry
        {
            public string Guid { get; }
            public string Name { get; }

            public ReferenceEntry(string guid, string name)
            {
                Guid = guid;
                Name = name;
            }
        }

        private static string CatalogPath =>
            Path.Combine(
                Paths.ConfigPath,
                "CatQuest3Randomizer_Catalog.txt"
            );

        private static string ChestReferencePath =>
            Path.Combine(
                Paths.ConfigPath,
                "ChestCoverageReference.txt"
            );

        private static string QuestItemReferencePath =>
            Path.Combine(
                Paths.ConfigPath,
                "QuestItemCoverageReference.txt"
            );

        private static string CoveragePath =>
            Path.Combine(
                Paths.ConfigPath,
                "CatQuest3Randomizer_Coverage.txt"
            );

        public static void WriteReport()
        {
            if (!File.Exists(CatalogPath))
            {
                return;
            }

            List<ReferenceEntry> chestReferences =
                LoadReferenceFile(ChestReferencePath);

            List<ReferenceEntry> questItemReferences =
                LoadReferenceFile(QuestItemReferencePath);

            HashSet<string> observedChestIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            HashSet<string> observedQuestItemIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ReadObservedCatalog(
                observedChestIds,
                observedQuestItemIds
            );

            StringBuilder output =
                new StringBuilder();

            output.AppendLine("CATALOG COVERAGE REFERENCE");
            output.AppendLine();
            output.AppendLine(
                "Unobserved means the scanner has not seen the reference asset yet."
            );
            output.AppendLine(
                "It does NOT automatically mean the asset is a missing randomizer location."
            );
            output.AppendLine();

            WriteSection(
                output,
                "CHEST IDS",
                chestReferences,
                observedChestIds
            );

            output.AppendLine();

            WriteSection(
                output,
                "QUEST ITEMS",
                questItemReferences,
                observedQuestItemIds
            );

            File.WriteAllText(
                CoveragePath,
                output.ToString()
            );
        }

        private static List<ReferenceEntry> LoadReferenceFile(
            string path)
        {
            List<ReferenceEntry> entries =
                new List<ReferenceEntry>();

            if (!File.Exists(path))
            {
                Plugin.Log.LogWarning(
                    $"Coverage reference file not found: {path}"
                );

                return entries;
            }

            foreach (string rawLine in File.ReadAllLines(path))
            {
                string line = rawLine.Trim();

                if (line.Length == 0 ||
                    line.StartsWith("#"))
                {
                    continue;
                }

                int separator = line.IndexOf('|');

                if (separator <= 0 ||
                    separator >= line.Length - 1)
                {
                    continue;
                }

                string guid =
                    line.Substring(0, separator).Trim();

                string name =
                    line.Substring(separator + 1).Trim();

                entries.Add(
                    new ReferenceEntry(
                        guid,
                        name
                    )
                );
            }

            entries.Sort(
                (a, b) =>
                    string.Compare(
                        a.Name,
                        b.Name,
                        StringComparison.OrdinalIgnoreCase
                    )
            );

            return entries;
        }

        private static void ReadObservedCatalog(
            HashSet<string> observedChestIds,
            HashSet<string> observedQuestItemIds)
        {
            foreach (string rawLine in File.ReadAllLines(CatalogPath))
            {
                string line = rawLine.Trim();

                if (line.StartsWith(
                    "LOCATION|",
                    StringComparison.Ordinal))
                {
                    string key =
                        line.Substring(
                            "LOCATION|".Length
                        );

                    if (LooksLikeGuid(key))
                    {
                        observedChestIds.Add(key);
                    }

                    continue;
                }

                if (line.StartsWith(
                    "OPTION|QuestItem|",
                    StringComparison.Ordinal))
                {
                    string[] parts =
                        line.Split('|');

                    if (parts.Length >= 3)
                    {
                        observedQuestItemIds.Add(
                            parts[2]
                        );
                    }
                }
            }
        }

        private static bool LooksLikeGuid(string value)
        {
            if (value == null ||
                value.Length != 32)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                bool isHex =
                    (c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F');

                if (!isHex)
                {
                    return false;
                }
            }

            return true;
        }

        private static void WriteSection(
            StringBuilder output,
            string title,
            List<ReferenceEntry> references,
            HashSet<string> observed)
        {
            int found = 0;

            foreach (ReferenceEntry entry in references)
            {
                if (observed.Contains(entry.Guid))
                {
                    found++;
                }
            }

            int unobserved =
                references.Count - found;

            output.AppendLine(title);
            output.AppendLine(
                $"Reference: {references.Count}"
            );
            output.AppendLine(
                $"Observed: {found}"
            );
            output.AppendLine(
                $"Unobserved: {unobserved}"
            );

            if (unobserved == 0)
            {
                return;
            }

            output.AppendLine();
            output.AppendLine("UNOBSERVED:");

            foreach (ReferenceEntry entry in references)
            {
                if (observed.Contains(entry.Guid))
                {
                    continue;
                }

                output.AppendLine(
                    $"{entry.Name}|{entry.Guid}"
                );
            }
        }
    }
}
