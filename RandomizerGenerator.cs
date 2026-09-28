using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class RandomizerGenerator
    {
        private const int MaxLogicAttempts =
            1000;


        public static void Generate(
            RandomizerSettings settings,
            int seed)
        {
            for (
                int attempt = 0;
                attempt < MaxLogicAttempts;
                attempt++)
            {
                int attemptSeed =
                    unchecked(
                        seed + attempt
                    );


                RewardCatalog.Clear();


                CatalogResolver.ResolveToRewardCatalog(
                    attemptSeed
                );


                List<RewardSlot> eligibleSlots =
                    RewardCatalog.GetEligibleSlots(
                        settings
                    );


                RewardShuffler.Shuffle(
                    eligibleSlots,
                    attemptSeed
                );


                bool logicValid =
                    LogicValidator.IsCurrentLayoutValid();


                if (!logicValid)
                {
                    Plugin.Log.LogInfo(
                        $"LOGIC VALIDATION | FAIL | " +
                        $"Attempt:{attempt + 1} | " +
                        $"Attempt Seed:{attemptSeed}"
                    );


                    LogicValidator
                        .LogCurrentLayoutFailureDiagnostics();


                    continue;
                }


                Plugin.Log.LogInfo(
                    $"LOGIC VALIDATION | PASS | " +
                    $"Attempt:{attempt + 1} | " +
                    $"Attempt Seed:{attemptSeed}"
                );


                Plugin.Log.LogInfo(
                    $"Generated randomizer with " +
                    $"{eligibleSlots.Count} eligible reward slots | " +
                    $"Seed:{seed} | " +
                    $"Attempts:{attempt + 1}"
                );


                return;
            }


            Plugin.Log.LogError(
                $"LOGIC VALIDATION | " +
                $"Could not generate a beatable layout " +
                $"after {MaxLogicAttempts} attempts | " +
                $"Seed:{seed}"
            );


            throw new InvalidOperationException(
                $"Could not generate a beatable randomizer " +
                $"layout after {MaxLogicAttempts} attempts."
            );
        }
    }
}