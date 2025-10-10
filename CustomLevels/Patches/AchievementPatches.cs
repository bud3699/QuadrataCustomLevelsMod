using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace QuadrataPatcher
{
    public static class AchievementPatches
    {
        public static void ApplyPatch(HarmonyLib.Harmony harmony)
        {
            var original = AccessTools.Method(typeof(AchievementManager), nameof(AchievementManager.Achieve));
            var transpiler = AccessTools.Method(typeof(AchievementPatches), nameof(AchieveTranspiler));
            harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
        }

        public static IEnumerable<CodeInstruction> AchieveTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var levelBuilderField = AccessTools.Field(typeof(Director), "levelBuilder");
            var customLevelField = AccessTools.Field(typeof(LevelGameBuilder), "customLevel");
            var directorInstanceGetter = AccessTools.PropertyGetter(typeof(Director), "instance");

            var debugLogMethod = AccessTools.Method(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.Log), new[] { typeof(object) });

            var newInstructions = new List<CodeInstruction>();

            var continueLabel = new Label();

            newInstructions.Add(new CodeInstruction(OpCodes.Call, directorInstanceGetter));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, levelBuilderField));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, customLevelField));
            newInstructions.Add(new CodeInstruction(OpCodes.Brfalse_S, continueLabel));
            newInstructions.Add(new CodeInstruction(OpCodes.Ldstr, "blocked achievement"));
            newInstructions.Add(new CodeInstruction(OpCodes.Call, debugLogMethod));
            newInstructions.Add(new CodeInstruction(OpCodes.Ret));

            var instructionList = instructions.ToList();

            if (instructionList.Count > 0)
            {
                instructionList[0].labels.Add(continueLabel);
            }

            foreach (var instr in newInstructions)
                yield return instr;

            foreach (var instr in instructionList)
                yield return instr;
        }

    }
}
