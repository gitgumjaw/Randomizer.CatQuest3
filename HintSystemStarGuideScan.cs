
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    internal static class HintSystemStarGuideScan
    {
        internal static void Dump()
        {
            Scene scene = SceneManager.GetSceneByName("MainOverworld");

            if (!scene.IsValid() || !scene.isLoaded)
            {
                Plugin.Log.LogWarning(
                    "STAR GUIDE SCAN | MainOverworld is not loaded.");
                return;
            }

            List<string> paths = new List<string>();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                ScanTransform(root.transform, paths);
            }

            paths.Sort(StringComparer.Ordinal);

            Plugin.Log.LogInfo(
                "STAR GUIDE SCAN | === START === | count=" + paths.Count);

            foreach (string path in paths)
            {
                Plugin.Log.LogInfo(
                    "STAR GUIDE SCAN | LOCATION|MainOverworld|" + path);
            }

            Plugin.Log.LogInfo(
                "STAR GUIDE SCAN | === END === | count=" + paths.Count);
        }

        private static void ScanTransform(
            Transform current,
            List<string> paths)
        {
            string name = current.gameObject.name;

            if (name == "Interactable_StarGuide" ||
                name.StartsWith(
                    "Interactable_StarGuide (",
                    StringComparison.Ordinal))
            {
                paths.Add(GetHierarchyPath(current));
            }

            for (int i = 0; i < current.childCount; i++)
            {
                ScanTransform(current.GetChild(i), paths);
            }
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;

            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
