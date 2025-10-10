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

            var newInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Call, directorInstanceGetter),
                new CodeInstruction(OpCodes.Ldfld, levelBuilderField),
                new CodeInstruction(OpCodes.Ldfld, customLevelField),
                new CodeInstruction(OpCodes.Brfalse_S, instructions.First().labels.FirstOrDefault()),
                new CodeInstruction(OpCodes.Ret) 
            };

            foreach (var instr in newInstructions)
                yield return instr;

            foreach (var instr in instructions)
                yield return instr;
        }
    }
}
