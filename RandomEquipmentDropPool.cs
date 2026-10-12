using System.Collections.Generic;
using ProjectStar.Data;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    public static class RandomEquipmentDropPool
    {
        private static List<EquipmentItemData>
            equipment;


        private static void BuildPool()
        {
            equipment =
                new List<EquipmentItemData>();

            var database =
                AddressableSingletonScriptableObject<
                    EquipmentDatabase
                >
                    .Instance;

            if (
                database == null ||
                database.contentTable == null
            )
            {
                Plugin.Log.LogError(
                    "Could not build random " +
                    "equipment drop pool: " +
                    "EquipmentDatabase was unavailable."
                );

                return;
            }

            IReadOnlyList<string>
                allowedEquipmentIds =
                    RewardNameResolver.GetIds(
                        RewardType.Equipment
                    );

            foreach (
                string equipmentId
                in allowedEquipmentIds)
            {
                if (string.IsNullOrWhiteSpace(
                        equipmentId))
                {
                    continue;
                }

                if (!database.contentTable
                    .TryGetValue(
                        equipmentId,
                        out EquipmentItemData item
                    ))
                {
                    Plugin.Log.LogWarning(
                        "Random equipment drop " +
                        "pool item not found in " +
                        "EquipmentDatabase | Guid:" +
                        equipmentId
                    );

                    continue;
                }

                if (item == null)
                {
                    Plugin.Log.LogWarning(
                        "Random equipment drop " +
                        "pool item resolved to NULL | Guid:" +
                        equipmentId
                    );

                    continue;
                }

                if (item is ShipBlueprintItemData)
                {
                    Plugin.Log.LogWarning(
                        "Random equipment drop " +
                        "pool whitelist resolved to " +
                        "ShipBlueprintItemData | Guid:" +
                        equipmentId
                    );

                    continue;
                }

                equipment.Add(
                    item
                );
            }

            Plugin.Log.LogInfo(
                "Built random equipment drop pool " +
                "from RewardNames whitelist: " +
                equipment.Count +
                " items."
            );
        }


        public static EquipmentItemData GetRandom()
        {
            if (equipment == null)
            {
                BuildPool();
            }

            if (
                equipment == null ||
                equipment.Count == 0
            )
            {
                return null;
            }

            int index =
                UnityEngine.Random.Range(
                    0,
                    equipment.Count
                );

            return equipment[
                index
            ];
        }
    }
}