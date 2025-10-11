using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace CustomLevelsModUpdater
{
    public static class GitHubChecker
    {
        private const string RepoOwner = "bud3699";
        private const string RepoName = "QuadrataCustomLevelsMod";
        private const string ModName = "Custom Levels";

        public static void CheckForUpdates()
        {
            try
            {
                string installedVersion = GetInstalledModVersion(ModName);
                if (installedVersion == null)
                {
                    MelonLogger.Warning($"Mod '{ModName}' not found. Skipping update check.");
                    return;
                }

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "ModUpdater");

                    string url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases";
                    MelonLogger.Msg($"Checking: {url}");
                    string json = client.DownloadString(url);
                    JArray releases = JArray.Parse(json);

                    foreach (var release in releases)
                    {
                        string latestTag = release["tag_name"]?.ToString();
                        bool isPreRelease = release["prerelease"]?.ToObject<bool>() ?? false;

                        string normalizedTag = latestTag?.StartsWith("v") == true ? latestTag.Substring(1) : latestTag;

                        if (normalizedTag != null)
                        {
                            MelonLogger.Msg($"Installed version: {installedVersion}, Latest tag: {normalizedTag}");

                            try
                            {
                                Version latest = new Version(normalizedTag);
                                Version installed = new Version(installedVersion);

                                if (latest > installed)
                                {
                                    MelonLogger.Msg($"Pre-release update detected: {normalizedTag}");
                                    DownloadRelease((JObject)release);
                                }
                                else
                                {
                                    MelonLogger.Msg("No update needed — already up to date.");
                                }
                            }
                            catch (Exception ex)
                            {
                                MelonLogger.Warning($"Version comparison failed: {ex.Message}");
                            }

                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Update check failed: {ex.Message}");
            }
        }

        private static string GetInstalledModVersion(string modName)
        {
            foreach (var melon in MelonMod.RegisteredMelons)
            {
                if (melon.Info.Name == modName)
                    return melon.Info.Version;
            }
            return null;
        }

        private static void DownloadRelease(JObject release)
        {
            System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;

            var assets = release["assets"];
            if (assets == null) return;

            foreach (var asset in assets)
            {
                string name = asset["name"]?.ToString();
                string url = asset["browser_download_url"]?.ToString();

                if (name != null && name.EndsWith(".dll") && url != null)
                {
                    string modPath = Path.Combine(MelonEnvironment.ModsDirectory, name);
                    MelonLogger.Msg($"Attempting to download to: {modPath}");

                    try
                    {
                        if (File.Exists(modPath))
                        {
                            MelonLogger.Msg($"Deleting existing file: {modPath}");
                            File.Delete(modPath);
                        }

                        using (var client = new WebClient())
                        {
                            client.Headers.Add("User-Agent", "ModUpdater");
                            client.Headers.Add("Accept", "application/octet-stream");

                            client.DownloadFile(url, modPath);
                            MelonLogger.Msg($"Updated mod downloaded: {name}");
                            MelonLogger.Msg("Update applied. Please restart the game to load the new version.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Download failed for {name}: {ex.Message}");
                    }
                }
            }
        }
    }
}
