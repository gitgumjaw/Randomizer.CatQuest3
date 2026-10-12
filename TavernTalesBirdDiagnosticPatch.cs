using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch]
    public static class
        TavernTalesBirdDiagnosticPatch
    {
        private static MethodBase[] targetMethods;


        public static System.Collections.Generic.IEnumerable<
            MethodBase
        > TargetMethods()
        {
            Type checkType =
                AccessTools.TypeByName(
                    "TavernTalesCheckIfCompanionImproved"
                );

            Type rewardType =
                AccessTools.TypeByName(
                    "TavernTalesShowCompanionReward"
                );


            if (checkType != null)
            {
                MethodInfo method =
                    AccessTools.Method(
                        checkType,
                        "OnEnter"
                    );

                if (method != null)
                {
                    yield return method;
                }
            }


            if (rewardType != null)
            {
                MethodInfo method =
                    AccessTools.Method(
                        rewardType,
                        "OnEnter"
                    );

                if (method != null)
                {
                    yield return method;
                }
            }
        }


        [HarmonyPrefix]
        public static void Prefix(
            object __instance)
        {
            if (__instance == null)
            {
                return;
            }


            string typeName =
                __instance
                    .GetType()
                    .FullName;


            Plugin.Log.LogInfo(
                "TAVERN COMPANION DIAGNOSTIC | " +
                "ENTER | Type:" +
                typeName
            );


            LogObject(
                __instance,
                "Action",
                0
            );
        }


        [HarmonyPostfix]
        public static void Postfix(
            object __instance)
        {
            if (__instance == null)
            {
                return;
            }


            string typeName =
                __instance
                    .GetType()
                    .FullName;


            Plugin.Log.LogInfo(
                "TAVERN COMPANION DIAGNOSTIC | " +
                "EXIT | Type:" +
                typeName
            );


            LogObject(
                __instance,
                "ActionAfter",
                0
            );
        }


        private static void LogObject(
            object target,
            string label,
            int depth)
        {
            if (
                target == null ||
                depth > 1
            )
            {
                return;
            }


            Type currentType =
                target.GetType();


            Plugin.Log.LogInfo(
                "TAVERN COMPANION OBJECT | " +
                "Label:" +
                label +
                " | Type:" +
                currentType.FullName
            );


            Type scanType =
                currentType;


            while (
                scanType != null &&
                scanType != typeof(object)
            )
            {
                FieldInfo[] fields =
                    scanType.GetFields(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly
                    );


                foreach (
                    FieldInfo field
                    in fields)
                {
                    object value;


                    try
                    {
                        value =
                            field.GetValue(
                                target
                            );
                    }
                    catch
                    {
                        continue;
                    }


                    Plugin.Log.LogInfo(
                        "TAVERN COMPANION FIELD | " +
                        "Label:" +
                        label +
                        " | DeclaredBy:" +
                        scanType.Name +
                        " | Field:" +
                        field.Name +
                        " | Type:" +
                        field.FieldType.FullName +
                        " | Value:" +
                        FormatValue(
                            value
                        )
                    );


                    if (
                        depth == 0 &&
                        ShouldInspectReferencedObject(
                            field,
                            value
                        )
                    )
                    {
                        LogObject(
                            value,
                            label +
                            "." +
                            field.Name,
                            depth + 1
                        );
                    }
                }


                PropertyInfo[] properties =
                    scanType.GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly
                    );


                foreach (
                    PropertyInfo property
                    in properties)
                {
                    if (
                        property.GetIndexParameters()
                            .Length != 0
                    )
                    {
                        continue;
                    }


                    MethodInfo getter =
                        property.GetGetMethod(
                            true
                        );


                    if (getter == null)
                    {
                        continue;
                    }


                    object value;


                    try
                    {
                        value =
                            property.GetValue(
                                target,
                                null
                            );
                    }
                    catch
                    {
                        continue;
                    }


                    Plugin.Log.LogInfo(
                        "TAVERN COMPANION PROPERTY | " +
                        "Label:" +
                        label +
                        " | DeclaredBy:" +
                        scanType.Name +
                        " | Property:" +
                        property.Name +
                        " | Type:" +
                        property.PropertyType.FullName +
                        " | Value:" +
                        FormatValue(
                            value
                        )
                    );
                }


                scanType =
                    scanType.BaseType;
            }
        }


        private static bool
            ShouldInspectReferencedObject(
                FieldInfo field,
                object value)
        {
            if (
                field == null ||
                value == null
            )
            {
                return false;
            }


            Type type =
                value.GetType();


            if (
                type.IsPrimitive ||
                type.IsEnum ||
                value is string ||
                value is Array ||
                value is ICollection
            )
            {
                return false;
            }


            string fieldName =
                field.Name ??
                string.Empty;

            string typeName =
                type.FullName ??
                type.Name;


            return
                ContainsIgnoreCase(
                    fieldName,
                    "companion"
                ) ||
                ContainsIgnoreCase(
                    fieldName,
                    "manager"
                ) ||
                ContainsIgnoreCase(
                    fieldName,
                    "reward"
                ) ||
                ContainsIgnoreCase(
                    fieldName,
                    "encounter"
                ) ||
                ContainsIgnoreCase(
                    fieldName,
                    "tale"
                ) ||
                ContainsIgnoreCase(
                    typeName,
                    "companion"
                ) ||
                ContainsIgnoreCase(
                    typeName,
                    "tavern"
                ) ||
                ContainsIgnoreCase(
                    typeName,
                    "reward"
                );
        }


        private static string FormatValue(
            object value)
        {
            if (value == null)
            {
                return "NULL";
            }


            Type type =
                value.GetType();


            if (
                type.IsPrimitive ||
                type.IsEnum ||
                value is string ||
                value is decimal
            )
            {
                return value.ToString();
            }


            if (
                value is UnityEngine.Object
                    unityObject)
            {
                return
                    unityObject.name +
                    " [" +
                    unityObject
                        .GetType()
                        .Name +
                    "]";
            }


            if (value is Array array)
            {
                return
                    "Array Count:" +
                    array.Length;
            }


            if (
                value is ICollection
                    collection)
            {
                return
                    "Collection Count:" +
                    collection.Count;
            }


            return value.ToString();
        }


        private static bool ContainsIgnoreCase(
            string source,
            string value)
        {
            if (
                string.IsNullOrEmpty(source) ||
                string.IsNullOrEmpty(value)
            )
            {
                return false;
            }


            return
                source.IndexOf(
                    value,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }
    }
}