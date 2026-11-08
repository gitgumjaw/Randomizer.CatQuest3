using System;
using System.Collections.Generic;
using ProjectStar.Data;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class RandomEquipmentDropPool
    {
        private static List<EquipmentItemData> equipment;

        private static void BuildPool()
        {
            equipment = new List<EquipmentItemData>();

            var database = AddressableSingletonScriptableObject<EquipmentDatabase>.Instance;
            if (database == null || database.contentTable == null)
            {
                Plugin.Log.LogError(
                    "Could not build random equipment drop pool: EquipmentDatabase was unavailable.");
                return;
            }

            // Mew Game equipment belongs only in the fixed supplemental
            // reward injection, not in repeatable random equipment draws.
            HashSet<string> excludedIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (Reward reward in MewGameRewards.GetAll())
            {
                if (reward != null && !string.IsNullOrEmpty(reward.Id))
                    excludedIds.Add(reward.Id);
            }

            IReadOnlyList<string> allowedEquipmentIds =
                RewardNameResolver.GetIds(RewardType.Equipment);

            foreach (string equipmentId in allowedEquipmentIds)
            {
                if (string.IsNullOrWhiteSpace(equipmentId) ||
                    excludedIds.Contains(equipmentId))
                    continue;

                EquipmentItemData item;
                if (!database.contentTable.TryGetValue(equipmentId, out item))
                {
                    Plugin.Log.LogWarning(
                        "Random equipment drop pool item not found in EquipmentDatabase | Guid:" +
                        equipmentId);
                    continue;
                }
                if (item == null)
                {
                    Plugin.Log.LogWarning(
                        "Random equipment drop pool item resolved to NULL | Guid:" +
                        equipmentId);
                    continue;
                }
                if (item is ShipBlueprintItemData)
                {
                    Plugin.Log.LogWarning(
                        "Random equipment drop pool whitelist resolved to ShipBlueprintItemData | Guid:" +
                        equipmentId);
                    continue;
                }
                equipment.Add(item);
            }

            Plugin.Log.LogInfo(
                "Built random equipment drop pool from RewardNames whitelist: " +
                equipment.Count + " items (Mew Game equipment excluded).");
        }

        // Used by repeatable bag drops: keep their existing Unity RNG behavior.
        public static EquipmentItemData GetRandom()
        {
            if (equipment == null) BuildPool();
            if (equipment == null || equipment.Count == 0) return null;
            return equipment[UnityEngine.Random.Range(0, equipment.Count)];
        }

        // Used only during seeded reward generation. This exposes the SAME
        // validated pool as bag drops, without using Unity's global RNG.
        public static List<Reward> GetShuffleCandidateRewards()
        {
            if (equipment == null) BuildPool();
            List<Reward> result = new List<Reward>();
            if (equipment == null) return result;

            foreach (EquipmentItemData item in equipment)
            {
                if (item != null && !string.IsNullOrEmpty(item.Guid))
                    result.Add(new Reward(RewardType.Equipment, item.Guid));
            }

            // Keep seed results independent of RewardNames.txt ordering.
            result.Sort((a, b) => StringComparer.Ordinal.Compare(a.Id, b.Id));
            return result;
        }
    }
}
