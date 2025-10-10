using HarmonyLib;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

namespace QuadrataPatcher
{
    [HarmonyPatch]
    public static class LayerMenuPatch
    {
        [HarmonyPatch(typeof(LayerMenu), "ChangeLevelSelectSize")]
        [HarmonyPrefix]
        public static bool Prefix_LayerMenu_ChangeLevelSelectSize(object __instance, Vector2 size, float time, Ease ease)
        {
            __instance.ChangeLevelSelectSizeExtended(size, time, ease);
            return false;
        }

        [HarmonyPatch(typeof(LayerMenu), "CalculateMenu")]
        [HarmonyPrefix]
        public static bool Prefix_LayerMenu_CalculateMenu(object __instance)
        {
            __instance.CalculateMenuExtended();
            return false;
        }

        public static void ApplyPatch(HarmonyLib.Harmony harmony)
        {
            var originalStart = AccessTools.Method(typeof(LayerMenu), "Start");
            var transpilerStart = AccessTools.Method(typeof(LayerMenuPatch), nameof(StartTranspiler));
            harmony.Patch(originalStart, transpiler: new HarmonyMethod(transpilerStart));
        }

        public static IEnumerable<CodeInstruction> StartTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var gameModeField = AccessTools.Field(typeof(Director), "gameMode");

            yield return new CodeInstruction(OpCodes.Ldc_I4_3);
            yield return new CodeInstruction(OpCodes.Stsfld, gameModeField);

            foreach (var code in codes)
            {
                yield return code;
            }
        }
    }
}
