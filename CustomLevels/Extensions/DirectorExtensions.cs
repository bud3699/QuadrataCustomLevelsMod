using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using DG.Tweening;
using System.Linq;

namespace QuadrataPatcher
{
    public static class DirectorExtensions
    {
        public static void LoadCustomLevelFromCode(this object directorInstance, string LevelCode = "A6XEAX")
        {
            if (UIManager.instance == null)
            {
                Debug.LogWarning("<color=yellow>UIManager not ready — deferring custom level load...</color>");
                if (directorInstance is MonoBehaviour mono)
                    mono.StartCoroutine(DeferredLoadCustomLevel(directorInstance, LevelCode));
                else
                    Debug.LogError("Director instance is not a MonoBehaviour — cannot start coroutine!");
                return;
            }

            ExecuteLoadCustomLevel(directorInstance, LevelCode);
        }

        private static IEnumerator DeferredLoadCustomLevel(object directorInstance, string LevelCode)
        {
            float waitTime = 0f;
            while (UIManager.instance == null)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
            Debug.Log($"<color=lime>UIManager detected after {waitTime:F2}s — continuing custom level load...</color>");
            ExecuteLoadCustomLevel(directorInstance, LevelCode);
        }

        private static void ExecuteLoadCustomLevel(object directorInstance, string LevelCode)
        {
            Debug.Log("<color=red>Game Mode: Loading Custom Level</color>");
            Traverse traverse = Traverse.Create(directorInstance);

            object levelBuilder = traverse.Field("levelBuilder").GetValue();
            if (levelBuilder == null)
            {
                Debug.LogError("LevelGameBuilder reference is null!");
                return;
            }

            object gameLevel;
            levelBuilder.InitCustomLevelFromCode(LevelCode, out gameLevel);
            CustomLevelCompleteUI.gameLevelUpload = JsonUtility.ToJson(gameLevel); 

            if (gameLevel == null)
            {
                Debug.LogError("Failed to load custom level — aborting LoadCustomLevel!");
                return;
            }
            DiscordManagerPatches.LevelCodeDiscord = LevelCode;

            AccessTools.TypeByName("LevelNumber")?.GetMethod("ChangeLevelText")?.Invoke(null, new object[] { LevelCode });

            AccessTools.Method(directorInstance.GetType(), "InitCommonSystems")?.Invoke(directorInstance, null);

            object heartContainer = traverse.Field("heartContainer").GetValue();
            int moveCount = (int)Traverse.Create(gameLevel).Field("moveCount").GetValue();
            if (heartContainer != null)
            {
                AccessTools.Method(heartContainer.GetType(), "HideOperations")?.Invoke(heartContainer, null);
                AccessTools.Method(heartContainer.GetType(), "Init")?.Invoke(heartContainer, new object[] { moveCount });
            }
            else Debug.LogWarning("HeartContainer is null!");

            object gridLines = AccessTools.Field(AccessTools.TypeByName("GridLines"), "instance")?.GetValue(null);
            bool reversed = (bool)Traverse.Create(gameLevel).Field("reversed").GetValue();
            if (gridLines != null)
                AccessTools.Method(gridLines.GetType(), "SetReversedLine")?.Invoke(gridLines, new object[] { reversed });
            else
                Debug.LogWarning("GridLines instance is null!");

            object openingField = traverse.Field("opening").GetValue();
            AccessTools.Method(AccessTools.TypeByName("LevelAnimation"), "OpenLevel")?.Invoke(null, new object[] { openingField });

            traverse.Field("editingGameLevel").SetValue(null);

            AccessTools.Method(directorInstance.GetType(), "InitEntitiesAndCropGrid")?.Invoke(directorInstance, null);

            object movementManager = traverse.Field("movementManager").GetValue();
            if (movementManager != null)
                AccessTools.Method(movementManager.GetType(), "Init")?.Invoke(movementManager, new object[] { moveCount, reversed });
            else
                Debug.LogWarning("MovementManager is null!");

            object replayManager = traverse.Field("replayManager").GetValue();
            if (replayManager != null)
                AccessTools.Method(replayManager.GetType(), "Init")?.Invoke(replayManager, null);
            else
                Debug.LogWarning("ReplayManager is null!");

            AccessTools.Method(directorInstance.GetType(), "InitVisuals")?.Invoke(directorInstance, null);
        }


        public static void SafeInitVisuals(object directorInstance)
        {
            var traverse = Traverse.Create(directorInstance);
            var gameMode = Director.gameMode;  //(GameMode)traverse.Field("gameMode").GetValue();

            (float, int, bool, float) tuple;

            if (gameMode == (GameMode)3)
            {
                Debug.Log("[Patch] Using fallback tuple for GameMode 3");
                tuple = (0f, 1, false, 4f);
            }
            else if (SandboxStateMapping.mapping.TryGetValue(gameMode, out var mappedTuple))
            {
                Debug.Log("[Patch] gamemode: " + gameMode.ToString());
                tuple = mappedTuple;
            }
            else
            {
                Debug.LogWarning($"[Patch] Unknown GameMode: {gameMode}, using default fallback");
                tuple = (0.75f, 1, false, 4.5f);
            }



            CameraSizeManager.SetAdditiveSize(tuple.Item1);

            //var gridLines = AccessTools.Field(AccessTools.TypeByName("GridLines"), "instance")?.GetValue(null);
            GridLines.instance?.SetGameGrid(tuple.Item2);
            //if (gridLines != null)
            //{
                //AccessTools.Method(gridLines.GetType(), "SetGameGrid")?.Invoke(gridLines, new object[] { tuple.Item2 });

                if (tuple.Item3)
                {
                    //var selectedButton = AccessTools.Field(AccessTools.TypeByName("LayerSandbox"), "selectedButton")?.GetValue(null);
                    //bool willPlaceOut = selectedButton is SandboxButtonEntity entity && entity.willPlaceOut;
                    //AccessTools.Method(gridLines.GetType(), "ShowCantPlace")?.Invoke(gridLines, new object[] { willPlaceOut });
                    GridLines.instance?.ShowCantPlace(LayerSandbox.selectedButton is SandboxButtonEntity sandboxButtonEntity && sandboxButtonEntity.willPlaceOut);
                }
                else
                {
                    //AccessTools.Method(gridLines.GetType(), "ResetCantPlace")?.Invoke(gridLines, null);
                    GridLines.instance?.ResetCantPlace();
                } 
                (UIManager.instance.sandboxLayer.allEntityButtons.Last() as SandboxButtonEntity).SetSprite();
                var heartContainer = traverse.Field("heartContainer").GetValue();
                AccessTools.Method(heartContainer.GetType(), "MoveContainerY")?.Invoke(heartContainer, new object[] { tuple.Item4 });
            //}
            /*
            var ui = UIManager.instance;
            var lastButton = ui?.sandboxLayer?.allEntityButtons?.LastOrDefault();
            if (lastButton is SandboxButtonEntity sbEntity)
            {
                sbEntity.SetSprite();
            }

            var heartContainer = traverse.Field("heartContainer").GetValue();
            AccessTools.Method(heartContainer.GetType(), "MoveContainerY")?.Invoke(heartContainer, new object[] { tuple.Item4 });
               */
            UIManager.instance?.menuLayer.CalculateMenu();
        }

    }
}
