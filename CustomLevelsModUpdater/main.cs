using MelonLoader;
using System.Threading;

[assembly: MelonInfo(typeof(CustomLevelsModUpdater.Main), "Custom Levels Mod Updater", "1.0.0", "Bud3699")]
namespace CustomLevelsModUpdater
{
    public class Main : MelonPlugin
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Custom Levels ModUpdater initialized.");
            Thread updaterThread = new Thread(GitHubChecker.CheckForUpdates);
            updaterThread.IsBackground = true;
            updaterThread.Start();
        }
    }
}
