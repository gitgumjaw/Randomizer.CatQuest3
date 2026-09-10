using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class CatalogDiagnostics
    {
        public static void LogWeightedEquipmentSlots()
        {
            List<CatalogRewardLocation> locations =
                CatalogScanResults.GetAll();

            int weightedEquipmentSlotCount = 0;

            foreach (CatalogRewardLocation location in locations)
            {
                for (int i = 0; i < location.RewardSlots.Count; i++)
                {
                    WeightedRewardSlot slot =
                        location.RewardSlots[i];

                    int equipmentOptionCount = 0;

                    foreach (WeightedRewardOption option in slot.Options)
                    {
                        if (option?.Reward == null)
                        {
                            continue;
                        }

                        if (option.Reward.Type == RewardType.Equipment)
                        {
                            equipmentOptionCount++;
                        }
                    }

                    if (equipmentOptionCount <= 1)
                    {
                        continue;
                    }

                    weightedEquipmentSlotCount++;

                    Plugin.Log.LogInfo(
                        $"Weighted Equipment Slot | " +
                        $"Location: {location.Key} | " +
                        $"Slot: {i} | " +
                        $"Equipment Options: {equipmentOptionCount}"
                    );
                }
            }

            Plugin.Log.LogInfo(
                $"TOTAL WEIGHTED EQUIPMENT SLOTS: " +
                $"{weightedEquipmentSlotCount}"
            );
        }
    }
}