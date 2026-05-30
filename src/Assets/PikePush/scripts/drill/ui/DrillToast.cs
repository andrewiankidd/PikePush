using UnityEngine;
using UnityEngine.UI;

namespace PikePush.Drill.UI
{
    // Top-centre toast banner for in-game notifications — currently used by
    // DrillCommandPanel to surface TODO warnings when the player presses a
    // command whose visual isn't implemented yet. Auto-hides after HoldSeconds.
    public class DrillToast : MonoBehaviour
    {
        const float HoldSeconds = 2.5f;

        Image bg;
        Text label;
        float hideAt;

        public static DrillToast Build(Transform canvasParent, Font font)
        {
            var go = new GameObject("DrillToast");
            go.transform.SetParent(canvasParent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -120f);
            rt.sizeDelta = new Vector2(820f, 60f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.05f, 0.05f, 0.92f);
            bg.raycastTarget = false;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(20f, 6f);
            trt.offsetMax = new Vector2(-20f, -6f);
            var txt = textGo.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 20;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(1f, 0.85f, 0.6f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = string.Empty;
            txt.raycastTarget = false;

            var toast = go.AddComponent<DrillToast>();
            toast.bg = bg;
            toast.label = txt;
            toast.SetVisible(false);
            return toast;
        }

        public void Show(string message)
        {
            if (label == null) return;
            label.text = message;
            hideAt = Time.unscaledTime + HoldSeconds;
            SetVisible(true);
        }

        void Update()
        {
            if (bg != null && bg.enabled && Time.unscaledTime >= hideAt)
                SetVisible(false);
        }

        // Toggle the renderers, not the GameObject, so Update keeps running
        // and a subsequent Show() can wake the toast back up without
        // hand-holding (same lesson as EngagementOverviewPanel had).
        void SetVisible(bool visible)
        {
            if (bg != null) bg.enabled = visible;
            if (label != null) label.enabled = visible;
        }
    }
}
