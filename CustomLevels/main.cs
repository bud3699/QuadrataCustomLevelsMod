using MelonLoader;
using HarmonyLib;
using System.Reflection;

[assembly: MelonInfo(typeof(QuadrataPatcher.Main), "Custom Levels", "0.1.0", "Bud3699")]
[assembly: MelonGame("Mindlabor", "Quadrata")]

namespace QuadrataPatcher
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Mod Starting!");

            var harmony = new HarmonyLib.Harmony("com.bud3699.quadrata.patch");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            MelonLogger.Msg("Harmony initialized!");

            DirectorPatches.ApplyPatch(harmony);
            MelonLogger.Msg("Manual patch for Director.Init applied!");
        }
    }
}
