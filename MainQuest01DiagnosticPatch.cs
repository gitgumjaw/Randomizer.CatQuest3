using HarmonyLib;
using HutongGames.PlayMaker;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(QuestBehaviour),
        "OnFsmStateChange"
    )]
    public static class MainQuest01TransitionDiagnosticPatch
    {
        public static void Postfix(
            QuestBehaviour __instance,
            FsmState fsmState)
        {
            if (
                __instance == null ||
                __instance.gameObject == null ||
                __instance.gameObject.name !=
                    "MainQuest_01" ||
                fsmState == null
            )
            {
                return;
            }

            string stateName =
                fsmState.Name;

            if (
                stateName != "Defeated Mr Clean" &&
                stateName != "Dock Tutorial" &&
                stateName != "Talk to Squeaky" &&
                stateName != "Enter Ship" &&
                stateName != "Ship Tutorial" &&
                stateName != "Quest Title"
            )
            {
                return;
            }

            Plugin.Log.LogInfo(
                $"QUEST TRANSITIONS | " +
                $"State:{stateName}"
            );

            FsmTransition[] transitions =
                fsmState.Transitions;

            if (
                transitions == null ||
                transitions.Length == 0
            )
            {
                Plugin.Log.LogInfo(
                    "QUEST TRANSITIONS | " +
                    "  No transitions."
                );

                return;
            }

            for (
                int i = 0;
                i < transitions.Length;
                i++)
            {
                FsmTransition transition =
                    transitions[i];

                if (transition == null)
                {
                    Plugin.Log.LogInfo(
                        $"QUEST TRANSITIONS | " +
                        $"  Transition {i}: null"
                    );

                    continue;
                }

                string eventName =
                    transition.FsmEvent != null
                        ? transition.FsmEvent.Name
                        : "<null>";

                Plugin.Log.LogInfo(
                    $"QUEST TRANSITIONS | " +
                    $"  {i}: " +
                    $"Event:{eventName} | " +
                    $"To:{transition.ToState}"
                );
            }
        }
    }
}