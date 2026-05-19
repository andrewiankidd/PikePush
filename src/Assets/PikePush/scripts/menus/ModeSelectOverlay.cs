using PikePush.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PikePush.Menus
{
    public class ModeSelectOverlay : MonoBehaviour
    {
        [SerializeField] Vector2 panelSize = new Vector2(540f, 520f);

        GameObject overlayRoot;

        public void Show()
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
                return;
            }
            BuildPanel();
        }

        public void Hide()
        {
            if (overlayRoot != null) overlayRoot.SetActive(false);
        }

        void BuildPanel()
        {
            var canvas = UIBuilder.EnsureCanvas("ModeSelectCanvas", sortingOrder: 90);
            overlayRoot = UIBuilder.Backdrop(canvas.transform, Hide, name: "ModeSelectRoot");

            var panel = UIBuilder.Panel(overlayRoot.transform, panelSize);

            var title = UIBuilder.Text(panel.transform, "Choose Mode",
                fontSize: UITheme.FontSizeTitle,
                alignment: TextAnchor.MiddleCenter,
                style: FontStyle.Bold,
                richText: false,
                name: "Title");
            UIBuilder.Anchor(title.rectTransform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(0f, -80f),
                offsetMax: new Vector2(0f, -20f));

            BuildModeButton(panel.transform, "Runner",   "the arcade dash",     130f, () => LoadOrToast("Game"));
            BuildModeButton(panel.transform, "Drill",    "command the block",    20f, () => LoadOrToast("Drill"));
            BuildModeButton(panel.transform, "Campaign", "coming soon",         -90f, null, enabled: false);

            BuildCloseButton(panel.transform);
        }

        void BuildModeButton(Transform parent, string label, string sub, float yFromCenter, System.Action onClick, bool enabled = true)
        {
            var btn = UIBuilder.Button(parent, label, new Vector2(420f, 90f),
                onClick: onClick,
                bgColor: enabled ? UITheme.SurfaceRaised : new Color(0.15f, 0.15f, 0.18f, 0.95f),
                fontSize: 28,
                name: $"Mode_{label}");

            var rect = btn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, yFromCenter);

            // Subtitle row inside the button (small line under the label)
            var subText = UIBuilder.Text(btn.transform, sub,
                fontSize: UITheme.FontSizeHint,
                color: enabled ? UITheme.TextMuted : new Color(0.45f, 0.45f, 0.45f),
                alignment: TextAnchor.MiddleCenter,
                richText: false,
                name: "Sub");
            UIBuilder.Anchor(subText.rectTransform,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 0f),
                offsetMin: new Vector2(0f, 8f),
                offsetMax: new Vector2(0f, 26f));

            if (!enabled) btn.interactable = false;
        }

        void BuildCloseButton(Transform panelParent)
        {
            var closeBtn = UIBuilder.Button(panelParent, "×",
                size: new Vector2(44f, 44f),
                onClick: Hide,
                bgColor: UITheme.Danger,
                fontSize: 28,
                name: "CloseButton");

            var rect = closeBtn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-12f, -12f);
        }

        static void LoadOrToast(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                Debug.LogWarning($"[ModeSelect] Scene '{sceneName}' is not in Build Settings yet — add it via File → Build Settings.");
        }
    }
}
