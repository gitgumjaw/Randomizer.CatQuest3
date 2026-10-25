
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public class HintSystemAreaClue
    {
        public string Area { get; }
        public int ShuffledSlots { get; }
        public int ImportantItems { get; }

        public HintSystemAreaClue(
            string area,
            int shuffledSlots,
            int importantItems)
        {
            Area = area;
            ShuffledSlots = shuffledSlots;
            ImportantItems = importantItems;
        }

        public string GetText()
        {
            return "There are " +
                ImportantItems +
                " important items in " +
                Area + ".";
        }
    }


    public static class HintSystemAreaClues
    {
        private const int MinimumShuffledSlots = 3;

        private static readonly HashSet<string>
            ExcludedAreas = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                "Catuga",
                "Zero Dimension",
                "Code Isle",
                "Code Island",
                "Sandy Isle",
                "Sandy Islands",
                "Starting Loadout"
            };


        public static List<HintSystemAreaClue> GetEligibleAreas(
            List<RewardSlot> shuffledSlots)
        {
            List<HintSystemAreaClue> result =
                new List<HintSystemAreaClue>();

            if (shuffledSlots == null)
            {
                return result;
            }

            List<RewardSlot> requiredSlots =
                LogicValidator.GetRequiredQuestItemSlots();

            Dictionary<RewardLocation, HashSet<int>>
                requiredByLocation =
                    new Dictionary<RewardLocation, HashSet<int>>();

            foreach (RewardSlot slot in requiredSlots)
            {
                if (!requiredByLocation.TryGetValue(
                    slot.Location,
                    out HashSet<int> indices))
                {
                    indices = new HashSet<int>();
                    requiredByLocation.Add(
                        slot.Location,
                        indices
                    );
                }

                indices.Add(slot.RewardIndex);
            }


            Dictionary<string, int> slotCounts =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            Dictionary<string, int> importantCounts =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);


            foreach (RewardSlot slot in shuffledSlots)
            {
                if (slot == null ||
                    slot.Location == null ||
                    string.IsNullOrWhiteSpace(
                        slot.Location.Area))
                {
                    continue;
                }

                HashSet<string> tags =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase);

                foreach (string rawTag in
                    slot.Location.Area.Split(';'))
                {
                    string tag = rawTag.Trim();

                    if (!string.IsNullOrEmpty(tag))
                    {
                        tags.Add(tag);
                    }
                }

                bool important =
                    requiredByLocation.TryGetValue(
                        slot.Location,
                        out HashSet<int> requiredIndices) &&
                    requiredIndices.Contains(
                        slot.RewardIndex);


                foreach (string tag in tags)
                {
                    if (!slotCounts.ContainsKey(tag))
                    {
                        slotCounts[tag] = 0;
                        importantCounts[tag] = 0;
                    }

                    slotCounts[tag]++;

                    if (important)
                    {
                        importantCounts[tag]++;
                    }
                }
            }


            foreach (string area in slotCounts.Keys)
            {
                if (ExcludedAreas.Contains(area) ||
                    slotCounts[area] < MinimumShuffledSlots)
                {
                    continue;
                }

                result.Add(
                    new HintSystemAreaClue(
                        area,
                        slotCounts[area],
                        importantCounts[area]
                    )
                );
            }

            result.Sort(
                (a, b) =>
                    StringComparer.OrdinalIgnoreCase.Compare(
                        a.Area,
                        b.Area
                    )
            );

            return result;
        }


        public static List<HintSystemAreaClue> GetShuffledAreas(
            List<RewardSlot> shuffledSlots,
            int seed)
        {
            List<HintSystemAreaClue> areas =
                GetEligibleAreas(shuffledSlots);

            // Separate from the reward shuffler's random sequence.
            Random random = new Random(
                unchecked(seed ^ 0x4A37B91D)
            );

            // Fisher-Yates shuffle.
            for (int i = areas.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                HintSystemAreaClue temporary = areas[i];
                areas[i] = areas[j];
                areas[j] = temporary;
            }

            return areas;
        }


        public static void LogAreaCounts(
            List<RewardSlot> shuffledSlots)
        {
            List<HintSystemAreaClue> areas =
                GetEligibleAreas(shuffledSlots);

            foreach (HintSystemAreaClue area in areas)
            {
                Plugin.Log.LogInfo(
                    "HINT AREA | " +
                    area.Area +
                    " | Shuffled Slots: " +
                    area.ShuffledSlots +
                    " | Important: " +
                    area.ImportantItems
                );
            }

            Plugin.Log.LogInfo(
                "HINT AREA | Eligible Areas: " +
                areas.Count
            );
        }


        public static void LogAreaSelection(
            List<RewardSlot> shuffledSlots,
            int seed)
        {
            List<HintSystemAreaClue> areas =
                GetShuffledAreas(shuffledSlots, seed);

            int count = Math.Min(12, areas.Count);

            for (int i = 0; i < count; i++)
            {
                Plugin.Log.LogInfo(
                    "HINT AREA SELECTION | " +
                    (i + 1) +
                    " | " +
                    areas[i].Area +
                    " | Important: " +
                    areas[i].ImportantItems
                );
            }

            Plugin.Log.LogInfo(
                "HINT AREA SELECTION | " +
                "Available unique areas: " +
                areas.Count
            );
        }

    }
}
