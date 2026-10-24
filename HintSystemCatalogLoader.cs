
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Randomizer.CatQuest3
{
    public static class HintSystemCatalogLoader
    {
        public static List<HintSystemSource> Load()
        {
            string pluginDirectory =
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location
                );

            string catalogPath =
                Path.Combine(
                    pluginDirectory,
                    "Data",
                    "HintSystemCatalog.txt"
                );

            List<HintSystemSource> sources =
                new List<HintSystemSource>();

            if (!File.Exists(catalogPath))
            {
                Plugin.Log.LogError(
                    "Hint source catalog not found: " +
                    catalogPath);

                return sources;
            }

            HintSystemSource currentSource = null;

            foreach (string rawLine in
                File.ReadAllLines(catalogPath))
            {
                string line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("#"))
                    continue;

                if (line.StartsWith(
                    "Catalog Sources:",
                    StringComparison.Ordinal))
                    continue;

                if (line.StartsWith(
                    "LOCATION|",
                    StringComparison.Ordinal))
                {
                    string key =
                        line.Substring("LOCATION|".Length);

                    if (string.IsNullOrWhiteSpace(key))
                    {
                        Plugin.Log.LogWarning(
                            "HINT SYSTEM | Empty source key.");

                        currentSource = null;
                        continue;
                    }

                    currentSource =
                        new HintSystemSource(key);

                    sources.Add(currentSource);
                    continue;
                }

                if (line.StartsWith(
                    "LABEL|",
                    StringComparison.Ordinal))
                {
                    if (currentSource == null)
                        continue;

                    currentSource.Label =
                        line.Substring("LABEL|".Length);

                    continue;
                }

                if (line.StartsWith(
                    "TRIGGER|",
                    StringComparison.Ordinal))
                {
                    if (currentSource == null)
                        continue;

                    string trigger =
                        line.Substring("TRIGGER|".Length);

                    // Expected:
                    // Dialogue|PlayMaker State Name
                    // Message|PlayMaker State Name

                    int separator = trigger.IndexOf('|');

                    if (separator <= 0 ||
                        separator == trigger.Length - 1)
                    {
                        Plugin.Log.LogWarning(
                            "HINT SYSTEM | Invalid trigger: " +
                            trigger);

                        continue;
                    }

                    string type =
                        trigger.Substring(0, separator);

                    if (type != "Dialogue" &&
                        type != "Message")
                    {
                        Plugin.Log.LogWarning(
                            "HINT SYSTEM | Unknown trigger type: " +
                            type);

                        continue;
                    }

                    currentSource.AddTrigger(trigger);
                }
            }

            Plugin.Log.LogInfo(
                "HINT SYSTEM | Loaded source catalog: " +
                sources.Count + " sources.");

            return sources;
        }
    }
}
