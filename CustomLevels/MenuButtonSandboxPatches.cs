using HarmonyLib;

namespace CustomLevels
{
    [HarmonyPatch(typeof(MenuButtonSandbox), "Awake")]
    internal static class MenuButtonSandboxPatches
    {
        static bool Prefix(MenuButtonSandbox __instance)
        {
            MenuButtonSandboxExtensions.ToggleMappingSet(__instance);
            return false;
        }

        public static void ApplyPatch(HarmonyLib.Harmony harmony)
        {
            var originalAwake = AccessTools.Method(typeof(MenuButtonSandbox), "Awake");
            var prefix = AccessTools.Method(typeof(MenuButtonSandboxPatches), nameof(Prefix));
            harmony.Patch(originalAwake, prefix: new HarmonyMethod(prefix));
        }
    }
}
