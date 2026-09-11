using ProjectStar.Data;

namespace Randomizer.CatQuest3
{
    public static class RewardDataResolver
    {
        public static EquipmentItemData GetEquipment(
            string guid)
        {
            var database =
                AddressableSingletonScriptableObject<EquipmentDatabase>
                    .Instance;

            if (database?.contentTable == null)
            {
                Plugin.Log.LogError(
                    "EquipmentDatabase was unavailable."
                );

                return null;
            }

            foreach (var entry in database.contentTable)
            {
                EquipmentItemData item =
                    entry.Value;

                if (item == null)
                {
                    continue;
                }

                if (item.Guid == guid)
                {
                    return item;
                }
            }

            Plugin.Log.LogError(
                $"Could not resolve equipment GUID: {guid}"
            );

            return null;
        }

        public static ShipBlueprintItemData GetBlueprint(
            string guid)
        {
            var database =
                AddressableSingletonScriptableObject<ShipBlueprintDatabase>
                    .Instance;

            if (database == null)
            {
                Plugin.Log.LogError(
                    "ShipBlueprintDatabase was unavailable."
                );

                return null;
            }

            ShipBlueprintItemData blueprint =
                database.GetEntry(guid);

            if (blueprint != null)
            {
                return blueprint;
            }

            Plugin.Log.LogError(
                $"Could not resolve blueprint GUID: {guid}"
            );

            return null;
        }

        public static SpellConfig GetSpell(
            string guid)
        {
            var database =
                AddressableSingletonScriptableObject<SpellConfigDatabase>
                    .Instance;

            if (database == null)
            {
                Plugin.Log.LogError(
                    "SpellConfigDatabase was unavailable."
                );

                return null;
            }

            SpellConfigBase spell =
                database.GetEntry(guid);

            if (spell is SpellConfig playerSpell)
            {
                return playerSpell;
            }

            Plugin.Log.LogError(
                $"Could not resolve player spell GUID: {guid}"
            );

            return null;
        }

        public static ShipSpellConfig GetShipSpell(
            string guid)
        {
            var database =
                AddressableSingletonScriptableObject<SpellConfigDatabase>
                    .Instance;

            if (database == null)
            {
                Plugin.Log.LogError(
                    "SpellConfigDatabase was unavailable."
                );

                return null;
            }

            SpellConfigBase spell =
                database.GetEntry(guid);

            if (spell is ShipSpellConfig shipSpell)
            {
                return shipSpell;
            }

            Plugin.Log.LogError(
                $"Could not resolve ship spell GUID: {guid}"
            );

            return null;
        }

        public static QuestItem GetQuestItem(
            string guid)
        {
            var database =
                AddressableSingletonScriptableObject<QuestItemDatabase>
                    .Instance;

            if (database == null)
            {
                Plugin.Log.LogError(
                    "QuestItemDatabase was unavailable."
                );

                return null;
            }

            QuestItem questItem =
                database.GetEntry(guid);

            if (questItem != null)
            {
                return questItem;
            }

            Plugin.Log.LogError(
                $"Could not resolve quest item GUID: {guid}"
            );

            return null;
        }
    }
}