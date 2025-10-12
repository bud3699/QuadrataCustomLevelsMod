using UnityEngine;
using UnityEngine.UI;

namespace QuadrataPatcher
{
    public class CustomLevelCompleteUI : MonoBehaviour
    {
        public static void Show()
        {
            if (UIManager.currentLayer == null)
            {
                Debug.LogWarning("No active UI layer found.");
                return;
            }

            var uiRoot = new GameObject("CustomLevelCompleteUI");
            uiRoot.transform.SetParent(UIManager.currentLayer.transform, false);

            var bg = uiRoot.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);

            var rootRect = uiRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(500, 400);
            rootRect.localPosition = Vector3.zero;

            var headerGO = new GameObject("HeaderText");
            headerGO.transform.SetParent(uiRoot.transform, false);
            var headerText = headerGO.AddComponent<Text>();
            headerText.text = "Would you like to upload your custom level?";
            headerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            headerText.alignment = TextAnchor.MiddleCenter;
            headerText.color = Color.white;
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

            var usernameInput = CreateInput("Username", 80f);
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
                Debug.Log($"Custom Level Upload Info:\nUsername: {usernameInput.text}\nTitle: {titleInput.text}\nDescription: {descInput.text}");
                Destroy(uiRoot);
                UIManager.currentLayer = UIManager.instance.sandboxLayer;
            });

            CreateButton("Cancel", -120f, () =>
            {
                Debug.Log("Custom level upload canceled.");
                Destroy(uiRoot);
                UIManager.currentLayer = UIManager.instance.sandboxLayer;
            });
        }
    }
}
