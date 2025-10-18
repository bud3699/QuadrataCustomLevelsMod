using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System.Text;
using System;
using static MonoMod.InlineRT.MonoModRule;
using System.Linq;
using DG.Tweening;


namespace QuadrataPatcher
{
    public class CustomLevelCompleteUI : MonoBehaviour
    {
        public static string gameLevelUpload;
        public static void ChangeButtonSize(Transform rectTransform, Vector2 target, float time = 0f, Ease ease = Ease.Linear, float delay = 0f)
        {
            rectTransform.DOScale(target, time).SetEase(ease).SetDelay(delay).Play();
        }
        public static void Show()
        {
            UIManager.isMenuMoving = true;
            LevelAnimation.isLevelLoading = true;
            if (UIManager.currentLayer == null)
            {
                Debug.LogWarning("No active UI layer found.");
                return;
            }
            var blocker = new GameObject("RaycastBlocker");
            blocker.transform.SetParent(UIManager.currentLayer.transform, false);

            var blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.pivot = new Vector2(0.5f, 0.5f);
            blockerRect.localPosition = Vector3.zero;
            blockerRect.localScale = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            var blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = new Color(0, 0, 0, 0.001f);

            var uiRoot = new GameObject("CustomLevelCompleteUI");
            uiRoot.transform.SetParent(blocker.transform, false);

            var bg = uiRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);

            Sprite menuSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "Menu");
            if (menuSprite != null)
            {
                var menuGO = new GameObject("MenuPanel");
                menuGO.transform.SetParent(uiRoot.transform, false);
                menuGO.transform.SetAsFirstSibling();

                var image = menuGO.AddComponent<Image>();
                image.sprite = menuSprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;

                var rect = image.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localPosition = Vector3.zero;
                rect.localScale = Vector3.one;
                rect.sizeDelta = new Vector2(600, 400);
            }

            var rootRect = uiRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(500, 400);
            rootRect.localPosition = Vector3.zero;
            rootRect.localScale = Vector3.zero;

            ChangeButtonSize(rootRect, Vector2.one, 0.25f, Ease.InBack, 0.1f);

            var headerGO = new GameObject("HeaderText");
            headerGO.transform.SetParent(uiRoot.transform, false);
            var headerText = headerGO.AddComponent<Text>();
            headerText.text = "Would you like to upload your custom level?";
            headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.color = Color.black;
            headerText.fontSize = 20;

