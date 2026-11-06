
// Cat Quest III Randomizer
// File: SpawnerLocationDiagnostic.cs
// Version: SpawnerLocationDiagnostic_20261105_1911_EST_v01
// Purpose: Map UnitSpawner instances to scene locations.
// Type: Read-only diagnostic.
// Installation: Add as a new C# file. No Plugin.cs changes.
// Capture: Logs each UnitSpawner instance once when populated.

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(UnitSpawner),
        nameof(UnitSpawner.PopulateSpawnerEntity)
    )]
    internal static class SpawnerLocationDiagnostic
    {
        private static readonly HashSet<int> LoggedInstances =
            new HashSet<int>();

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        private static void Prefix(UnitSpawner __instance)
        {
            if (__instance == null)
                return;

            try
            {
                int instanceId = __instance.GetInstanceID();

                if (!LoggedInstances.Add(instanceId))
                    return;

                GameObject go = __instance.gameObject;
                Transform transform = go.transform;

                string scene = go.scene.name;
                string path = GetHierarchyPath(transform);

                Vector3 position = transform.position;
                Vector3 localPosition = transform.localPosition;

                Plugin.Log.LogInfo(
                    "SPAWNER MAP | BEGIN" +
                    " | Scene:" + scene +
                    " | SpawnerID:" + __instance.spawnerId
                );

                Plugin.Log.LogInfo(
                    "SPAWNER MAP | IDENTITY" +
                    " | InstanceID:" + instanceId +
                    " | Object:" + go.name +
                    " | Path:" + path
                );

                Plugin.Log.LogInfo(
                    "SPAWNER MAP | POSITION" +
                    " | World:" + FormatVector(position) +
                    " | Local:" + FormatVector(localPosition)
                );

                Plugin.Log.LogInfo(
                    "SPAWNER MAP | SETTINGS" +
                    " | SpawnType:" + __instance.spawnType +
                    " | EnableOverrides:" + __instance.enableOverrides
                );

                LogSpawnList(
                    "VANILLA",
                    __instance.spawnList
                );

                if (__instance.enableOverrides)
                {
                    LogSpawnList(
                        "RUFFER",
                        __instance.rufferSpawnList
                    );
                }

                Plugin.Log.LogInfo(
                    "SPAWNER MAP | END" +
                    " | Scene:" + scene +
                    " | SpawnerID:" + __instance.spawnerId
                );
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(
                    "SPAWNER MAP | ERROR | " + ex
                );
            }
        }

        private static void LogSpawnList(
            string listName,
            List<SpawnTableItem> list)
        {
            if (list == null)
            {
                Plugin.Log.LogInfo(
                    "SPAWNER MAP | " + listName +
                    " | List:NULL"
                );
                return;
            }

            Plugin.Log.LogInfo(
                "SPAWNER MAP | " + listName +
                " | Count:" + list.Count
            );

            for (int i = 0; i < list.Count; i++)
            {
                SpawnTableItem item = list[i];

                if (item == null)
                {
                    Plugin.Log.LogInfo(
                        "SPAWNER MAP | " + listName +
                        " | Slot:" + i +
                        " | Item:NULL"
                    );
                    continue;
                }

                string configName =
                    item.spawnConfig != null
                        ? item.spawnConfig.name
                        : "NULL";

                Plugin.Log.LogInfo(
                    "SPAWNER MAP | " + listName +
                    " | Slot:" + i +
                    " | Config:" + configName +
                    " | Probability:" + item.probability +
                    " | SpawnCount:" + item.spawnCount
                );
            }
        }

        private static string GetHierarchyPath(
            Transform transform)
        {
            if (transform == null)
                return "NULL";

            string path = transform.name;
            int depth = 0;

            while (transform.parent != null && depth < 64)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
                depth++;
            }

            return path;
        }

        private static string FormatVector(Vector3 vector)
        {
            return
                vector.x.ToString("F3",
                    System.Globalization.CultureInfo.InvariantCulture) +
                "," +
                vector.y.ToString("F3",
                    System.Globalization.CultureInfo.InvariantCulture) +
                "," +
                vector.z.ToString("F3",
                    System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
