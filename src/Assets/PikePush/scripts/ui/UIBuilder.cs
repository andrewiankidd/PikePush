using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PikePush.UI
{
    public readonly struct ScrollViewParts
    {
        public readonly RectTransform Root;
        public readonly RectTransform Viewport;
        public readonly RectTransform Content;
        public readonly ScrollRect ScrollRect;

        public ScrollViewParts(RectTransform root, RectTransform viewport, RectTransform content, ScrollRect scroll)
        {
            Root = root;
            Viewport = viewport;
            Content = content;
            ScrollRect = scroll;
        }
    }

    public static class UIBuilder
    {
        public static Canvas EnsureCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static GameObject Backdrop(Transform parent, Action onClick = null, string name = "Backdrop")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            StretchToParent(rect);

            var img = go.AddComponent<Image>();
            img.color = UITheme.Backdrop;
            img.raycastTarget = true;

            if (onClick != null)
            {
                var btn = go.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => onClick());
            }
            return go;
        }

        public static GameObject Panel(Transform parent, Vector2 size, Color? color = null, string name = "Panel")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = color ?? UITheme.Surface;
            // Image.raycastTarget already eats clicks so a wrapping Backdrop dismiss
            // doesn't fire from inside the panel — Unity UI events don't propagate.

            return go;
        }

        public static Text Text(Transform parent, string content, int fontSize = 0, Color? color = null,
            TextAnchor alignment = TextAnchor.UpperLeft, bool richText = true, FontStyle style = FontStyle.Normal,
            string name = "Text")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = UITheme.DefaultFont;
            text.fontSize = fontSize > 0 ? fontSize : UITheme.FontSizeBody;
            text.fontStyle = style;
            text.color = color ?? UITheme.TextPrimary;
            text.alignment = alignment;
            text.supportRichText = richText;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button Button(Transform parent, string label, Vector2 size, Action onClick,
            Color? bgColor = null, Color? textColor = null, int fontSize = 0, string name = "Button")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.color = bgColor ?? UITheme.SurfaceRaised;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var labelText = Text(go.transform, label,
                fontSize > 0 ? fontSize : UITheme.FontSizeLabel,
                textColor ?? UITheme.TextPrimary,
                TextAnchor.MiddleCenter,
                richText: false,
                style: FontStyle.Bold,
                name: "Label");
            StretchToParent(labelText.rectTransform);

            return btn;
        }

        public static ScrollViewParts ScrollView(Transform parent, string name = "ScrollView")
        {
            var rootGo = new GameObject(name);
            rootGo.transform.SetParent(parent, false);
            var rootRect = rootGo.AddComponent<RectTransform>();
            StretchToParent(rootRect);

            var scroll = rootGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(rootGo.transform, false);
            var viewportRect = viewportGo.AddComponent<RectTransform>();
            StretchToParent(viewportRect);

            var viewportImg = viewportGo.AddComponent<Image>();
            viewportImg.color = new Color(1f, 1f, 1f, 0.01f);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = UITheme.ContentGap;
            layout.padding = new RectOffset(0, 0, 0, 0);

            scroll.viewport = viewportRect;
            scroll.content = contentRect;

            return new ScrollViewParts(rootRect, viewportRect, contentRect, scroll);
        }

        public static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
