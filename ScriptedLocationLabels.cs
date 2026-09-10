using System;
using System.Collections.Generic;

namespace Randomizer.CatQuest3
{
    public static class ScriptedLocationLabels
    {
        private static readonly Dictionary<string, string> Labels =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Castle_FirePiratesHideout|DungeonQuest_FirePirateHideout|FSM|Learn Spell", "Defeat Fire Captain" },
                { "Castle_FirePiratesHideout|DungeonQuest_FirePirateHideout|FSM|Spirit gives wand", "Free Wand in Fire Hideout" },
                { "Castle_GoldenTower|DungeonQuest_GoldenTower|FSM|Get Key", "Golden Tower - LV40 Rat Reward" },
                { "Castle_IcePiratesHideout|DungeonQuest_IcePiratesHideout|FSM|Learn Spell", "Defeat Ice Captain" },
                { "Castle_LovepurrCastle|Interactable_NPC_Clawford|FSM|(Intro cont.) Clawford tells player to break curse", "First Meeting Clawford" },
                { "Castle_TwinCastle_01|DungeonQuest_TwinCastle_01|FSM|Get Boss Room Key", "Twin Castle (Light) - Master Bedroom" },
                { "Castle_TwinCastle_01|DungeonQuest_TwinCastle_01|FSM|Get Dining Hall Key", "Twin Castle (Light) - Jailed Key" },
                { "Castle_TwinCastle_02|DungeonQuest_TwinCastle_02|FSM|Earn the Necropawmicon", "Defeat Necromouser" },
                { "Castle_TwinCastle_02|DungeonQuest_TwinCastle_02|FSM|Get Main Entrance Key", "Twin Castle (Dark) - Near Entrance Key" },
                { "Castle_TwinCastle_02|DungeonQuest_TwinCastle_02|FSM|Get Master Room Key", "Twin Castle (Dark) - Jailed Key" },
                { "Cave_BootyCave|DungeonQuest_BootyCave|FSM|Boss Reward", "Defeat Patchy" },
                { "Cave_Volcano|DungeonQuest_MainQuest_SquidBoss|FSM|Award Equipment", "Defeat Takemeowki" },
                { "InfinityTower|DungeonQuest_InfinityTower|FSM|Receive the North Star essence", "Ascend Infinity Tower" },
                { "InfinityTower|InfinityTowerFiniteManager|FSM|Cleared Final Batch|Wave:8", "Infinity Tower - Wave 8" },
                { "InfinityTower|InfinityTowerFiniteManager|FSM|Cleared Ongoing Batch|Wave:3", "Infinity Tower - Wave 3" },
                { "InfinityTower|InfinityTowerFiniteManager|FSM|Cleared Ongoing Batch|Wave:5", "Infinity Tower - Wave 5" },
                { "Interior_KiddCatSmithy|Interactable_KiddCat|FSM|Get Reward", "Smithy - Reward for Dragonbone" },
                { "Interior_MageShop|Interactable_Mage|FSM|Learn P2 Spell", "P2 Spell at Magic Shop" },
                { "Interior_MageShop|Interactable_Mage|FSM|Learn Spell", "Mage Shop - Reward for Necropawmicon" },
                { "Interior_MageShop|Interactable_Mage|FSM|Mage Intro", "Freebie Spell at Magic Shop" },
                { "Interior_MageShop|InteriorQuest_LostItem_VoodooDoll|FSM|Learn Spell", "Mage Shop -Reward for Voodoo Dool" },
                { "Interior_MewtallicaStage|DungeonQuest_MewtallicaStage_BossFight|FSM|Get Reward", "Defeat Meowtallika" },
                { "Interior_PirateKingHideout|DungeonQuest_PirateKing|FSM|Earn Reward", "Defeat Pi-rat King" },
                { "Interior_Tavern|Interactable_MamaMilka|FSM|Gives Reward", "Tavern - Cathulhu kill reward" },
                { "Interior_ZeroDimension|SQ_Aelius_ZeroDimension_01|FSM|give reward", "Aelius Visit 1" },
                { "Interior_ZeroDimension|SQ_Aelius_ZeroDimension_02|FSM|give reward 2", "Aelius Visit 2" },
                { "Interior_ZeroDimension|SQ_Aelius_ZeroDimension_03|FSM|give reward 2", "Aelius Visit 3" },
                { "MainOverworld|Interactable_Sibling02_Rock06_Trigger|FSM|Message to say get key and get key", "Heirloom Quest Under Rock" },
                { "MainOverworld|MainQuest_01|FSM|Defeated Mr Clean", "Defeat Mr Clean 1" },
                { "MainOverworld|MainQuest_07_Key_03|FSM|Get Squid Key", "Defeat Meowkoyakuza" },
                { "MainOverworld|SQ_Cathulu|FSM|Fight Cathulhu", "Defeat Cathulhu" },
                { "MainOverworld|SQ_FearsofthePawst_01|FSM|GIVE REWARD", "Purrgatory Questline Finish" },
                { "MainOverworld|SQ_GentlebrosSpeech|FSM|give reward Body and Head", "Gentlebros Quest Reward" },
                { "MainOverworld|SQ_SiblingRivalry_Sea|FSM|Message to say get key", "Heirloom Quest Sea Shadow" },
                { "MainOverworld|WorldQuest_BoarBoss|FSM|Defeat Monster and Reward", "Defeat Boar King" },
                { "MainOverworld|WorldQuest_Fishercat_FishGhost|FSM|CAUGHT FISH CHEST SPAWN", "Catch Ghost Fish" },
                { "MainOverworld|WorldQuest_Fishercat_FishLove|FSM|CAUGHT FISH CHEST SPAWN", "Catch Love Fish" },
                { "MainOverworld|WorldQuest_Fishercat_FishRock|FSM|CAUGHT FISH CHEST SPAWN", "Catch Rock Fish" },
                { "MainOverworld|WorldQuest_Fishercat_FishSunset|FSM|CAUGHT FISH CHEST SPAWN", "Catch Sunset Fish" },
                { "MainOverworld|WorldQuest_Fishercat|FSM|CAUGHT FISH SPAWN CHEST", "Catch Regular Fish" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|GET REWARD", "Delivery to Blue Starfish" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|GET REWARD 2", "Delivery to Pink Starfish" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|GET REWARD 3", "Delivery to Boarb" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|GET REWARD 4", "Delivery to Charon" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|GET REWARD 5", "Delivery to Floaty Rat" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|GET REWARD 6", "Delivery to Charon" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|GET REWARD 7", "Delivery to Skull" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item)", "Mail from Postmutt" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item) 2", "Mail from Blue Starfish" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item) 3", "Mail from Pink Starfish" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item) 4", "Mail from Board 1" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item) 5", "Mail from Floaty Rat" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item) 6", "Mail from Charon" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item) 7", "Mail from Skull" },
                { "MainOverworld|WorldQuest_GoingPostal|FSM|Get New Mail (Quest Item) 8", "Mail from Boarb 2" },
                { "MainOverworld|WorldQuest_HiddenItem_Furggy_Rock|FSM|Spawn Chest", "Reveal Hidden - Furrgy" },
                { "MainOverworld|WorldQuest_HiddenItem_Furtigua_Rock|FSM|Spawn Chest", "Reveal Hidden - Furtuga" },
                { "MainOverworld|WorldQuest_HiddenItem_PawtPurvanna_Bush|FSM|Spawn Chest", "Reveal Hidden - PawtPurvanna" },
                { "MainOverworld|WorldQuest_HiddenItem_Purvanna_Starfish|FSM|Spawn Chest", "Reveal Hidden - Purvanna Starfish" },
                { "MainOverworld|WorldQuest_HiddenItem_SandyIsle_Starfish|FSM|Spawn Chest", "Reveal Hidden - SandyIsle Starfish" },
                { "MainOverworld|WorldQuest_HiddenItem_SunsetMaze_Starfish|FSM|Spawn Chest", "Reveal Hidden - SunsetMaze Starfish" },
                { "MainOverworld|WorldQuest_LostItem_PurrierReefTreasure|FSM|Get Reward", "Lost Milk" },
                { "MainOverworld|WorldQuest_LostItem_SunsetTreasure|FSM|Get Reward", "Lost Voodoo Doll" },
                { "MainOverworld|WorldQuest_LostItem_TwilightTreasure|FSM|Get Reward", "Lost Hammer" },
                { "MainOverworld|WorldQuest_Mewtallica_BossShip|FSM|Give Ship Upgrade", "Defeat Meowtallicurse" },
                { "MainOverworld|WorldQuest_MonsterResearcher|FSM|Capture Boar and Reward", "Capture Boar for Science" },
                { "MainOverworld|WorldQuest_MonsterResearcher|FSM|Capture Chubby and Reward", "Capture Chubby for Science" },
                { "MainOverworld|WorldQuest_PirateKing_BossShip|FSM|Give Ship Upgrade", "Defeat Unsinkable" },
                { "MainOverworld|WorldQuest_PirateKing|FSM|Get Quest Item", "Defeat Pirat King (Dragonbone)" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_Catuga|FSM|Get Reward", "Purrmaid Pool Catuga" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_Furggy|FSM|Get Reward", "Purrmaid Pool Furggy" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_Furtigua|FSM|Get Reward", "Purrmaid Pool Furtigua" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_PawtPurvanna|FSM|Get Reward", "Purrmaid Pool PawtPurvanna" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_Purvanna|FSM|Get Reward", "Purrmaid Pool Purvanna" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_Sunset|FSM|Get Reward", "Purrmaid Pool Sunset" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_Twilight|FSM|Get Reward", "Purrmaid Pool Twilight" },
                { "MainOverworld|WorldQuest_PurrmaidTreasure_Twin|FSM|Get Reward", "Purrmaid Pool Twin Castle" },
                { "MainOverworld|WorldQuest_Purrmaid|FSM|Get Shell Phone", "Rescue Purrmaid" },
                { "MainOverworld|WorldQuest_PurrseidonsTrident|FSM|Get Trident as reward", "Purrseidon Reward" },
                { "MainOverworld|WorldQuest_PuzzleStone_Meowgus|FSM|Reward with mana", "Meowgus Mana Crystal" },
                { "MainOverworld|WorldQuest_RubberDuck|FSM|Award Blueprint", "Defeat Duck of Doom" },
                { "MainOverworld|WorldQuest_RunningGold_Furggy|FSM|Get Golden Key 05", "Running Coin Furrgy" },
                { "MainOverworld|WorldQuest_RunningGold_Furtigua|FSM|Get Golden Key 03", "Running Coin Furtuga" },
                { "MainOverworld|WorldQuest_RunningGold_Purvanna|FSM|Get Golden Key 02", "Running Coin Purvanna" },
                { "MainOverworld|WorldQuest_RunningGold_Sunset|FSM|Get Golden Key", "Running Coin Sunset" },
                { "MainOverworld|WorldQuest_StraitsWatchtower|FSM|Go get Lousy Boot and Reward", "Watchtower Quest 1" },
                { "MainOverworld|WorldQuest_StraitsWatchtower|FSM|Go get Lousy Gloves and Reward", "Watchtower Quest 2" },
                { "Ruins_Antares|MainQuest_04_AntaresRuins|FSM|Get Key of Antares", "Defeat Antares - Key" },
                { "Ruins_Antares|MainQuest_04_AntaresRuins|FSM|Learn Spell", "Defeat Antares - Spell" },
                { "Ruins_Antares|MainQuest_04_AntaresRuins|FSM|Spawn Loot", "Defeat Antares - Loot" },
                { "Ruins_Centauri|DungeonQuest_GoingPostal_OinkerChief|FSM|GET REWARD", "Delivery to Oinker" },
                { "Ruins_Centauri|DungeonQuest_GoingPostal_OinkerChief|FSM|Get New Mail (Quest Item)", "Mail from Oinker" },
                { "Ruins_Centauri|MainQuest_05_CentauriRuins|FSM|Go Talk to Pig Boss, get Centauri Key", "Oinker Chief Reward" },
                { "Ruins_Orion|MainQuest_03_OrionRuins|FSM|Get Orion Key", "Defeat Orion - Key" },
                { "Ruins_Orion|MainQuest_03_OrionRuins|FSM|Learn Spell", "Defeat Orion - Spell" },
                { "Ruins_Orion|MainQuest_03_OrionRuins|FSM|Spawn Boss Chest", "Defeat Orion - Loot" },
                { "Ruins_Polaris|MainQuest_06_PolarisRuins|FSM|Get Infinity Key", "Polaris Mural - Infinity Key" },
                { "Ruins_Polaris|MainQuest_06_PolarisRuins|FSM|Mewtallica talks", "Polaris - Meowtallika Ticket" },
            };

        public static int Count =>
            Labels.Count;

        public static bool TryGetLabel(
            string locationKey,
            out string label)
        {
            return Labels.TryGetValue(
                locationKey,
                out label
            );
        }
    }
}
