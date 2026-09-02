using HarmonyLib;

namespace Randomizer.CatQuest3
{
    [HarmonyPatch(typeof(LootDropHelper), "DropKey")]
    public static class DropKeyPatch
    {
        public static void Prefix(KeyData key)
        {
            if (key == null)
            {
                return;
            }

            Plugin.Log.LogInfo(
                $"Direct KeyData Award: {key.Guid}"
            );
        }
    }
}