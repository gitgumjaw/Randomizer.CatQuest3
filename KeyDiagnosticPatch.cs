using HarmonyLib;
using HutongGames.PlayMaker;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(
        typeof(QuestBehaviour),
        "OnFsmStateChange"
    )]
    public static class QuestStateDiagnosticPatch
    {
        public static void Prefix(
            QuestBehaviour __instance,
            FsmState fsmState)
        {
            if (__instance == null ||
                __instance.questData == null ||
                __instance.fsm == null ||
                fsmState == null)
            {
                return;
            }

            string questGuid =
                __instance.questData.Guid;

            if (questGuid !=
                "de381dd8f51fb2648ac3520111f5331b")
            {
                return;
            }

            string previousState =
                __instance.fsm.Fsm.PreviousActiveState?.Name
                ?? "<none>";

            Plugin.Log.LogInfo(
                $"SMALL SQUID FSM | " +
                $"Previous:{previousState} | " +
                $"Current:{fsmState.Name}"
            );
        }
    }
}