using Discord;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

public static class DiscordManagerPatches
{
    public static string LevelCodeDiscord;
    public static void ApplyPatch(HarmonyLib.Harmony harmony)
    {
        var originalFixedUpdate = AccessTools.Method(typeof(DiscordManager), "FixedUpdate");
        var prefixFixedUpdate = AccessTools.Method(typeof(DiscordManagerPatches), nameof(FixedUpdatePrefix));
        harmony.Patch(originalFixedUpdate, prefix: new HarmonyMethod(prefixFixedUpdate));

        var originalCreatePresence = AccessTools.Method(typeof(DiscordManager), "CreatePresence");
        var prefixCreatePresence = AccessTools.Method(typeof(DiscordManagerPatches), nameof(CreatePresencePrefix));
        harmony.Patch(originalCreatePresence, prefix: new HarmonyMethod(prefixCreatePresence));
    }

    public static bool CreatePresencePrefix(DiscordManager __instance)
    {
        try
        {
            var discord = new global::Discord.Discord(1431736823100608655, 1uL);
            var activityManager = discord.GetActivityManager();

            AccessTools.Field(typeof(DiscordManager), "discord").SetValue(__instance, discord);
            AccessTools.Field(typeof(DiscordManager), "activityManager").SetValue(__instance, activityManager);
            AccessTools.Field(typeof(DiscordManager), "hasStarted").SetValue(__instance, true);
        }
        catch (ResultException)
        {
            AccessTools.Field(typeof(DiscordManager), "hasErrored").SetValue(__instance, true);
        }

        return false;
    }

    public static bool FixedUpdatePrefix(DiscordManager __instance)
    {
        if (Time.frameCount % 20 != 0)
        {
            return false;
        }

        var hasStartedField = AccessTools.Field(typeof(DiscordManager), "hasStarted");
        var hasStarted = (bool)hasStartedField.GetValue(__instance);
        if (!hasStarted)
        {
            AccessTools.Method(typeof(DiscordManager), "CreatePresence").Invoke(__instance, null);
        }

        string levelString;
        string stateText = "Falling through the Matrix";

        if (Director.gameMode == GameMode.Game)
        {
            levelString = (SaveManager.lastLevel == "DemoLastLevelIndex$") ? "Demo" : "Level";
            stateText = $"{levelString} {LevelManager.levelIndex:D2}";
        }
        if (Director.gameMode == GameMode.SandboxEdit || Director.gameMode == GameMode.SandboxPlay)
        {
            stateText = "Level Editor";
        }
        if (Director.gameMode.ToString() == "3")
        {
            if (string.IsNullOrEmpty(LevelCodeDiscord))
            {
                stateText = "Loading Custom Level...";
            }
            else
            {
                stateText = $"Playing Custom Level: {LevelCodeDiscord}";
            }
        }
        if (LevelManager.levelIndex == 0)
        {
            stateText = "Opening";
        }

        AccessTools.Field(typeof(DiscordManager), "hasErrored").SetValue(__instance, false);
        var hasErrored = (bool)AccessTools.Field(typeof(DiscordManager), "hasErrored").GetValue(__instance);
        if (hasErrored)
        {
            return false;
        }

        try
        {
            var discord = (global::Discord.Discord)AccessTools.Field(typeof(DiscordManager), "discord").GetValue(__instance);
            discord?.RunCallbacks();

            AccessTools.Method(typeof(DiscordManager), "UpdatePresence").Invoke(__instance, new object[] { stateText });
        }
        catch (NullReferenceException)
        {
            AccessTools.Field(typeof(DiscordManager), "hasErrored").SetValue(__instance, true);
        }
        catch (ResultException)
        {
            AccessTools.Field(typeof(DiscordManager), "hasErrored").SetValue(__instance, true);
        }

        return false;
    }
}
