using HarmonyLib;
using UnityEngine;
using System.Linq;
using MelonLoader;


namespace QuadrataPatcher
{
    public static class DirectorPatches
    {
        private static string customLevelCode;

        public static void ApplyPatch(HarmonyLib.Harmony harmony)
        {
            ParseCommandLineArgs();

            var directorType = AccessTools.TypeByName("Director");
            if (directorType == null)
            {
                Debug.LogError("[Patch] Could not find Director type!");
                return;
            }

            var initGameModeMethod = AccessTools.Method(directorType, "InitGameMode");
            if (initGameModeMethod == null)
            {
                Debug.LogError("[Patch] Could not find InitGameMode method!");
                return;
            }

            harmony.Patch(initGameModeMethod, prefix: new HarmonyMethod(typeof(DirectorPatches).GetMethod(nameof(InitGameModePrefix))));
            Debug.Log("[Patch] Director.InitGameMode patched with Prefix!");
        }

        private static void ParseCommandLineArgs()
        {
            var args = System.Environment.GetCommandLineArgs();
            int codeIndex = args.ToList().FindIndex(a => a.Equals("-code", System.StringComparison.OrdinalIgnoreCase));

            if (codeIndex >= 0 && codeIndex < args.Length - 1)
            {
                customLevelCode = args[codeIndex + 1];
                Debug.Log($"[Patch] Found -code argument: {customLevelCode}");
            }
            else
            {
                Debug.Log("[Patch] No -code argument found; skipping custom level load.");
            }
        }

        public static bool InitGameModePrefix(object __instance)
        {
            if (!string.IsNullOrEmpty(customLevelCode))
            {
                Debug.Log($"[Patch] Loading custom level from code: {customLevelCode}");
                DirectorExtensions.LoadCustomLevelFromCode(__instance, customLevelCode);
                return false; 
            }

            return true; 
        }
    }
}
