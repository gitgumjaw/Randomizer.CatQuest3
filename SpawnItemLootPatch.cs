using HarmonyLib;
using ProjectStar.Data;
using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(ChestBehaviour), "SpawnItemLoot")]
    public static class SpawnItemLootPatch
    {
        private const int Seed = 12345;

        private static int StableHash(string text)
        {
            unchecked
            {
                uint hash = 2166136261;

                foreach (char c in text)
                {
                    hash ^= c;
                    hash *= 16777619;
                }

                return (int)hash;
            }
        }

        public static void Postfix(
            ref EquipmentItemData __result,
            EquipmentLootTableItem[] ___itemsDrops,
            ChestID ___chestID)
        {
            if (___chestID != null)
            {
                Plugin.Log.LogInfo($"Chest GUID: {___chestID.Guid}");
            }

            List<EquipmentLootTableItem> validEntries =
                new List<EquipmentLootTableItem>();

            foreach (EquipmentLootTableItem entry in ___itemsDrops)
            {
                if (entry.item != null)
                {
                    validEntries.Add(entry);
                }
            }

            if (validEntries.Count == 0 || ___chestID == null)
            {
                return;
            }

            int chestSeed = Seed ^ StableHash(___chestID.Guid);

            Random random = new Random(chestSeed);

            int index = random.Next(validEntries.Count);

            __result = validEntries[index].item;

            Plugin.Log.LogInfo(
                $"SpawnItemLoot randomized to: {__result.itemName}"
            );
        }
    }
}