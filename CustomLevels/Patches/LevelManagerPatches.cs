using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Collections;
using Mindlabor.Utils;

namespace QuadrataPatcher
{
    public static class LevelManagerPatches
    {
        public static void ApplyPatch(HarmonyLib.Harmony harmony)
        {
            var original = AccessTools.Method(typeof(LevelManager), "CollectDiamondCoroutine", new[] {
                typeof(AudioSourceSettings),
                typeof(AudioSourceSettings),
                typeof(int),
                typeof(AudioSourceSettings)
            });

            var transpiler = AccessTools.Method(typeof(LevelManagerPatches), nameof(CollectDiamondCoroutineTranspiler));
            var prefix = AccessTools.Method(typeof(LevelManagerPatches), nameof(CollectDiamondCoroutinePrefix));

            harmony.Patch(original, transpiler: new HarmonyMethod(transpiler), prefix: new HarmonyMethod(prefix));

        }

        public static IEnumerable<CodeInstruction> CollectDiamondCoroutineTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var gameModeField = AccessTools.Field(typeof(Director), "gameMode");
            var toStringMethod = AccessTools.Method(typeof(object), "ToString");
            var stringEqualsMethod = AccessTools.Method(typeof(string), "Equals", new[] { typeof(string), typeof(string) });
            var debugLogMethod = AccessTools.Method(typeof(Debug), "Log", new[] { typeof(object) });

            var gameModeEnum = typeof(GameMode);
            var gameModeGame = (int)System.Enum.Parse(gameModeEnum, "Game");

            var modified = false;

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];

                if (!modified &&
                    code.opcode == OpCodes.Ldsfld && code.operand == gameModeField &&
                    i + 1 < codes.Count &&
                    codes[i + 1].opcode == OpCodes.Ldc_I4 && (int)codes[i + 1].operand == gameModeGame)
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, "Checking if gameMode == Game or ToString == \"3\"");
                    yield return new CodeInstruction(OpCodes.Call, debugLogMethod);

                    yield return new CodeInstruction(OpCodes.Ldsfld, gameModeField);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, gameModeGame);
                    yield return new CodeInstruction(OpCodes.Ceq);

                    yield return new CodeInstruction(OpCodes.Ldsfld, gameModeField);
                    yield return new CodeInstruction(OpCodes.Box, typeof(GameMode));
                    yield return new CodeInstruction(OpCodes.Callvirt, toStringMethod);
                    yield return new CodeInstruction(OpCodes.Ldstr, "3");
                    yield return new CodeInstruction(OpCodes.Call, stringEqualsMethod);

                    yield return new CodeInstruction(OpCodes.Or);

                    yield return new CodeInstruction(OpCodes.Ldstr, "Matched gameMode == Game or ToString == \"3\"");
                    yield return new CodeInstruction(OpCodes.Call, debugLogMethod);

                    modified = true;
                    i += 2;
                    continue;
                }

                if (code.opcode == OpCodes.Call &&
                    code.operand is MethodInfo method &&
                    method.Name == "SetInt")
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, gameModeField);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, gameModeGame);
                    yield return new CodeInstruction(OpCodes.Ceq);

                    Label skipLog = codes[i].labels.Count > 0 ? codes[i].labels[0] : new Label();
                    var branch = new CodeInstruction(OpCodes.Brfalse, skipLog);
                    yield return branch;

                    yield return new CodeInstruction(OpCodes.Ldstr, "Saving game");
                    yield return new CodeInstruction(OpCodes.Call, debugLogMethod);

                    continue;
                }

                yield return code;
            }
        }

        [HarmonyPrefix]
        public static bool CollectDiamondCoroutinePrefix(
            AudioSourceSettings firstDiamond,
            AudioSourceSettings secondDiamond,
            int side,
            AudioSourceSettings success,
            ref IEnumerator __result)
        {
            __result = LevelManagerExtension.HandleGameModeLevelLoad(firstDiamond, secondDiamond, side, success);
            return false;
        }
    }
}
