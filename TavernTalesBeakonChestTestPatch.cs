using System;
using HarmonyLib;

namespace Randomizer.CatQuest3
{
    // TEMPORARY TEST ONLY: place three copies of the new permanent Companion
    // reward type into the Catuga chest queue, without changing the seed layout.
    // Remove this file when the real companion locations are integrated.
    [HarmonyPatch]
    public static class TavernTalesBeakonChestTestPatch
    {
        private const string NorthCrate = "0294cd0074d1d204db0bac9eab555e55";
        private const string WestChest = "7790326666aad054e8775e3759d4a28a";
        private const string EastCrate = "b3c5d4e55fb2a914b8a547680be68a78";
        private const string Beakon = "5a855371377284baa94f29916ca93f61";

        [HarmonyPatch(typeof(RewardGrantQueue), "Enqueue")]
        [HarmonyPrefix]
        private static void SubstituteCompanion(RewardLocation location, int slotIndex, ref Reward reward)
        {
            RandomizerRunConfiguration config = RandomizerRuntime.CurrentConfiguration;
            if (config == null || !config.Enabled || !config.TavernTalesParrots ||
                location == null || slotIndex != 0) return;

            if (!string.Equals(location.Key, NorthCrate, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(location.Key, WestChest, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(location.Key, EastCrate, StringComparison.OrdinalIgnoreCase)) return;

            Plugin.Log.LogInfo("COMPANION PIPELINE TEST | Location:" + location.Label +
                " | Original:" + (reward == null ? "NULL" : reward.Type + ":" + reward.Id) +
                " | Replacement:Companion:" + Beakon);

            reward = new Reward(RewardType.Companion, Beakon);
        }
    }
}
