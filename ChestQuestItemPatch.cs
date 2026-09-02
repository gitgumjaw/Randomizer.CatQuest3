using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(ChestBehaviour), "DropQuestItem")]
    public static class ChestQuestItemPatch
    {
        public static void Prefix(
            ChestID ___chestID,
            LootTable ___currentTable)
        {
            if (___chestID == null || ___currentTable?.list == null)
            {
                return;
            }

            foreach (LootTableItem entry in ___currentTable.list)
            {
                if (entry.dropType != LootTableItem.DropType.QuestItem ||
                    entry.dropQuestItem == null)
                {
                    continue;
                }

                RewardLocation location = new RewardLocation(
                    ___chestID.Guid,
                    new Reward(
                        RewardType.QuestItem,
                        entry.dropQuestItem.Guid
                    )
                );

                RewardRegistry.Register(location);
            }
        }
    }
}