            var headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 0.5f);
            headerRect.anchorMax = new Vector2(0.5f, 0.5f);
            headerRect.pivot = new Vector2(0.5f, 0.5f);
            headerRect.sizeDelta = new Vector2(480, 40);
            headerRect.localPosition = new Vector3(0, 130f, 0);

            InputField CreateInput(string placeholderText, float yOffset)
            {
                var inputGO = new GameObject(placeholderText + "Input");
                inputGO.transform.SetParent(uiRoot.transform, false);

                var input = inputGO.AddComponent<InputField>();
                var image = inputGO.AddComponent<Image>();
                image.color = Color.white;

                var inputRect = inputGO.GetComponent<RectTransform>();
                inputRect.anchorMin = new Vector2(0.5f, 0.5f);
                inputRect.anchorMax = new Vector2(0.5f, 0.5f);
                inputRect.pivot = new Vector2(0.5f, 0.5f);
                inputRect.sizeDelta = new Vector2(400, 30);
                inputRect.localPosition = new Vector3(0, yOffset, 0);

                var textGO = new GameObject("Text");
                textGO.transform.SetParent(inputGO.transform, false);
                var text = textGO.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.color = Color.black;
                text.fontSize = 16;
                text.alignment = TextAnchor.MiddleLeft;

                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.localPosition = Vector3.zero;
                textRect.sizeDelta = Vector2.zero;

                input.textComponent = text;

                var placeholderGO = new GameObject("Placeholder");
                placeholderGO.transform.SetParent(inputGO.transform, false);
                var placeholder = placeholderGO.AddComponent<Text>();
                placeholder.text = placeholderText;
                placeholder.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                placeholder.fontSize = 16;
                placeholder.color = new Color(0.5f, 0.5f, 0.5f);
                placeholder.alignment = TextAnchor.MiddleLeft;

                var placeholderRect = placeholderGO.GetComponent<RectTransform>();
                placeholderRect.anchorMin = new Vector2(0f, 0f);
                placeholderRect.anchorMax = new Vector2(1f, 1f);
                placeholderRect.pivot = new Vector2(0.5f, 0.5f);
                placeholderRect.localPosition = Vector3.zero;
                placeholderRect.sizeDelta = Vector2.zero;

                input.placeholder = placeholder;

                return input;
            }

            var titleInput = CreateInput("Level Title", 30f);
            var descInput = CreateInput("Level Description", -20f);

            GameObject CreateButton(string label, float yOffset, UnityEngine.Events.UnityAction onClick)
            {
                var buttonGO = new GameObject(label + "Button");
                buttonGO.transform.SetParent(uiRoot.transform, false);
                var button = buttonGO.AddComponent<Button>();
                var image = buttonGO.AddComponent<Image>();
                image.color = Color.white;

                var buttonRect = buttonGO.GetComponent<RectTransform>();
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.pivot = new Vector2(0.5f, 0.5f);
                buttonRect.sizeDelta = new Vector2(160, 40);
                buttonRect.localPosition = new Vector3(0, yOffset, 0);

                var textGO = new GameObject("Text");
                textGO.transform.SetParent(buttonGO.transform, false);
                var text = textGO.AddComponent<Text>();
                text.text = label;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.alignment = TextAnchor.MiddleCenter;
                text.color = Color.black;
                text.fontSize = 18;

                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0f, 0f);
                textRect.anchorMax = new Vector2(1f, 1f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.localPosition = Vector3.zero;
                textRect.sizeDelta = Vector2.zero;

                button.onClick.AddListener(onClick);
                return buttonGO;
            }

            CreateButton("Submit", -70f, () =>
            {
                if (SaveManagerCustom.CurrentData.playerName != SteamworksManager.GetSteamUsername().Trim()) SaveManagerCustom.UpdatePlayerName(SteamworksManager.GetSteamUsername().Trim());
                if (string.IsNullOrEmpty(gameLevelUpload)) { Debug.LogError("❌ gameLevelUpload is empty or null!"); return; }

                GameLevelData loadedLevel;
                try { loadedLevel = JsonUtility.FromJson<GameLevelData>(gameLevelUpload); }
                catch (Exception e) { Debug.LogError("Failed to parse level JSON: " + e.Message); return; }

                if (loadedLevel == null) { Debug.LogError("Parsed GameLevelData is null!"); return; }

                SerializableVector2[] convertedPositions = Array.ConvertAll(
                    loadedLevel.characterPositions,
                    v => new SerializableVector2(v)
                );

                string charPositionsJson = "[";
                for (int i = 0; i < convertedPositions.Length; i++)
                {
                    var p = convertedPositions[i];
                    charPositionsJson += $"{{\"x\":{p.x},\"y\":{p.y}}}";
                    if (i < convertedPositions.Length - 1)
                        charPositionsJson += ",";
                }
                charPositionsJson += "]";

                string entityDataEscaped = loadedLevel.entityData
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\r", "\\r")
                    .Replace("\n", "\\n");

                string json =
                    $"{{\"name\":\"{titleInput.text.Trim()}\"," +
                    $"\"description\":\"{descInput.text.Trim()}\"," +
                    $"\"creator\":\"{SteamworksManager.GetSteamUsername().Trim()}\"," +
                    $"\"data\":{{" +
                    $"\"characterPositions\":{charPositionsJson}," +
                    $"\"entityData\":\"{entityDataEscaped}\"," +
                    $"\"moveCount\":{loadedLevel.moveCount}," +
                    $"\"reversed\":{loadedLevel.reversed.ToString().ToLower()}" +
                    $"}}}}";

                try
                {
                    var request = (HttpWebRequest)WebRequest.Create("https://bud.mynetgear.com/quadrata/upload");
                    request.Method = "POST";
                    request.ContentType = "application/json";

                    byte[] bytes = Encoding.UTF8.GetBytes(json);
                    request.ContentLength = bytes.Length;

                    using (var stream = request.GetRequestStream())
                        stream.Write(bytes, 0, bytes.Length);

                    var response = (HttpWebResponse)request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string result = reader.ReadToEnd();
                        Debug.Log("Upload successful:\n" + result);
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                            Debug.LogError("Upload failed:\n" + reader.ReadToEnd());
                    }
                    else
                    {
                        Debug.LogError("Upload failed: " + ex.Message);
                    }
                }
                gameLevelUpload = null;
                rootRect.DOScale(Vector2.zero, 0.25f)
                    .SetEase(Ease.InBack)
                    .SetDelay(0.1f)
                    .OnComplete(() =>
                    {
                        Destroy(blocker);
                        UIManager.currentLayer = UIManager.instance.sandboxLayer;
                        UIManager.isMenuMoving = false;
                        LevelAnimation.isLevelLoading = false;
                    })
                    .Play();
            });

            CreateButton("Cancel", -120f, () =>
            {
                Debug.Log("Custom level upload canceled.");
                rootRect.DOScale(Vector2.zero, 0.25f)
                    .SetEase(Ease.InBack)
                    .SetDelay(0.1f)
                    .OnComplete(() =>
                    {
                        Destroy(blocker);
                        UIManager.currentLayer = UIManager.instance.sandboxLayer;
                        UIManager.isMenuMoving = false;
                        LevelAnimation.isLevelLoading = false;
                    })
                    .Play();

                //ChangeButtonSize(rootRect, Vector2.zero, 0.25f, Ease.InBack, 0.1f);
            });
        }
    }
}
