
// Cat Quest III Randomizer
// File: GentlebrosSpawnerDiagnostic.cs
// Version: TavernTalesSpawnerDiagnostic_20261108_v02
//
// Universal, read-only Tavern Tales spawner diagnostic.
//
// F12: Capture every UnitSpawner in the current Tavern Tales arena.
// Automatic scans: Scene load + 0.5s, +2s, +5s.
//
// IMPORTANT:
// - No deduplication by spawnerId.
// - Records every component on every scan.
// - Captures empty and dynamically populated spawn tables.
// - Detects duplicate spawner IDs.
// - Does not modify spawners or randomization.
// - Replaces the previous Gentlebros-only diagnostic.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    internal sealed class GentlebrosSpawnerDiagnostic : MonoBehaviour
    {
        private static GentlebrosSpawnerDiagnostic instance;

        private string outputPath;
        private int scanNumber;

        internal static void Initialize()
        {
            if (instance != null)
                return;

            GameObject go = new GameObject(
                "Randomizer_TavernTalesSpawnerDiagnostic");

            DontDestroyOnLoad(go);

            instance =
                go.AddComponent<GentlebrosSpawnerDiagnostic>();
        }

        private void Awake()
        {
            string folder = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            outputPath = Path.Combine(
                folder,
                "Data",
                "TavernTalesSpawnerScan.tsv");

            Directory.CreateDirectory(
                Path.GetDirectoryName(outputPath));

            // Fresh report each game launch.
            // Does not touch SpawnerSceneScan.tsv.
            File.WriteAllText(
                outputPath,
                "Scan\tReason\tScene\tInstanceID\t" +
                "SpawnerID\tName\tGroup\tHierarchy\t" +
                "ActiveHierarchy\tActiveSelf\tEnabled\t" +
                "SpawnType\tEnableOverrides\t" +
                "WorldPosition\tVanillaConfigs\t" +
                "RufferConfigs" +
                Environment.NewLine,
                Encoding.UTF8);

            SceneManager.sceneLoaded += OnSceneLoaded;

            Plugin.Log.LogInfo(
                "TAVERN TALES DIAG | Ready." +
                " F12 scans all Tavern Tales spawners." +
                " Output:" + outputPath);

            StartCoroutine(InitialScan());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (instance == this)
                instance = null;
        }

        private IEnumerator InitialScan()
        {
            yield return new WaitForSecondsRealtime(1f);

            Scan("INITIAL");
        }

        private void OnSceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            if (!IsTavernTalesScene(scene.name))
                return;

            StartCoroutine(DelayedScans(scene.name));
        }

        private IEnumerator DelayedScans(string sceneName)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            Scan("SCENE_LOADED_0.5s:" + sceneName);

            yield return new WaitForSecondsRealtime(1.5f);
            Scan("SCENE_LOADED_2s:" + sceneName);

            yield return new WaitForSecondsRealtime(3f);
            Scan("SCENE_LOADED_5s:" + sceneName);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
                Scan("F12");
        }

        private void Scan(string reason)
        {
            scanNumber++;

            int found = 0;
            int errors = 0;
            int empty = 0;

            Dictionary<string, int> idCounts =
                new Dictionary<string, int>(
                    StringComparer.Ordinal);

            List<string> rows = new List<string>();

            try
            {
                UnitSpawner[] all =
                    Resources.FindObjectsOfTypeAll<UnitSpawner>();

                foreach (UnitSpawner spawner in all)
                {
                    if (spawner == null)
                        continue;

                    try
                    {
                        GameObject go = spawner.gameObject;

                        if (go == null)
                            continue;

                        Scene scene = go.scene;

                        if (!scene.IsValid() || !scene.isLoaded)
                            continue;

                        string hierarchy =
                            GetHierarchy(go.transform);

                        if (!IsTavernTalesScene(scene.name) &&
                            !IsTavernTalesHierarchy(hierarchy))
                            continue;

                        found++;

                        string id = spawner.spawnerId ?? "";

                        string duplicateKey =
                            scene.name + "|" + id;

                        int count;

                        idCounts.TryGetValue(
                            duplicateKey,
                            out count);

                        idCounts[duplicateKey] = count + 1;

                        string group =
                            GetGroup(go.transform);

                        string vanilla =
                            DescribeList(spawner.spawnList);

                        string ruffer =
                            spawner.enableOverrides
                                ? DescribeList(spawner.rufferSpawnList)
                                : "";

                        if (spawner.spawnList == null ||
                            spawner.spawnList.Count == 0)
                        {
                            empty++;
                        }

                        Vector3 position =
                            go.transform.position;

                        string coordinates =
                            Format(position.x) + "," +
                            Format(position.y) + "," +
                            Format(position.z);

                        string[] columns =
                        {
                            scanNumber.ToString(),
                            reason,
                            scene.name,
                            spawner.GetInstanceID().ToString(),
                            id,
                            go.name,
                            group,
                            hierarchy,
                            go.activeInHierarchy.ToString(),
                            go.activeSelf.ToString(),
                            spawner.enabled.ToString(),
                            spawner.spawnType.ToString(),
                            spawner.enableOverrides.ToString(),
                            coordinates,
                            vanilla,
                            ruffer
                        };

                        rows.Add(JoinTsv(columns));

                        Plugin.Log.LogInfo(
                            "TAVERN TALES DIAG | SPAWNER" +
                            " | Scan:" + scanNumber +
                            " | Scene:" + scene.name +
                            " | Instance:" + spawner.GetInstanceID() +
                            " | ID:" + id +
                            " | Group:" + group +
                            " | Name:" + go.name +
                            " | Active:" + go.activeInHierarchy +
                            " | Enabled:" + spawner.enabled +
                            " | Type:" + spawner.spawnType +
                            " | Vanilla:" + vanilla +
                            " | Ruffer:" + ruffer +
                            " | Path:" + hierarchy);
                    }
                    catch (Exception ex)
                    {
                        errors++;

                        Plugin.Log.LogWarning(
                            "TAVERN TALES DIAG | Entry error: " +
                            ex);
                    }
                }

                // Write all rows, including duplicate IDs and
                // repeated observations across combat phases.
                if (rows.Count > 0)
                {
                    File.AppendAllText(
                        outputPath,
                        string.Join(
                            Environment.NewLine,
                            rows.ToArray()) +
                        Environment.NewLine,
                        Encoding.UTF8);
                }

                foreach (KeyValuePair<string, int> pair
                    in idCounts)
                {
                    if (pair.Value <= 1)
                        continue;

                    Plugin.Log.LogWarning(
                        "TAVERN TALES DIAG | DUPLICATE ID" +
                        " | Scene+ID:" + pair.Key +
                        " | Components:" + pair.Value);
                }
            }
            catch (Exception ex)
            {
                errors++;

                Plugin.Log.LogError(
                    "TAVERN TALES DIAG | Scan error: " +
                    ex);
            }

            Plugin.Log.LogWarning(
                "TAVERN TALES DIAG | SCAN COMPLETE" +
                " | Number:" + scanNumber +
                " | Trigger:" + reason +
                " | ActiveScene:" +
                    SceneManager.GetActiveScene().name +
                " | Spawners:" + found +
                " | EmptyTables:" + empty +
                " | Errors:" + errors);
        }

        private static bool IsTavernTalesScene(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return
                name.IndexOf(
                    "TavernTales",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf(
                    "GentlebrosArena",
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTavernTalesHierarchy(string hierarchy)
        {
            if (string.IsNullOrEmpty(hierarchy))
                return false;

            return
                hierarchy.IndexOf(
                    "TavernTalesEncounterContainer",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                hierarchy.IndexOf(
                    "TavernTalesEncounterManager",
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string DescribeList(
            List<SpawnTableItem> list)
        {
            if (list == null)
                return "NULL";

            if (list.Count == 0)
                return "EMPTY";

            List<string> entries = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                SpawnTableItem item = list[i];

                if (item == null)
                {
                    entries.Add(i + ":NULL");
                    continue;
                }

                string configName =
                    item.spawnConfig != null
                        ? item.spawnConfig.name
                        : "NULL";

                entries.Add(
                    i + ":" + configName +
                    " x" + item.spawnCount +
                    " p" + item.probability);
            }

            return string.Join("; ", entries.ToArray());
        }

        private static string GetGroup(Transform transform)
        {
            Transform current = transform;

            while (current != null)
            {
                if (current.parent != null &&
                    string.Equals(
                        current.parent.name,
                        "Spawner Groups",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return current.name;
                }

                current = current.parent;
            }

            return "Unknown";
        }

        private static string GetHierarchy(Transform transform)
        {
            List<string> names = new List<string>();

            Transform current = transform;

            int depth = 0;

            while (current != null && depth++ < 128)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();

            return string.Join("/", names.ToArray());
        }

        private static string Format(float value)
        {
            return value.ToString(
                "F3",
                CultureInfo.InvariantCulture);
        }

        private static string JoinTsv(string[] columns)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = (columns[i] ?? "")
                    .Replace('\t', ' ')
                    .Replace('\r', ' ')
                    .Replace('\n', ' ');
            }

            return string.Join("\t", columns);
        }
    }
}
