using HarmonyLib;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(OptionsPanel),
        "Show"
    )]
    public static class CameraShakeHierarchyDiagnosticPatch
    {
        private static bool logged;


        public static void Postfix(
            OptionsPanel __instance)
        {
            if (logged ||
                __instance == null)
            {
                return;
            }

            logged = true;


            Transform cameraShake =
                __instance.transform.Find(
                    "CameraShake"
                );


            if (cameraShake == null)
            {
                /*
                 * Depending on which object the OptionsPanel
                 * component is attached to, CameraShake may
                 * be under the same root rather than a direct
                 * child of this transform.
                 */
                cameraShake =
                    FindChildRecursive(
                        __instance.transform,
                        "CameraShake"
                    );
            }


            if (cameraShake == null)
            {
                Plugin.Log.LogError(
                    "RANDOMIZER CAMERA SHAKE HIERARCHY | " +
                    "CameraShake not found."
                );

                return;
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER CAMERA SHAKE HIERARCHY | BEGIN"
            );


            DumpTransform(
                cameraShake,
                0,
                5
            );


            Plugin.Log.LogInfo(
                "RANDOMIZER CAMERA SHAKE HIERARCHY | END"
            );
        }


        private static void DumpTransform(
            Transform transform,
            int depth,
            int maxDepth)
        {
            if (transform == null ||
                depth > maxDepth)
            {
                return;
            }


            string indent =
                new string(
                    ' ',
                    depth * 2
                );


            RectTransform rect =
                transform as RectTransform;


            string rectInfo =
                "";

            if (rect != null)
            {
                rectInfo =
                    " | AnchoredPos:" +
                    rect.anchoredPosition +
                    " | Size:" +
                    rect.rect.size +
                    " | AnchorMin:" +
                    rect.anchorMin +
                    " | AnchorMax:" +
                    rect.anchorMax +
                    " | Pivot:" +
                    rect.pivot;
            }


            Plugin.Log.LogInfo(
                "RANDOMIZER CAMERA SHAKE HIERARCHY | " +
                indent +
                transform.name +
                " | ActiveSelf:" +
                transform.gameObject.activeSelf +
                " | Components:" +
                GetComponentNames(
                    transform.gameObject
                ) +
                rectInfo
            );


            for (int i = 0;
                 i < transform.childCount;
                 i++)
            {
                DumpTransform(
                    transform.GetChild(i),
                    depth + 1,
                    maxDepth
                );
            }
        }


        private static Transform FindChildRecursive(
            Transform parent,
            string name)
        {
            if (parent == null)
            {
                return null;
            }


            for (int i = 0;
                 i < parent.childCount;
                 i++)
            {
                Transform child =
                    parent.GetChild(i);

                if (child.name == name)
                {
                    return child;
                }


                Transform result =
                    FindChildRecursive(
                        child,
                        name
                    );

                if (result != null)
                {
                    return result;
                }
            }


            return null;
        }


        private static string GetComponentNames(
            GameObject obj)
        {
            Component[] components =
                obj.GetComponents<Component>();

            string result =
                "";


            for (int i = 0;
                 i < components.Length;
                 i++)
            {
                Component component =
                    components[i];

                if (component == null)
                {
                    continue;
                }


                if (result.Length > 0)
                {
                    result += ",";
                }


                result +=
                    component
                        .GetType()
                        .Name;
            }


            return result;
        }
    }
}