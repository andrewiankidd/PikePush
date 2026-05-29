using System;
using PikePush.Utls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PikePush.Drill.UI
{
    // The +/− panel for adding and removing blocks at runtime in Drill mode.
    // Clamped 1..MaxBlocks by DrillBootstrap; the panel just disables the
    // button at the boundaries.
    public class BlockCountPanel : MonoBehaviour
    {
        Text countText;
        Button addButton;
        Button removeButton;
        Func<int> getCount;
        Action onAdd;
        Action onRemove;
        int minCount;
        int maxCount;

        public static BlockCountPanel Build(Transform canvasParent, Font font,
            Func<int> getCount, Action onAdd, Action onRemove,
            int minCount, int maxCount)
        {
            var go = new GameObject("BlockCountPanel");
            go.transform.SetParent(canvasParent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
            rect.sizeDelta = new Vector2(220f, 56f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var panel = go.AddComponent<BlockCountPanel>();
            panel.minCount = minCount;
            panel.maxCount = maxCount;
            panel.getCount = getCount;
            panel.onAdd = onAdd;
            panel.onRemove = onRemove;

            panel.removeButton = BuildButton(go.transform, font, "−");
            panel.removeButton.onClick.AddListener(() =>
            {
                if (getCount() <= minCount) return;
                onRemove?.Invoke();
                panel.Refresh();
            });

            panel.countText = BuildLabel(go.transform, font);

            panel.addButton = BuildButton(go.transform, font, "+");
            panel.addButton.onClick.AddListener(() =>
            {
                if (getCount() >= maxCount) return;
                onAdd?.Invoke();
                panel.Refresh();
            });

            panel.Refresh();
            return panel;
        }

        public void Refresh()
        {
            int n = getCount();
            countText.text = $"Blocks: {n}";
            removeButton.interactable = n > minCount;
            addButton.interactable = n < maxCount;
        }

        static Button BuildButton(Transform parent, Font font, string label)
        {
            var go = new GameObject($"Btn_{label}");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);

            var btn = go.AddComponent<Button>();

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var trect = textGo.AddComponent<RectTransform>();
            trect.anchorMin = Vector2.zero;
            trect.anchorMax = Vector2.one;
            trect.offsetMin = Vector2.zero;
            trect.offsetMax = Vector2.zero;
            var txt = textGo.AddComponent<Text>();
            txt.font = font;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 22;
            txt.text = label;
            txt.raycastTarget = false;

            return btn;
        }

        static Text BuildLabel(Transform parent, Font font)
        {
            var go = new GameObject("Count");
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.font = font;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 18;
            txt.text = "Blocks: 1";
            txt.raycastTarget = false;
            return txt;
        }
    }
}
