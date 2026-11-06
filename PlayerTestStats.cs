
// Cat Quest III Randomizer
// File: PlayerTestStats.cs
// Version: PlayerTestStats_20261105_2034_EST_v07
// Purpose: Press F8 to set player combat stats to 999999.
// Strength, Magic and Defense.
// No persistence or continuous updates.

using System;
using System.Reflection;
using UnityEngine;

namespace Randomizer.CatQuest3
{
    internal sealed class PlayerTestStats : MonoBehaviour
    {
        private const int TargetValue = 999999;

        private const BindingFlags Flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        private static PlayerTestStats instance;

        internal static void Initialize()
        {
            if (instance != null)
                return;

            GameObject go = new GameObject(
                "Randomizer_PlayerTestStats"
            );

            DontDestroyOnLoad(go);
            instance = go.AddComponent<PlayerTestStats>();
        }

        private void Awake()
        {
            Plugin.Log.LogInfo(
                "TEST STATS v07 | Ready. F8 boosts STR, MAG, DEF."
            );
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                Apply();
        }

        private static void Apply()
        {
            try
            {
                GameEntity player =
                    Contexts.sharedInstance.game
                        .GetEntityWithPlayerId(0);

                if (player == null || !player.hasCombatAgent)
                {
                    Plugin.Log.LogWarning(
                        "TEST STATS | Player combat agent unavailable."
                    );
                    return;
                }

                object agent = player.combatAgent.value;

                if (agent == null)
                {
                    Plugin.Log.LogWarning(
                        "TEST STATS | Combat agent is NULL."
                    );
                    return;
                }

                object stats = FindCombatStats(agent);

                if (stats == null)
                {
                    Plugin.Log.LogWarning(
                        "TEST STATS | CombatStats not found on " +
                        agent.GetType().FullName
                    );

                    LogPotentialMembers(agent);
                    return;
                }

                Plugin.Log.LogWarning(
                    "TEST STATS | ===== F8 START ====="
                );

                SetStat(stats, "strStat", "Strength");
                SetStat(stats, "magicStat", "Magic");
                SetStat(stats, "defStat", "Defense");

                Plugin.Log.LogWarning(
                    "TEST STATS | ===== F8 END ====="
                );
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "TEST STATS | Error: " + ex
                );
            }
        }

        private static void SetStat(
            object stats,
            string fieldName,
            string label)
        {
            object stat = ReadMember(stats, fieldName);

            if (stat == null)
            {
                Plugin.Log.LogWarning(
                    "TEST STATS | " + label +
                    " unavailable | Field:" + fieldName
                );
                return;
            }

            object before = ReadMember(stat, "baseValue");

            if (!WriteMember(stat, "baseValue", TargetValue))
            {
                Plugin.Log.LogWarning(
                    "TEST STATS | Could not write " +
                    label + ".baseValue"
                );
                return;
            }

            object after = ReadMember(stat, "baseValue");

            Plugin.Log.LogWarning(
                "TEST STATS | " + label +
                " | Before:" + before +
                " | After:" + after
            );
        }

        private static object FindCombatStats(object agent)
        {
            Type type = agent.GetType();

            foreach (FieldInfo field in type.GetFields(Flags))
            {
                if (field.FieldType.Name == "CombatStats")
                    return field.GetValue(agent);
            }

            foreach (PropertyInfo property in type.GetProperties(Flags))
            {
                if (property.PropertyType.Name != "CombatStats" ||
                    !property.CanRead ||
                    property.GetIndexParameters().Length != 0)
                    continue;

                return property.GetValue(agent, null);
            }

            return null;
        }

        private static object ReadMember(
            object source,
            string name)
        {
            Type type = source.GetType();

            FieldInfo field = type.GetField(name, Flags);

            if (field != null)
                return field.GetValue(source);

            PropertyInfo property = type.GetProperty(name, Flags);

            if (property != null &&
                property.CanRead &&
                property.GetIndexParameters().Length == 0)
            {
                return property.GetValue(source, null);
            }

            return null;
        }

        private static bool WriteMember(
            object source,
            string name,
            int value)
        {
            Type type = source.GetType();

            FieldInfo field = type.GetField(name, Flags);

            if (field != null &&
                !field.IsInitOnly &&
                !field.IsStatic)
            {
                object converted = Convert.ChangeType(
                    value,
                    field.FieldType
                );

                field.SetValue(source, converted);
                return true;
            }

            PropertyInfo property = type.GetProperty(name, Flags);

            if (property != null &&
                property.CanWrite &&
                property.GetIndexParameters().Length == 0)
            {
                object converted = Convert.ChangeType(
                    value,
                    property.PropertyType
                );

                property.SetValue(source, converted, null);
                return true;
            }

            return false;
        }

        private static void LogPotentialMembers(object agent)
        {
            Type type = agent.GetType();

            foreach (FieldInfo field in type.GetFields(Flags))
            {
                if (field.Name.ToLowerInvariant().Contains("stat"))
                {
                    Plugin.Log.LogInfo(
                        "TEST STATS | Candidate field:" +
                        field.Name +
                        " | Type:" + field.FieldType.FullName
                    );
                }
            }

            foreach (PropertyInfo property in type.GetProperties(Flags))
            {
                if (property.Name.ToLowerInvariant().Contains("stat"))
                {
                    Plugin.Log.LogInfo(
                        "TEST STATS | Candidate property:" +
                        property.Name +
                        " | Type:" + property.PropertyType.FullName
                    );
                }
            }
        }
    }
}
