using HarmonyLib;
using Mindlabor.Utils;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Linq;
using System;

namespace QuadrataPatcher
{
    public static class DirectorPatches
    {
        private static string customLevelCode;

        public static void ApplyPatch(HarmonyLib.Harmony harmony)
        {
            ParseCommandLineArgs();

            var original = AccessTools.Method(typeof(Director), "Init");
            var transpiler = AccessTools.Method(typeof(DirectorPatches), nameof(InitTranspiler));
            harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));

            var originalVisuals = AccessTools.Method(typeof(Director), "InitVisuals");
            var prefixVisuals = AccessTools.Method(typeof(DirectorPatches), nameof(InitVisualsPrefix));
            harmony.Patch(originalVisuals, prefix: new HarmonyMethod(prefixVisuals));

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

        public static IEnumerable<CodeInstruction> InitTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var gameModeField = AccessTools.Field(typeof(Director), "gameMode");
            var loadCustomMethod = AccessTools.Method(typeof(DirectorExtensions), "LoadCustomLevelFromCode");
            var customLevelCodeField = AccessTools.Field(typeof(DirectorPatches), "customLevelCode");
            var toStringMethod = AccessTools.Method(typeof(object), "ToString");
            var stringEqualsMethod = AccessTools.Method(typeof(string), "Equals", new[] { typeof(string), typeof(string) });

            /*
            if (!string.IsNullOrEmpty(customLevelCode))
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_3);
                yield return new CodeInstruction(OpCodes.Stsfld, gameModeField);
            }
            */

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];

                if (code.opcode == OpCodes.Call && code.operand is MethodInfo method && method.Name == "Log")
                {
                    var skipLabel = new Label();
                    code.labels.Add(skipLabel);

                    if (!string.IsNullOrEmpty(customLevelCode))
                    {
                        yield return new CodeInstruction(OpCodes.Ldsfld, gameModeField);
                        yield return new CodeInstruction(OpCodes.Box, typeof(GameMode));
                        yield return new CodeInstruction(OpCodes.Callvirt, toStringMethod);
                        yield return new CodeInstruction(OpCodes.Ldstr, "3");
                        yield return new CodeInstruction(OpCodes.Call, stringEqualsMethod);
                        yield return new CodeInstruction(OpCodes.Brfalse, skipLabel);

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldsfld, customLevelCodeField);
                        yield return new CodeInstruction(OpCodes.Call, loadCustomMethod);
                    }
                }

                yield return code;
            }
        }

        [HarmonyPrefix]
        public static bool InitVisualsPrefix(object __instance)
        {
            DirectorExtensions.SafeInitVisuals(__instance);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Director), "Init")]
        public static void PostInitSetGameMode()
        {
            if (!string.IsNullOrEmpty(customLevelCode))
            {
                Director.gameMode = (GameMode)3;
                Debug.Log("[Patch] ✅ Custom GameMode set to 3 via Postfix in Director.Init");
            }
        }




    }
}
