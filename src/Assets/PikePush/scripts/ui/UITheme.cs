using UnityEngine;

namespace PikePush.UI
{
    public static class UITheme
    {
        public static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.78f);
        public static readonly Color Surface = new Color(0.10f, 0.10f, 0.12f, 0.98f);
        public static readonly Color SurfaceRaised = new Color(0.18f, 0.18f, 0.22f, 0.95f);
        public static readonly Color Danger = new Color(0.55f, 0.18f, 0.18f, 1f);
        public static readonly Color Accent = new Color(0.30f, 0.55f, 0.85f, 1f);

        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = new Color(0.92f, 0.92f, 0.92f);
        public static readonly Color TextMuted = new Color(0.70f, 0.70f, 0.70f);

        public const int FontSizeTitle = 38;
        public const int FontSizeHeading = 26;
        public const int FontSizeBody = 20;
        public const int FontSizeLabel = 18;
        public const int FontSizeHint = 14;

        public const float ButtonHeight = 48f;
        public const float ButtonMinWidth = 140f;
        public const float PanelPadding = 30f;
        public const float ContentGap = 8f;

        static Font _cachedFont;
        public static Font DefaultFont
        {
            get
            {
                if (_cachedFont != null) return _cachedFont;
                _cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_cachedFont == null) _cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return _cachedFont;
            }
        }
    }
}
