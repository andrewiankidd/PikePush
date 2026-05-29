using System.Collections.Generic;
using PikePush.Combat;
using PikePush.Drill;
using UnityEngine;
using UnityEngine.UI;

namespace PikePush.Drill.UI
{
    // Top-left stacked overview of every active engagement. One row per
    // engaged pair with two horizontal fill bars (friendly + enemy meter),
    // labels, and percentage readouts. Rows are spawned / culled to match
    // the live engagement list.
    public class EngagementOverviewPanel : MonoBehaviour
    {
        const float RowHeight = 64f;
        const float BarHeight = 18f;

        static readonly Color FriendlyBarColor = new Color(0.30f, 0.60f, 0.95f);
        static readonly Color EnemyBarColor    = new Color(0.95f, 0.30f, 0.30f);
        static readonly Color BarBackground    = new Color(0.10f, 0.10f, 0.12f, 0.80f);

        Font font;
        Image background;
        RectTransform rowContainer;
        IReadOnlyList<Engagement> source;
        readonly List<Row> rows = new List<Row>();

        public static EngagementOverviewPanel Build(Transform canvasParent, Font font,
            IReadOnlyList<Engagement> source)
        {
            var go = new GameObject("EngagementOverview");
            go.transform.SetParent(canvasParent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -20f);
            rect.sizeDelta = new Vector2(460f, RowHeight + 24f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            var rowsGo = new GameObject("Rows");
            rowsGo.transform.SetParent(go.transform, false);
            var rowsRect = rowsGo.AddComponent<RectTransform>();
            rowsRect.anchorMin = Vector2.zero;
            rowsRect.anchorMax = Vector2.one;
            rowsRect.offsetMin = new Vector2(10f, 10f);
            rowsRect.offsetMax = new Vector2(-10f, -10f);

            var layout = rowsGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            var panel = go.AddComponent<EngagementOverviewPanel>();
            panel.font = font;
            panel.background = bg;
            panel.rowContainer = rowsRect;
            panel.source = source;
            panel.SetVisible(false);
            return panel;
        }

        void Update()
        {
            int n = source?.Count ?? 0;

            while (rows.Count < n) AddRow();
            while (rows.Count > n) RemoveLastRow();

            SetVisible(n > 0);
            if (n == 0) return;

            for (int i = 0; i < n; i++) rows[i].UpdateView(source[i]);

            // Grow the panel to fit current row count.
            var rect = (RectTransform)transform;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, (RowHeight * n) + (8f * Mathf.Max(0, n - 1)) + 20f);
        }

        void AddRow()
        {
            var row = Row.Build(rowContainer, font);
            rows.Add(row);
        }

        void RemoveLastRow()
        {
            int last = rows.Count - 1;
            if (last < 0) return;
            if (rows[last].root != null) Destroy(rows[last].root);
            rows.RemoveAt(last);
        }

        // Toggle the renderers, not the GameObject. Disabling the GameObject
        // stops Update() from running, which is how the original version got
        // stuck hidden on first frame and never reappeared when an engagement
        // started.
        void SetVisible(bool visible)
        {
            if (background != null) background.enabled = visible;
        }

        // -- Row view -----------------------------------------------------

        class Row
        {
            public GameObject root;
            public Text title;
            public Image friendlyFill;
            public Text friendlyPercent;
            public Image enemyFill;
            public Text enemyPercent;

            public void UpdateView(Engagement e)
            {
                title.text = $"{LabelOf(e.A)}  vs  {LabelOf(e.B)}";
                SetBar(friendlyFill, friendlyPercent, e.MeterA.Value);
                SetBar(enemyFill, enemyPercent, e.MeterB.Value);
            }

            static string LabelOf(Block b) => b != null ? b.label : "?";

            static void SetBar(Image fill, Text pct, float value)
            {
                fill.fillAmount = Mathf.Clamp01(value);
                pct.text = $"{Mathf.RoundToInt(value * 100f)}%";
            }

