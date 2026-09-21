namespace Randomizer.CatQuest3
{
    public static class RandomizerRuntime
    {
        public static RandomizerRunConfiguration CurrentConfiguration
        {
            get;
            private set;
        } = CreateVanillaConfiguration();


        public static int CurrentProfileIndex
        {
            get;
            private set;
        } = -1;


        public static bool IsEnabled
        {
            get
            {
                return CurrentConfiguration != null &&
                    CurrentConfiguration.Enabled;
            }
        }


        public static void Activate(
            int profileIndex,
            RandomizerRunConfiguration configuration)
        {
            CurrentProfileIndex =
                profileIndex;


            CurrentConfiguration =
                configuration ??
                CreateVanillaConfiguration();


            ApplyCurrentConfiguration();


            Plugin.Log.LogInfo(
                "RANDOMIZER RUNTIME | " +
                "Activated profile index:" +
                CurrentProfileIndex +
                " | Enabled:" +
                CurrentConfiguration.Enabled +
                " | Seed:" +
                CurrentConfiguration.SeedText +
                " | Hash:" +
                CurrentConfiguration.SeedValue
            );
        }


        public static void ActivateVanilla(
            int profileIndex)
        {
            CurrentProfileIndex =
                profileIndex;

            CurrentConfiguration =
                CreateVanillaConfiguration();


            ApplyCurrentConfiguration();


            Plugin.Log.LogInfo(
                "RANDOMIZER RUNTIME | " +
                "Activated vanilla configuration for profile index:" +
                CurrentProfileIndex
            );
        }


        private static void ApplyCurrentConfiguration()
        {
            RandomizerSettings settings =
                CreateSettings(
                    CurrentConfiguration
                );


            RandomizerState.Settings =
                settings;


            Plugin.Log.LogInfo(
                "RANDOMIZER SETTINGS | " +
                "Equipment:" +
                settings.RandomizeEquipment +
                " | Spells:" +
                settings.RandomizeSpells +
                " | QuestItems:" +
                settings.RandomizeQuestItems +
                " | ManaCrystals:" +
                settings.RandomizeManaCrystals +
                " | Collectibles:" +
                settings.RandomizeCollectibles +
                " | MatchEquipmentLevelToPlayer:" +
                settings.MatchEquipmentLevelToPlayer +
                " | ShipKey:" +
                settings.RandomizeShipKey +
                " | InfinityKey:" +
                settings.RandomizeInfinityKey +
                " | BirdPoop:" +
                settings.RandomizeBirdPoop
            );


            if (!CurrentConfiguration.Enabled)
            {
                RewardCatalog.Clear();

                Plugin.Log.LogInfo(
                    "RANDOMIZER RUNTIME | " +
                    "Randomizer disabled; reward catalog cleared."
                );

                return;
            }


            RandomizerGenerator.Generate(
                settings,
                CurrentConfiguration.SeedValue
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER RUNTIME | " +
                "Generated active randomizer state from saved configuration."
            );
        }


        private static RandomizerSettings CreateSettings(
            RandomizerRunConfiguration configuration)
        {
            bool enabled =
                configuration != null &&
                configuration.Enabled;


            return new RandomizerSettings
            {
                RandomizeEquipment =
                    enabled &&
                    configuration.Equipment,

                RandomizeSpells =
                    enabled &&
                    configuration.Spells,

                RandomizeQuestItems =
                    enabled &&
                    configuration.QuestItems,

                RandomizeManaCrystals =
                    enabled &&
                    configuration.ManaCrystals,

                RandomizeCollectibles =
                    enabled &&
                    configuration.ResourcesCollectibles,

                MatchEquipmentLevelToPlayer =
                    enabled &&
                    configuration.MatchPlayerLevel,

                RandomizeShipKey =
                    enabled &&
                    configuration.ShipKey,

                RandomizeInfinityKey =
                    enabled &&
                    configuration.InfinityKey,

                RandomizeBirdPoop =
                    enabled &&
                    configuration.BirdPoop
            };
        }


        private static RandomizerRunConfiguration
            CreateVanillaConfiguration()
        {
            return new RandomizerRunConfiguration(
                false, // Enabled
                false, // Equipment
                false, // Spells
                false, // QuestItems
                false, // ManaCrystals
                false, // ResourcesCollectibles
                false, // ShipKey
                false, // InfinityKey
                false, // BirdPoop
                false, // MatchPlayerLevel
                "",
                0
            );
        }
    }
}
