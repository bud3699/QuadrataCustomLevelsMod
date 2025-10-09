using System;
using System.IO;
using System.Net;
using UnityEngine;
using HarmonyLib;

namespace QuadrataPatcher
{
    public static class LevelGameBuilderExtensions
    {
        public static void InitCustomLevelFromCode(this object instance, string levelCode, out int currentLevelIndex, out object currentGameLevel)
        {
            currentLevelIndex = 90;
            currentGameLevel = null;

            try
            {
                string url = "http://bud.mynetgear.com/quadrata/api/levels/code/" + levelCode;
                Debug.Log($"<color=yellow>Fetching level from server: {url}</color>");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                string json;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("<color=red>Empty level JSON received!</color>");
                    return;
                }

                GameLevelData levelData = JsonUtility.FromJson<GameLevelData>(json);
                if (levelData == null)
                {
                    Debug.LogError("<color=red>Failed to parse level JSON!</color>");
                    return;
                }

                Type gameLevelType = AccessTools.TypeByName("GameLevel");
                if (gameLevelType == null)
                {
                    Debug.LogError("<color=red>Could not find type GameLevel!</color>");
                    return;
                }

                object gameLevel = ScriptableObject.CreateInstance(gameLevelType);

                Traverse gameLevelTraverse = Traverse.Create(gameLevel);
                gameLevelTraverse.Field("characterPositions").SetValue(levelData.characterPositions);
                gameLevelTraverse.Field("entityData").SetValue(levelData.entityData.Replace("\r", ""));
                gameLevelTraverse.Field("moveCount").SetValue(levelData.moveCount);
                gameLevelTraverse.Field("reversed").SetValue(levelData.reversed);

                Traverse traverse = Traverse.Create(instance);
                traverse.Field("customLevel").SetValue(true);
                traverse.Field("customGameLevel").SetValue(gameLevel);
                traverse.Field("currentScene").SetValue(false);

                traverse.Method("ResetScene").GetValue();
                traverse.Method("LoadLevel", new object[] { gameLevel }).GetValue();

                currentGameLevel = gameLevel;

                Debug.Log($"<color=green>Custom level loaded from code '{levelCode}' with index {currentLevelIndex}!</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>Failed to fetch or load level: {ex.Message}</color>");
            }
        }
    }
}