            public static Row Build(RectTransform parent, Font font)
            {
                var rowGo = new GameObject("Row");
                rowGo.transform.SetParent(parent, false);

                var rowRect = rowGo.AddComponent<RectTransform>();
                rowRect.sizeDelta = new Vector2(0f, RowHeight);
                var le = rowGo.AddComponent<LayoutElement>();
                le.minHeight = RowHeight;
                le.preferredHeight = RowHeight;
                le.flexibleHeight = 0f;

                var titleGo = new GameObject("Title");
                titleGo.transform.SetParent(rowGo.transform, false);
                var trect = titleGo.AddComponent<RectTransform>();
                trect.anchorMin = new Vector2(0f, 1f);
                trect.anchorMax = new Vector2(1f, 1f);
                trect.pivot = new Vector2(0f, 1f);
                trect.anchoredPosition = new Vector2(0f, 0f);
                trect.sizeDelta = new Vector2(0f, 18f);
                var ttxt = titleGo.AddComponent<Text>();
                ttxt.font = font;
                ttxt.fontSize = 14;
                ttxt.color = Color.white;
                ttxt.alignment = TextAnchor.MiddleLeft;
                ttxt.raycastTarget = false;

                var (fFill, fPct) = BuildBar(rowGo.transform, font, FriendlyBarColor,
                    yFromTop: 22f);
                var (eFill, ePct) = BuildBar(rowGo.transform, font, EnemyBarColor,
                    yFromTop: 22f + BarHeight + 4f);

                return new Row
                {
                    root = rowGo,
                    title = ttxt,
                    friendlyFill = fFill,
                    friendlyPercent = fPct,
                    enemyFill = eFill,
                    enemyPercent = ePct,
                };
            }

            static (Image fill, Text pct) BuildBar(Transform parent, Font font, Color fillColor, float yFromTop)
            {
                var barGo = new GameObject("Bar");
                barGo.transform.SetParent(parent, false);
                var brect = barGo.AddComponent<RectTransform>();
                brect.anchorMin = new Vector2(0f, 1f);
                brect.anchorMax = new Vector2(1f, 1f);
                brect.pivot = new Vector2(0f, 1f);
                brect.anchoredPosition = new Vector2(0f, -yFromTop);
                brect.sizeDelta = new Vector2(0f, BarHeight);
                var bbg = barGo.AddComponent<Image>();
                bbg.color = BarBackground;
                bbg.raycastTarget = false;

                var fillGo = new GameObject("Fill");
                fillGo.transform.SetParent(barGo.transform, false);
                var frect = fillGo.AddComponent<RectTransform>();
                frect.anchorMin = new Vector2(0f, 0f);
                frect.anchorMax = new Vector2(1f, 1f);
                frect.offsetMin = new Vector2(2f, 2f);
                frect.offsetMax = new Vector2(-2f, -2f);
                var fill = fillGo.AddComponent<Image>();
                fill.color = fillColor;
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillOrigin = (int)Image.OriginHorizontal.Left;
                fill.fillAmount = 0f;
                fill.raycastTarget = false;

                var pctGo = new GameObject("Percent");
                pctGo.transform.SetParent(barGo.transform, false);
                var prect = pctGo.AddComponent<RectTransform>();
                prect.anchorMin = new Vector2(1f, 0f);
                prect.anchorMax = new Vector2(1f, 1f);
                prect.pivot = new Vector2(1f, 0.5f);
                prect.anchoredPosition = new Vector2(-6f, 0f);
                prect.sizeDelta = new Vector2(60f, 0f);
                var ptxt = pctGo.AddComponent<Text>();
                ptxt.font = font;
                ptxt.fontSize = 13;
                ptxt.color = Color.white;
                ptxt.alignment = TextAnchor.MiddleRight;
                ptxt.raycastTarget = false;

                return (fill, ptxt);
            }
        }
    }
}
