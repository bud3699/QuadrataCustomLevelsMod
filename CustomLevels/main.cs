using MelonLoader;
using HarmonyLib;
using System.Reflection;
using CustomLevels;

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
            MelonLogger.Msg("Manual patch for Director applied!");

            LevelManagerPatches.ApplyPatch(harmony);
            MelonLogger.Msg("Patched Level Manger to allow gameplay");

            MenuButtonSandboxPatches.ApplyPatch(harmony);
            MelonLogger.Msg("Patched MenuButtonSandbox to allow icon changes");

            string[] args = System.Environment.GetCommandLineArgs();
            bool patchedLayerMenu = false;

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-code" && !string.IsNullOrWhiteSpace(args[i + 1]))
                {
                    LayerMenuPatch.ApplyPatch(harmony);
                    MelonLogger.Msg("Patched LayerMenu");
                    patchedLayerMenu = true;
                    break;
                }
            }

            if (!patchedLayerMenu)
            {
                MelonLogger.Msg("Continuing with regular boot ");
            }
        }
    }
}
