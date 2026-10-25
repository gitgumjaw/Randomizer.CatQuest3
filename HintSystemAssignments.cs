
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class HintSystemAssignments
    {
        private static readonly Dictionary<string, string>
            assignedHints =
                new Dictionary<string, string>(
                    StringComparer.Ordinal);

        public static string GetText(string sourceKey)
        {
            if (string.IsNullOrEmpty(sourceKey))
                return null;

            assignedHints.TryGetValue(
                sourceKey,
                out string text);

            return text;
        }

        public static void Clear()
        {
            assignedHints.Clear();
        }

        public static void Generate(
            List<RewardSlot> shuffledSlots,
            RandomizerSettings settings,
            int seed)
        {
            Clear();

            List<HintSystemSource> sources =
                HintSystemCatalog.GetAll();

            sources.Sort((a, b) =>
                StringComparer.Ordinal.Compare(
                    a.Key, b.Key));

            List<RewardSlot> spoilers =
                HintSystemSpoilers.GetShuffledSlots(
                    shuffledSlots, seed);

            List<HintSystemAreaClue> clues =
                HintSystemAreaClues.GetShuffledAreas(
                    shuffledSlots, seed);

            Random random = new Random(
                unchecked(seed ^ 0x6B29C4F1));

            int spoilerIndex = 0;
            int clueIndex = 0;
            int spoilerCount = 0;
            int clueCount = 0;

            bool allowClues =
                settings != null &&
                settings.RandomizeQuestItems;

            foreach (HintSystemSource source in sources)
            {
                bool useClue =
                    allowClues &&
                    random.Next(100) < 30;

                // Never repeat an area. If the clue
                // pool is exhausted, use a Spoiler.
                if (clueIndex >= clues.Count)
                    useClue = false;

                string text;

                if (useClue)
                {
                    HintSystemAreaClue clue =
                        clues[clueIndex++];

                    string itemWord =
                        clue.ImportantItems == 1
                            ? "item"
                            : "items";

                    text =
                        "<b>You found a</b>\n" +
                        "<b>Clue!</b>\n\n" +
                        clue.ImportantItems +
                        " important " + itemWord + "\n" +
                        "in\n" +
                        clue.Area;

                    clueCount++;

                    Plugin.Log.LogInfo(
                        "HINT ASSIGNMENT | CLUE | " +
                        source.Label +
                        " | " + clue.Area +
                        " | Important: " +
                        clue.ImportantItems);
                }
                else
                {
                    if (spoilers.Count == 0)
                    {
                        Plugin.Log.LogWarning(
                            "HINT SYSTEM | No eligible " +
                            "Spoilers for " + source.Key);
                        continue;
                    }

                    // Reuse targets only after the
                    // complete pool has been consumed.
                    RewardSlot slot =
                        spoilers[
                            spoilerIndex % spoilers.Count];

                    spoilerIndex++;

                    string area =
                        slot.Location.Area ?? "";

                    if (area.Contains(";"))
                    {
                        area = area.Split(';')[0];
                    }

                    area = area.Trim();

                    text =
                        "<b>You found a</b>\n" +
                        "<b>Spoiler!</b>\n\n" +
                        SpoilerLogWriter.FormatReward(
                            slot.RandomizedReward) + "\n" +
                        "is in\n" +
                        area;

                    spoilerCount++;

                    Plugin.Log.LogInfo(
                        "HINT ASSIGNMENT | SPOILER | " +
                        source.Label +
                        " | " +
                        SpoilerLogWriter.FormatReward(
                            slot.RandomizedReward) +
                        " | " + area);
                }

                assignedHints[source.Key] = text;
            }

            Plugin.Log.LogInfo(
                "HINT ASSIGNMENT | " +
                "Sources: " + sources.Count +
                " | Spoilers: " + spoilerCount +
                " | Clues: " + clueCount +
                " | Assigned: " + assignedHints.Count);
        }
    }
}
