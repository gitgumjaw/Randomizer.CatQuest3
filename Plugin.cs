using BepInEx;
using HarmonyLib;
using ProjectStar.Data;

namespace Randomizer.CatQuest3
{
    [BepInPlugin("Randomizer.CatQuest3", "Cat Quest 3 Randomizer", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource Log;
        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo("Hello from Cat Quest 3 Randomizer!");

            Harmony harmony = new Harmony("Randomizer.CatQuest3");
            harmony.PatchAll();

            Logger.LogInfo("Harmony patches applied.");
        }
    }

    //========================================
    // RANDOM LOOT ITEM LOGGER
    //========================================

    [HarmonyPatch(typeof(ChestBehaviour), "RandomLootItem")]
    public static class ChestBehaviourRandomLootItemPatch
    {
        public static void Prefix()
        {
            Plugin.Log.LogInfo("RandomLootItem was called.");
        }
    }

    //========================================
    // SPAWN ITEM LOOT PATCH
    //========================================

    [HarmonyPatch(typeof(ChestBehaviour), "SpawnItemLoot")]
    public static class ChestBehaviourSpawnItemLootPatch
    {
        private static readonly System.Random random = new System.Random(12345);

        public static void Postfix(
            ref EquipmentItemData __result,
            EquipmentLootTableItem[] ___itemsDrops)
        {
            var validEntries = new System.Collections.Generic.List<EquipmentLootTableItem>();

            foreach (var entry in ___itemsDrops)
            {
                if (entry.item != null)
                {
                    validEntries.Add(entry);
                }
            }

            if (validEntries.Count == 0)
            {
                return;
            }

            int index = random.Next(validEntries.Count);

            __result = validEntries[index].item;

            Plugin.Log.LogInfo(
                $"SpawnItemLoot randomized to: {__result.itemName}"
            );
        }
    }
}