
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class HintSystemCatalog
    {
        private static readonly Dictionary
            <string, HintSystemSource> sources =
            new Dictionary
                <string, HintSystemSource>(
                    StringComparer.Ordinal);

        private static readonly Dictionary
            <string, HintSystemSource> triggerSources =
            new Dictionary
                <string, HintSystemSource>(
                    StringComparer.Ordinal);

        private static bool loaded;

        private static void EnsureLoaded()
        {
            if (loaded)
                return;

            loaded = true;

            AddRange(
                HintSystemCatalogLoader.Load());
        }

        public static void Add(HintSystemSource source)
        {
            if (source == null ||
                string.IsNullOrWhiteSpace(source.Key))
                return;

            if (sources.ContainsKey(source.Key))
            {
                Plugin.Log.LogError(
                    "HINT SYSTEM | Duplicate source: " +
                    source.Key);

                return;
            }

            sources.Add(source.Key, source);

            foreach (string trigger in source.Triggers)
            {
                string triggerKey =
                    source.Key + "\u001F" + trigger;

                if (triggerSources.ContainsKey(triggerKey))
                {
                    Plugin.Log.LogError(
                        "HINT SYSTEM | Duplicate trigger: " +
                        triggerKey);

                    continue;
                }

                triggerSources.Add(triggerKey, source);
            }
        }

        public static void AddRange(
            IEnumerable<HintSystemSource> newSources)
        {
            foreach (HintSystemSource source in newSources)
                Add(source);
        }

        public static HintSystemSource Get(string key)
        {
            EnsureLoaded();

            HintSystemSource source;

            if (sources.TryGetValue(key, out source))
                return source;

            return null;
        }

        public static HintSystemSource Get(
            string key,
            string interactionType,
            string state)
        {
            EnsureLoaded();

            if (string.IsNullOrEmpty(key) ||
                string.IsNullOrEmpty(interactionType) ||
                string.IsNullOrEmpty(state))
                return null;

            string triggerKey =
                key + "\u001F" +
                interactionType + "|" + state;

            HintSystemSource source;

            if (triggerSources.TryGetValue(
                triggerKey,
                out source))
                return source;

            return null;
        }

        public static List<HintSystemSource> GetAll()
        {
            EnsureLoaded();

            return new List<HintSystemSource>(
                sources.Values);
        }

        public static int Count
        {
            get
            {
                EnsureLoaded();
                return sources.Count;
            }
        }

        public static void Clear()
        {
            sources.Clear();
            triggerSources.Clear();
            loaded = false;
        }
    }
}
