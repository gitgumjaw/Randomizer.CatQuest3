
// Cat Quest III Randomizer
// HeartpurrokenPatch_20261105_2253_EST_v02.cs
//
// Heartpurroken-specific modifications:
// 1. Force boss variant agentNavtype to 2.
// 2. TEMPORARY TEST: Force boss Heartpurroken at
//    the MainOverworld Boar Boss quest spawner.
//
// No changes to Plugin.cs required.

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(UnitSpawner),
        nameof(UnitSpawner.PopulateSpawnerEntity))]
    internal static class HeartpurrokenPatch
    {
        private const string TargetConfig =
            "Spherical_SpikeballBoss_MonsterConfig";

        private const string TestScene = "MainOverworld";

        private const string TestSpawnerId =
            "1724078896-8ba0fbd11d84e144c87311f29eadee97";

        // Change to false after finishing the spawn test.
        private const bool ForceTestSpawn = false;

        private const int TargetNavType = 2;

        private static readonly HashSet<int> processed =
            new HashSet<int>();

        // Execute after the enemy randomizer's normal Prefix.
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        private static void Prefix(UnitSpawner __instance)
        {
            if (__instance == null ||
                __instance.spawnList == null)
                return;

            if (ForceTestSpawn &&
                string.Equals(
                    __instance.gameObject.scene.name,
                    TestScene,
                    StringComparison.Ordinal) &&
                string.Equals(
                    __instance.spawnerId.ToString(),
                    TestSpawnerId,
                    StringComparison.Ordinal))
            {
                ForceHeartpurroken(__instance);
            }

            // Apply navigation change to the boss variant
            // wherever that configuration appears.
            foreach (SpawnTableItem item in __instance.spawnList)
            {
                if (item == null || item.spawnConfig == null)
                    continue;

                UnitConfig config = item.spawnConfig;

                if (!string.Equals(
                    config.name,
                    TargetConfig,
                    StringComparison.Ordinal))
                    continue;

                ApplyNavigation(config);
            }
        }

        private static void ForceHeartpurroken(
            UnitSpawner spawner)
        {
            UnitConfig boss = null;

            foreach (UnitConfig config in
                Resources.FindObjectsOfTypeAll<UnitConfig>())
            {
                if (config != null &&
                    string.Equals(
                        config.name,
                        TargetConfig,
                        StringComparison.Ordinal))
                {
                    boss = config;
                    break;
                }
            }

            if (boss == null)
            {
                Plugin.Log.LogWarning(
                    "HEARTPURROKEN TEST | Boss UnitConfig not found.");
                return;
            }

            // Set navigation before the spawner uses the asset.
            ApplyNavigation(boss);

            List<SpawnTableItem> replacementList =
                new List<SpawnTableItem>();

            foreach (SpawnTableItem original in spawner.spawnList)
            {
                if (original == null)
                    continue;

                SpawnTableItem replacement =
                    new SpawnTableItem(
                        boss,
                        original.probability);

                replacement.spawnCount = original.spawnCount;

                replacementList.Add(replacement);
            }

            if (replacementList.Count == 0)
            {
                Plugin.Log.LogWarning(
                    "HEARTPURROKEN TEST | No spawn entries to replace.");
                return;
            }

            spawner.spawnList = replacementList;

            Plugin.Log.LogWarning(
                "HEARTPURROKEN TEST | FORCED | Scene:" +
                spawner.gameObject.scene.name +
                " | Spawner:" + spawner.spawnerId +
                " | Config:" + TargetConfig +
                " | Entries:" + replacementList.Count);
        }

        private static void ApplyNavigation(UnitConfig config)
        {
            if (!processed.Add(config.GetInstanceID()))
                return;

            if (SetNavType(config))
            {
                Plugin.Log.LogInfo(
                    "HEARTPURROKEN NAV | Config:" +
                    config.name +
                    " | agentNavtype:2");
            }
            else
            {
                processed.Remove(config.GetInstanceID());
            }
        }

        private static bool SetNavType(UnitConfig config)
        {
            const BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            try
            {
                for (Type type = config.GetType();
                     type != null;
                     type = type.BaseType)
                {
                    FieldInfo field =
                        type.GetField("agentNavtype", flags);

                    if (field != null)
                    {
                        object value = field.FieldType.IsEnum
                            ? Enum.ToObject(
                                field.FieldType, TargetNavType)
                            : Convert.ChangeType(
                                TargetNavType, field.FieldType);

                        field.SetValue(config, value);
                        return true;
                    }

                    PropertyInfo property =
                        type.GetProperty("agentNavtype", flags);

                    if (property != null && property.CanWrite)
                    {
                        object value = property.PropertyType.IsEnum
                            ? Enum.ToObject(
                                property.PropertyType, TargetNavType)
                            : Convert.ChangeType(
                                TargetNavType, property.PropertyType);

                        property.SetValue(config, value, null);
                        return true;
                    }
                }

                Plugin.Log.LogWarning(
                    "HEARTPURROKEN NAV | agentNavtype not found");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(
                    "HEARTPURROKEN NAV | Error:" +
                    ex.GetType().Name + " | " + ex.Message);
            }

            return false;
        }
    }
}
