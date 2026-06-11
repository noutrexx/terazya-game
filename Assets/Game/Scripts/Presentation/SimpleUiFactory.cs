using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DengeGame.Presentation
{
    /// <summary>
    /// Faz 3 iskelet arayüzünü koddan üreten geçici yardımcı. Tasarımcı kaynağı gerektirmeden
    /// menü → oyun → sonuç akışının çalıştığını kanıtlamak içindir.
    /// Faz 6'da TextMeshPro tabanlı gerçek, tasarlanmış arayüz ile DEĞİŞTİRİLECEKTİR.
    /// </summary>
    public static class SimpleUiFactory
    {
        private static Font UiFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        /// <summary>Tam ekran dikey yerleşimli bir kök panel + Canvas + EventSystem üretir.</summary>
        public static RectTransform CreateScreen(string name, Color background)
        {
            EnsureEventSystem();

            var canvasGo = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // dikey mobil referans
            scaler.matchWidthOrHeight = 0.5f;

            var panelGo = new GameObject("Panel", typeof(Image), typeof(VerticalLayoutGroup));
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.SetParent(canvasGo.transform, false);
            Stretch(panelRect);
            panelGo.GetComponent<Image>().color = background;

            var layout = panelGo.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 40;
            layout.padding = new RectOffset(80, 80, 200, 200);
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return panelRect;
        }

        public static Text CreateLabel(RectTransform parent, string text, int fontSize, Color color)
        {
            var go = new GameObject("Label", typeof(Text), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var label = go.GetComponent<Text>();
            label.font = UiFont;
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            go.GetComponent<LayoutElement>().minHeight = fontSize * 2f;
            return label;
        }

        public static Button CreateButton(RectTransform parent, string text, Action onClick)
        {
            var go = new GameObject("Button", typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.20f, 0.28f, 0.40f, 1f);
            go.GetComponent<LayoutElement>().minHeight = 140;

            var button = go.GetComponent<Button>();
            if (onClick != null)
                button.onClick.AddListener(() => onClick());

            var label = CreateLabel((RectTransform)go.transform, text, 48, Color.white);
            Stretch((RectTransform)label.transform);

            return button;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
