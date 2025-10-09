using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using DG.Tweening;

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

            int currentLevelIndex;
            object gameLevel;
            levelBuilder.InitCustomLevelFromCode(LevelCode, out currentLevelIndex, out gameLevel);

            if (gameLevel == null)
            {
                Debug.LogError("Failed to load custom level — aborting LoadCustomLevel!");
                return;
            }

            Type levelManagerType = AccessTools.TypeByName("LevelManager");
            levelManagerType?.GetMethod("Init")?.Invoke(null, new object[] { currentLevelIndex });

            var ui = UIManager.instance;
            if (ui != null)
            {
                var menuLayer = AccessTools.Field(ui.GetType(), "menuLayer")?.GetValue(ui);
                if (menuLayer != null)
                {
                    var levelButtons = AccessTools.Field(menuLayer.GetType(), "levelButtons")?.GetValue(menuLayer) as IEnumerable<object>;
                    if (levelButtons != null)
                    {
                        foreach (var btn in levelButtons)
                        {
                            if (btn is Component comp)
                            {
                                AccessTools.Method(comp.GetType(), "Init")?.Invoke(comp, new object[] { currentLevelIndex });
                            }
                        }
                    }
                    else Debug.LogWarning("UIManager menuLayer.levelButtons is null!");
                }
                else Debug.LogWarning("UIManager menuLayer is null!");
            }
            else
            {
                Debug.LogError("UIManager instance still null after waiting!");
                return;
            }
            string levelText = "Testing";
            AccessTools.TypeByName("LevelNumber")?.GetMethod("ChangeLevelText")?.Invoke(null, new object[] { levelText });

            AccessTools.Method(directorInstance.GetType(), "InitCommonSystems")?.Invoke(directorInstance, null);

            object heartContainer = traverse.Field("heartContainer").GetValue();
            int moveCount = (int)Traverse.Create(gameLevel).Field("moveCount").GetValue();
            if (heartContainer != null)
            {
                AccessTools.Method(heartContainer.GetType(), "HideOperations")?.Invoke(heartContainer, null);
                AccessTools.Method(heartContainer.GetType(), "Init")?.Invoke(heartContainer, new object[] { moveCount });
            }
            else Debug.LogWarning("HeartContainer is null!");

            object gridLines = AccessTools.Property(AccessTools.TypeByName("GridLines"), "instance")?.GetValue(null);
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

            object selectionFrame = AccessTools.Property(AccessTools.TypeByName("Finder"), "selectionFrame")?.GetValue(null);
            if (selectionFrame != null)
                AccessTools.Method(selectionFrame.GetType(), "AnimateSelectionFrame")?.Invoke(selectionFrame, new object[] { Vector2.zero, Ease.InBack, 0f });
            else
                Debug.LogWarning("SelectionFrame is null!");

            Debug.Log($"<color=green>Custom level loaded successfully: Index {currentLevelIndex}</color>");
        }
    }
}
