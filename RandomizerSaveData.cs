using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Randomizer.CatQuest3
{
    public static class RandomizerSaveData
    {
        private const int CurrentVersion = 1;

        private static string SaveDirectory
        {
            get
            {
                return Path.Combine(
                    Paths.ConfigPath,
                    "Randomizer.CatQuest3",
                    "SaveSlots"
                );
            }
        }


        public static bool SaveConfiguration(
            int profileIndex,
            RandomizerRunConfiguration configuration)
        {
            if (profileIndex < 0)
            {
                Plugin.Log.LogWarning(
                    "RANDOMIZER SAVE | " +
                    "Cannot save configuration for invalid profile index:" +
                    profileIndex
                );

                return false;
            }


            if (configuration == null)
            {
                Plugin.Log.LogWarning(
                    "RANDOMIZER SAVE | " +
                    "Cannot save configuration because it is null. " +
                    "Profile index:" +
                    profileIndex
                );

                return false;
            }


            try
            {
                Directory.CreateDirectory(
                    SaveDirectory
                );


                string path =
                    GetProfilePath(
                        profileIndex
                    );


                string encodedSeed =
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            configuration.SeedText ?? ""
                        )
                    );


                string[] lines =
                    new string[]
                    {
                        "Version=" +
                            CurrentVersion,

                        "ProfileIndex=" +
                            profileIndex,

                        "Enabled=" +
                            configuration.Enabled,

                        "Equipment=" +
                            configuration.Equipment,

                        "Spells=" +
                            configuration.Spells,

                        "QuestItems=" +
                            configuration.QuestItems,

                        "ManaCrystals=" +
                            configuration.ManaCrystals,

                        "ResourcesCollectibles=" +
                            configuration.ResourcesCollectibles,

                        "ShipKey=" +
                            configuration.ShipKey,

                        "InfinityKey=" +
                            configuration.InfinityKey,

                        "BirdPoop=" +
                            configuration.BirdPoop,

                        "MatchPlayerLevel=" +
                            configuration.MatchPlayerLevel,

                        "SeedTextBase64=" +
                            encodedSeed,

                        "SeedValue=" +
                            configuration.SeedValue
                    };


                File.WriteAllLines(
                    path,
                    lines
                );


                Plugin.Log.LogInfo(
                    "RANDOMIZER SAVE | " +
                    "Configuration persisted for profile index:" +
                    profileIndex +
                    " | Path:" +
                    path
                );


                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER SAVE | " +
                    "Failed to persist configuration for profile index:" +
                    profileIndex +
                    " | " +
                    ex
                );


                return false;
            }
        }


        public static bool TryLoadConfiguration(
            int profileIndex,
            out RandomizerRunConfiguration configuration)
        {
            configuration =
                null;


            if (profileIndex < 0)
            {
                return false;
            }


            string path =
                GetProfilePath(
                    profileIndex
                );


            if (!File.Exists(path))
            {
                Plugin.Log.LogInfo(
                    "RANDOMIZER SAVE | " +
                    "No stored configuration for profile index:" +
                    profileIndex
                );

                return false;
            }


            try
            {
                Dictionary<string, string> values =
                    ReadValues(
                        path
                    );


                int version;
                if (!TryGetInt(
                    values,
                    "Version",
                    out version))
                {
                    Plugin.Log.LogWarning(
                        "RANDOMIZER SAVE | " +
                        "Configuration has no valid version. " +
                        "Profile index:" +
                        profileIndex
                    );

                    return false;
                }


                if (version != CurrentVersion)
                {
                    Plugin.Log.LogWarning(
                        "RANDOMIZER SAVE | " +
                        "Unsupported configuration version:" +
                        version +
                        " | Profile index:" +
                        profileIndex
                    );

                    return false;
                }


                bool enabled;
                bool equipment;
                bool spells;
                bool questItems;
                bool manaCrystals;
                bool resourcesCollectibles;
                bool shipKey;
                bool infinityKey;
                bool birdPoop;
                bool matchPlayerLevel;
                int seedValue;


                if (!TryGetBool(values, "Enabled", out enabled) ||
                    !TryGetBool(values, "Equipment", out equipment) ||
                    !TryGetBool(values, "Spells", out spells) ||
                    !TryGetBool(values, "QuestItems", out questItems) ||
                    !TryGetBool(values, "ManaCrystals", out manaCrystals) ||
                    !TryGetBool(values, "ResourcesCollectibles", out resourcesCollectibles) ||
                    !TryGetBool(values, "ShipKey", out shipKey) ||
                    !TryGetBool(values, "InfinityKey", out infinityKey) ||
                    !TryGetBool(values, "BirdPoop", out birdPoop) ||
                    !TryGetBool(values, "MatchPlayerLevel", out matchPlayerLevel) ||
                    !TryGetInt(values, "SeedValue", out seedValue))
                {
                    Plugin.Log.LogWarning(
                        "RANDOMIZER SAVE | " +
                        "Configuration is missing or has invalid values. " +
                        "Profile index:" +
                        profileIndex
                    );

                    return false;
                }


                string encodedSeed;
                if (!values.TryGetValue(
                    "SeedTextBase64",
                    out encodedSeed))
                {
                    Plugin.Log.LogWarning(
                        "RANDOMIZER SAVE | " +
                        "Configuration has no seed text. " +
                        "Profile index:" +
                        profileIndex
                    );

                    return false;
                }


                string seedText =
                    Encoding.UTF8.GetString(
                        Convert.FromBase64String(
                            encodedSeed
                        )
                    );


                configuration =
                    new RandomizerRunConfiguration(
                        enabled,
                        equipment,
                        spells,
                        questItems,
                        manaCrystals,
                        resourcesCollectibles,
                        shipKey,
                        infinityKey,
                        birdPoop,
                        matchPlayerLevel,
                        seedText,
                        seedValue
                    );


                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER SAVE | " +
                    "Failed to load configuration for profile index:" +
                    profileIndex +
                    " | " +
                    ex
                );


                configuration =
                    null;

                return false;
            }
        }


        private static Dictionary<string, string> ReadValues(
            string path)
        {
            Dictionary<string, string> values =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase
                );


            string[] lines =
                File.ReadAllLines(
                    path
                );


            for (int i = 0;
                 i < lines.Length;
                 i++)
            {
                string line =
                    lines[i];


                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }


                int separatorIndex =
                    line.IndexOf('=');


                if (separatorIndex <= 0)
                {
                    continue;
                }


                string key =
                    line.Substring(
                        0,
                        separatorIndex
                    )
                    .Trim();

                string value =
                    line.Substring(
                        separatorIndex + 1
                    )
                    .Trim();


                values[key] =
                    value;
            }


            return values;
        }


        private static bool TryGetBool(
            Dictionary<string, string> values,
            string key,
            out bool result)
        {
            result =
                false;


            string value;
            if (!values.TryGetValue(
                key,
                out value))
            {
                return false;
            }


            return bool.TryParse(
                value,
                out result
            );
        }


        private static bool TryGetInt(
            Dictionary<string, string> values,
            string key,
            out int result)
        {
            result =
                0;


            string value;
            if (!values.TryGetValue(
                key,
                out value))
            {
                return false;
            }


            return int.TryParse(
                value,
                out result
            );
        }


        private static string GetProfilePath(
            int profileIndex)
        {
            return Path.Combine(
                SaveDirectory,
                "profile_" +
                    profileIndex +
                    ".cfg"
            );
        }
    }
}
