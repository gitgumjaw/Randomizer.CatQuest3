using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(PlayerEnableFloat), "OnEnter")]
    public static class PlayerEnableFloatPatch
    {
        public static void Prefix(PlayerEnableFloat __instance)
        {
            Plugin.Log.LogInfo(
                $"Float ENABLED at: {PlayMakerLocation.GetKey(__instance.Fsm)}"
            );
        }
    }

    [HarmonyPatch(typeof(PlayerDisableFloat), "OnEnter")]
    public static class PlayerDisableFloatPatch
    {
        public static void Prefix(PlayerDisableFloat __instance)
        {
            Plugin.Log.LogInfo(
                $"Float DISABLED at: {PlayMakerLocation.GetKey(__instance.Fsm)}"
            );
        }
    }
}