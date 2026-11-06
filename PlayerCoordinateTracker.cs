
// Cat Quest III Randomizer | PlayerCoordinateTracker.cs
// Version: PlayerCoordinateTracker_20261105_2150_EST_v01
// F10 toggles a live world-coordinate overlay.
// Read-only diagnostic. No Harmony patches.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Randomizer.CatQuest3
{
    internal sealed class PlayerCoordinateTracker : MonoBehaviour
    {
        private bool visible;
        private GUIStyle titleStyle;
        private GUIStyle valueStyle;
        private GUIStyle detailStyle;

        internal static void Initialize()
        {
            if (GameObject.Find("Randomizer_CoordinateTracker") != null)
                return;

            GameObject go = new GameObject(
                "Randomizer_CoordinateTracker");

            DontDestroyOnLoad(go);
            go.AddComponent<PlayerCoordinateTracker>();
        }

        private void Awake()
        {
            Plugin.Log.LogInfo(
                "COORD TRACKER | Ready. Press F10 to toggle.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10))
            {
                visible = !visible;

                Plugin.Log.LogInfo(
                    "COORD TRACKER | " +
                    (visible ? "Visible" : "Hidden"));
            }
        }

        private void OnGUI()
        {
            if (!visible)
                return;

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontSize = 12;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.normal.textColor = Color.cyan;

                valueStyle = new GUIStyle(GUI.skin.label);
                valueStyle.fontSize = 14;
                valueStyle.normal.textColor = Color.white;

                detailStyle = new GUIStyle(GUI.skin.label);
                detailStyle.fontSize = 11;
                detailStyle.normal.textColor = Color.white;
            }

            Rect panel = new Rect(12f, 12f, 225f, 155f);
            GUI.Box(panel, "");

            GUI.Label(
                new Rect(22f, 18f, 205f, 22f),
                "WORLD COORDINATES",
                titleStyle);

            Vector3 position;

            if (TryGetPlayerPosition(out position))
            {
                GUI.Label(
                    new Rect(22f, 44f, 205f, 22f),
                    "X: " + position.x.ToString("F3"),
                    valueStyle);

                GUI.Label(
                    new Rect(22f, 68f, 205f, 22f),
                    "Y: " + position.y.ToString("F3"),
                    valueStyle);

                GUI.Label(
                    new Rect(22f, 92f, 205f, 22f),
                    "Z: " + position.z.ToString("F3"),
                    valueStyle);
            }
            else
            {
                GUI.Label(
                    new Rect(22f, 55f, 205f, 22f),
                    "Player not available",
                    detailStyle);
            }

            GUI.Label(
                new Rect(22f, 124f, 205f, 22f),
                "Scene: " + SceneManager.GetActiveScene().name,
                detailStyle);
        }

        private static bool TryGetPlayerPosition(
            out Vector3 position)
        {
            position = Vector3.zero;

            try
            {
                foreach (GameEntity player in
                    Contexts.sharedInstance.game.GetGroup(
                        GameMatcher.PlayerId))
                {
                    if (player != null &&
                        player.hasTransform &&
                        player.transform.value != null)
                    {
                        position = player.transform.value.position;
                        return true;
                    }
                }
            }
            catch (System.Exception)
            {
                // Player entity may not be ready during loading.
            }

            return false;
        }
    }
}
