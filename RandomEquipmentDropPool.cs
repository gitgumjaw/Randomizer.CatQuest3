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
            equipment =
                new List<EquipmentItemData>();

            var database =
                AddressableSingletonScriptableObject<EquipmentDatabase>
                    .Instance;

            if (database?.contentTable == null)
            {
                Plugin.Log.LogError(
                    "Could not build random equipment drop pool: " +
                    "EquipmentDatabase was unavailable."
                );

                return;
            }

            foreach (var entry in database.contentTable)
            {
                EquipmentItemData item =
                    entry.Value;

                if (item == null)
                {
                    continue;
                }

                // Blueprints are not random enemy equipment drops.
                if (item is ShipBlueprintItemData)
                {
                    continue;
                }

                equipment.Add(item);
            }

            Plugin.Log.LogInfo(
                $"Built random equipment drop pool: " +
                $"{equipment.Count} items."
            );
        }

        public static EquipmentItemData GetRandom()
        {
            if (equipment == null)
            {
                BuildPool();
            }

            if (equipment == null ||
                equipment.Count == 0)
            {
                return null;
            }

            int index =
                Random.Range(
                    0,
                    equipment.Count
                );

            return equipment[index];
        }
    }
}