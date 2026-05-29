using System.Collections.Generic;
using System.Text;
using PikePush.Combat;
using PikePush.Drill;
using UnityEngine;
using UnityEngine.UI;

namespace PikePush.Drill.UI
{
    // Top-left stacked overview of every active engagement. One row per
    // engaged pair, friendly side on the left, enemy on the right, percentages
    // for each meter. Floating world-anchored HUDs are a polish follow-up;
    // this gets us a readable signal that meters are ticking.
    public class EngagementOverviewPanel : MonoBehaviour
    {
        Font font;
        Image background;
        Text text;
        IReadOnlyList<Engagement> source;

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
            rect.sizeDelta = new Vector2(420f, 120f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trect = textGo.AddComponent<RectTransform>();
            trect.anchorMin = Vector2.zero;
            trect.anchorMax = Vector2.one;
            trect.offsetMin = new Vector2(10f, 8f);
            trect.offsetMax = new Vector2(-10f, -8f);
            var txt = textGo.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 16;
            txt.color = Color.white;
            txt.alignment = TextAnchor.UpperLeft;
            txt.text = "";
            txt.raycastTarget = false;

            var panel = go.AddComponent<EngagementOverviewPanel>();
            panel.font = font;
            panel.background = bg;
            panel.text = txt;
            panel.source = source;
            panel.SetVisible(false);
            return panel;
        }

        void Update()
        {
            if (source == null || source.Count == 0)
            {
                SetVisible(false);
                return;
            }
            SetVisible(true);
            text.text = Render();
        }

        string Render()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Engagements");
            for (int i = 0; i < source.Count; i++)
            {
                var e = source[i];
                sb.AppendLine(
                    $"  {Label(e.A)}: {Percent(e.MeterA.Value)}  vs  {Label(e.B)}: {Percent(e.MeterB.Value)}");
            }
            return sb.ToString();
        }

        static string Label(Block b) => b != null ? b.label : "?";
        static string Percent(float v) => $"{Mathf.RoundToInt(v * 100f),3}%";

        // Toggle the renderers, not the GameObject. Disabling the GameObject
        // stops Update() from running, which is how the original version got
        // stuck hidden on first frame and never reappeared when an engagement
        // started.
        void SetVisible(bool visible)
        {
            if (background != null) background.enabled = visible;
            if (text != null) text.enabled = visible;
        }
    }
}
