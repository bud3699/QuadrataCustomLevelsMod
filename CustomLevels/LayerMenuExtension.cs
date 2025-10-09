using DG.Tweening;
using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LayerMenuExtensions
{
    public static void ChangeLevelSelectSizeExtended(this object layerMenuInstance, Vector2 size, float time, Ease ease)
    {
        var traverse = Traverse.Create(layerMenuInstance);

        var levelSelectArea = traverse.Field("levelSelectArea").GetValue<RectTransform>();
        var levelButtons = traverse.Field("levelButtons").GetValue<List<ComponentButton>>();

        if (levelSelectArea == null)
        {
            Debug.LogError("[Patch] levelSelectArea is null — aborting ChangeLevelSelectSizeExtended");
            return;
        }

        if (levelButtons == null)
        {
            Debug.LogError("[Patch] levelButtons is null — aborting ChangeLevelSelectSizeExtended");
            return;
        }

        int gameMode = (int)Traverse.Create(typeof(Director)).Field("gameMode").GetValue();
        int levelIndex = LevelManager.levelIndex; 

        if ((gameMode != 0 && gameMode != 3 && size.x > 0f) || levelIndex < 1 || levelIndex > 90)
        {
            return;
        }

        levelSelectArea.DOSizeDelta(size, time).SetEase(ease).OnComplete(() =>
        {
            foreach (var levelButton in levelButtons)
            {
                if (levelButton != null)
                {
                    AccessTools.Method(levelButton.GetType(), "Init")?.Invoke(levelButton, new object[] { levelIndex });
                }
            }
        }).Play();
    }


    public static void CalculateMenuExtended(this object layerMenuInstance)
    {
        Debug.Log("Using Extended Calculate Menu");
        var traverse = Traverse.Create(layerMenuInstance);

        var topButtons = traverse.Field("topButtons").GetValue<ButtonArray[]>();
        var activeButtons = traverse.Field("activeButtons").GetValue<List<ComponentButton>>();
        var levelButtons = traverse.Field("levelButtons").GetValue<List<ComponentButton>>();
        var peakTopButtons = traverse.Field("peakTopButtons").GetValue<int>();
        var gapTopButtons = traverse.Field("gapTopButtons").GetValue<int>();
        var openedMenuSizeField = traverse.Field("openedMenuSize");

        if (topButtons == null || activeButtons == null)
        {
            Debug.LogError("[Patch] CalculateMenuExtended failed: topButtons or activeButtons is null");
            return;
        }

        activeButtons.Clear();

        if (!SteamManager.Initialized)
        {
            topButtons[3].active = false;
            topButtons[4].active = false;
        }
        else if (SteamApps.BIsDlcInstalled((AppId_t)3676950u))
        {
            topButtons[3].active = true;
        }
        else
        {
            topButtons[3].active = false;
        }

        foreach (var btn in topButtons.Where(b => b.active))
            activeButtons.Add(btn.button);

        foreach (var btn in topButtons.Where(b => !b.active))
            btn.button.gameObject.SetActive(false);

        for (int i = 0; i < activeButtons.Count; i++)
        {
            activeButtons[i].rectTransform.DOAnchorPosX(peakTopButtons + gapTopButtons * i, 0f).Play();
        }

        Vector2 openedMenuSize = new Vector2(120 * activeButtons.Count, 120f) + Vector2.one * 4f;
        openedMenuSizeField.SetValue(openedMenuSize);

        int gameMode = (int)Traverse.Create(typeof(Director)).Field("gameMode").GetValue();

        for (int j = 0; j < activeButtons.Count; j++)
        {
            if (j > 0)
                activeButtons[j].navigationButtons[Vector2.left] = activeButtons[j - 1];
            if (j < activeButtons.Count - 1)
                activeButtons[j].navigationButtons[Vector2.right] = activeButtons[j + 1];

            if (gameMode != 3 && levelButtons != null && levelButtons.Count > 0)
                activeButtons[j].navigationButtons[Vector2.down] = levelButtons[0];
            else
                activeButtons[j].navigationButtons.Remove(Vector2.down);
        }

        if (gameMode != 3 && levelButtons != null)
        {
            levelButtons.ForEach(button =>
            {
                button.navigationButtons[Vector2.up] = activeButtons[0];
            });
        }
        else if (levelButtons != null)
        {
            foreach (var button in levelButtons)
            {
                button.gameObject.SetActive(false);
            }
        }

        Debug.Log(gameMode);
        Debug.Log($"[Patch] CalculateMenuExtended executed — {(gameMode == 3 ? "vertical navigation removed" : "full navigation enabled")}.");
    }
}
