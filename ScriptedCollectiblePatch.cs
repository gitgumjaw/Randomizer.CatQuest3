using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(SpawnLootItem), "OnEnter")]
    public static class ScriptedCollectiblePatch
    {
        public static void Prefix(SpawnLootItem __instance)
        {
            string locationKey =
                PlayMakerLocation.GetKey(__instance.Fsm);

            Plugin.Log.LogInfo(
                $"Scripted Collectible: {locationKey} | " +
                $"Type: {__instance.collectibleType} | " +
                $"Quantity: {__instance.quantity} | " +
                $"Value: {__instance.value}"
            );
        }
    }
}