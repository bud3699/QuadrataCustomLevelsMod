using DG.Tweening;
using HarmonyLib;
using QuadrataPatcher;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class LayerMenuExtensions
{
    private static IEnumerator WaitAndChangeSprite(MenuButtonSandbox sandboxBtn, Sprite sprite)
    {
        yield return new WaitWhile(() => UIManager.isMenuMoving);

        var traverse = Traverse.Create(sandboxBtn);
        var buttonIcon = traverse.Field("buttonIcon").GetValue<Image>();

        if (buttonIcon != null){buttonIcon.sprite = sprite;}
    }

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
        var levelNumber = traverse.Field("levelNumber").GetValue<RectTransform>();

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
        openedMenuSizeField.SetValue(
            SteamApps.BIsDlcInstalled((AppId_t)3676950u)
            ? openedMenuSize + new Vector2(440f, 0f)
            : openedMenuSize
        );

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
            foreach (var button in levelButtons)
                button.gameObject.SetActive(true);
        }
        else if (levelButtons != null)
        {
            foreach (var button in levelButtons)
                button.gameObject.SetActive(false);
        }

        Transform menuParent = activeButtons.Count > 0 ? activeButtons[0].rectTransform.parent : null;

        if (menuParent != null && SteamApps.BIsDlcInstalled((AppId_t)3676950u))
        {
            bool alreadyExists = false;
            foreach (Transform child in menuParent)
            {
                if (child.name == "MenuLoadButton" || child.name == "MenuInputField")
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                GameObject loadButtonObj = new GameObject("MenuLoadButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                RectTransform loadButtonRect = loadButtonObj.GetComponent<RectTransform>();
                loadButtonObj.transform.SetParent(menuParent, false);
                loadButtonRect.sizeDelta = new Vector2(70, 60);
                loadButtonRect.anchorMin = new Vector2(1f, 1f);
                loadButtonRect.anchorMax = new Vector2(1f, 1f);
                loadButtonRect.pivot = new Vector2(1f, 1f);
                loadButtonRect.anchoredPosition = new Vector2(-60f, -30f);

                GameObject loadTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform loadTextRect = loadTextObj.GetComponent<RectTransform>();
                loadTextObj.transform.SetParent(loadButtonObj.transform, false);
                loadTextRect.sizeDelta = new Vector2(80, 60);
                loadTextRect.anchoredPosition = Vector2.zero;

                Text loadText = loadTextObj.GetComponent<Text>();
                loadText.text = "Load";
                loadText.alignment = TextAnchor.MiddleCenter;
                loadText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                loadText.color = Color.black;
                loadText.fontSize = 20;

                GameObject inputWrapper = new GameObject("MenuInputWrapper", typeof(RectTransform));
                RectTransform wrapperRect = inputWrapper.GetComponent<RectTransform>();
                inputWrapper.transform.SetParent(menuParent, false);
                wrapperRect.sizeDelta = new Vector2(160, 60);
                wrapperRect.anchorMin = new Vector2(1f, 1f);
                wrapperRect.anchorMax = new Vector2(1f, 1f);
                wrapperRect.pivot = new Vector2(1f, 1f);
                wrapperRect.anchoredPosition = new Vector2(-140f, -30f);

                GameObject borderObj = new GameObject("Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform borderRect = borderObj.GetComponent<RectTransform>();
                borderObj.transform.SetParent(inputWrapper.transform, false);
                borderRect.anchorMin = new Vector2(0.5f, 0.5f);
                borderRect.anchorMax = new Vector2(0.5f, 0.5f);
                borderRect.pivot = new Vector2(0.5f, 0.5f);
                borderRect.sizeDelta = new Vector2(168, 68);
                borderRect.anchoredPosition = Vector2.zero;

                Image borderImage = borderObj.GetComponent<Image>();
                borderImage.color = Color.black;

                GameObject inputObj = new GameObject("MenuInputField", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
                RectTransform inputRect = inputObj.GetComponent<RectTransform>();
                inputObj.transform.SetParent(inputWrapper.transform, false);
                inputRect.sizeDelta = new Vector2(160, 60);
                inputRect.anchorMin = new Vector2(0.5f, 0.5f);
                inputRect.anchorMax = new Vector2(0.5f, 0.5f);
                inputRect.pivot = new Vector2(0.5f, 0.5f);
                inputRect.anchoredPosition = Vector2.zero;

                Image inputImage = inputObj.GetComponent<Image>();
                inputImage.color = Color.white;

                GameObject inputTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
                inputTextObj.transform.SetParent(inputObj.transform, false);
                inputTextRect.sizeDelta = new Vector2(160, 60);
                inputTextRect.anchorMin = new Vector2(0.5f, 0.5f);
                inputTextRect.anchorMax = new Vector2(0.5f, 0.5f);
                inputTextRect.pivot = new Vector2(0.5f, 0.5f);
                inputTextRect.anchoredPosition = Vector2.zero;

                Text inputText = inputTextObj.GetComponent<Text>();
                inputText.text = "";
                inputText.alignment = TextAnchor.MiddleCenter;
                inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                inputText.color = Color.black;
                inputText.fontSize = 30;

                InputField inputField = inputObj.GetComponent<InputField>();
                inputField.textComponent = inputText;
                inputField.text = "3MPYMS";

                GameObject versionTextObj = new GameObject("ModVersionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                RectTransform versionTextRect = versionTextObj.GetComponent<RectTransform>();
                versionTextObj.transform.SetParent(menuParent, false);
                versionTextRect.sizeDelta = new Vector2(200, 30);
                versionTextRect.anchorMin = new Vector2(1f, 1f);
                versionTextRect.anchorMax = new Vector2(1f, 1f);
                versionTextRect.pivot = new Vector2(1f, 1f);
                versionTextRect.anchoredPosition = new Vector2(-140f, -90f);

                Text versionText = versionTextObj.GetComponent<Text>();
                versionText.text = $"Current Mod Version: {Main.ModVersion}";
                versionText.alignment = TextAnchor.MiddleRight;
                versionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                versionText.color = new Color(0.5f, 0.5f, 0.5f);
                versionText.fontSize = 14;

                Button loadButton = loadButtonObj.GetComponent<Button>();
                loadButton.onClick.AddListener(() =>
                {
                    var sandboxBtn = GameObject.FindObjectsOfType<MenuButtonSandbox>().FirstOrDefault();
                    var sandboxTraverse = Traverse.Create(sandboxBtn);
                    var toggleMapping = sandboxTraverse.Field("toggleMapping").GetValue<Dictionary<GameMode, (GameMode targetMode, Sprite targetIcon)>>();

                    Director.instance?.StartCoroutine(LoadCustomLevelCoroutine(inputField, sandboxBtn, toggleMapping));
                });

                Debug.Log("[Patch] Menu Load button and InputField added.");
            }
        }

        Debug.Log(gameMode);
        Debug.Log($"[Patch] CalculateMenuExtended executed — {(gameMode == 3 ? "vertical navigation removed" : "full navigation enabled")}.");
    }


    private static IEnumerator LoadCustomLevelCoroutine(InputField inputField, MenuButtonSandbox sandboxBtn, Dictionary<GameMode, (GameMode targetMode, Sprite targetIcon)> toggleMapping)
    {
        /*
            if (toggleMapping.TryGetValue(currentMode, out var mapping) && !(Director.gameMode == (GameMode)3)) {
            Debug.Log("Got value of pre: " + Director.gameMode.ToString());
            Director.gameMode = mapping.targetMode;
            Debug.Log("Got value of: " + mapping.targetMode.ToString());
        }*/
        Director.gameMode = (GameMode)3;
        DiscordManagerPatches.LevelCodeDiscord = null;
        UIManager.instance?.menuLayer.CloseMenu();

        LevelAnimation.isLevelLoading = true;
        Character leftCharacter = Finder.allCharacters.FirstOrDefault(c => c.transform.position.x < 0f);
        Character rightCharacter = Finder.allCharacters.FirstOrDefault(c => c.transform.position.x > 0f);

        if (leftCharacter != null && rightCharacter != null)
        {
            yield return new WaitWhile(() => leftCharacter.moving || rightCharacter.moving);
            yield return new WaitForSecondsRealtime(0.1f);
            yield return new WaitWhile(() => leftCharacter.moving || rightCharacter.moving);
            yield return new WaitForSecondsRealtime(0.2f);
        }
        Director.instance.CloseLevel();
        yield return new WaitWhile(() => LevelAnimation.isLevelLoading);
        yield return new WaitForSecondsRealtime(0.1f);

        UIManager.instance?.sandboxLayer.sideButtons.Last().ResetButton();
        Director.instance?.StartCoroutine(WaitAndChangeSprite(sandboxBtn, toggleMapping[Director.gameMode].targetIcon));
        DirectorPatches.customLevelCode = inputField.text;
        Director.instance?.Init();

        Debug.Log($"[Patch] Loaded custom level code: {inputField.text}");
    }
}
