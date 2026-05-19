using PikePush.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PikePush.Menus
{
    public class CreditsOverlay : MonoBehaviour
    {
        [Header("Trigger button (optional fallback)")]
        [Tooltip("Spawn a floating corner button at Start. Leave off when there's already a real menu button wired to GameCredits().")]
        [SerializeField] bool spawnCornerTriggerButton = false;
        [SerializeField] string triggerButtonLabel = "Credits";
        [SerializeField] Vector2 triggerButtonSize = new Vector2(160f, 48f);
        [SerializeField] Vector2 triggerButtonMargin = new Vector2(20f, 20f);

        [Header("Panel")]
        [SerializeField] Vector2 panelSize = new Vector2(900f, 720f);
        [SerializeField] [TextArea(10, 40)] string creditsBody = DefaultCreditsBody;

        const string DefaultCreditsBody =
@"<b>PikePush</b>
A game by Andrew Kidd

<b>Dedicated to</b>
The Earl of Loudoun's Regiment of Foote
A 17th-century pike block re-enactment society
https://loudouns.org.uk/

This game would not exist without their work keeping the drill, kit, and spirit of the period alive.


<b>Third-party assets</b>
Polytope Studio — Modular character pack
Kevin Iglesias — Character animations
FlexibleColorPicker — Colour customization UI
FreeDraw — Flag painting
Low Poly Styled Rocks — Environment props
Low Poly Styled Trees — Environment props
VertexColorFarmAnimals — Wildlife
Wand and Circles — VFX & selection
Controller Input Icons — UI hints
TextMesh Pro — Text rendering (Unity)
SimpleInput — Touch input plugin

Thank you to every Asset Store publisher whose work made this possible.


<b>Built in</b>
Unity

<b>Source</b>
github.com/andrewiankidd/PikePush
";

        GameObject overlayRoot;

        void Start()
        {
            if (spawnCornerTriggerButton) BuildTriggerButton();
        }

        void BuildTriggerButton()
        {
            var canvas = UIBuilder.EnsureCanvas("CreditsTriggerCanvas", sortingOrder: 50);

            var btn = UIBuilder.Button(canvas.transform, triggerButtonLabel, triggerButtonSize, Show);
            var rect = btn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = triggerButtonMargin;
        }

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
            var canvas = UIBuilder.EnsureCanvas("CreditsOverlayCanvas", sortingOrder: 100);
            overlayRoot = UIBuilder.Backdrop(canvas.transform, Hide, name: "CreditsRoot");

            var panel = UIBuilder.Panel(overlayRoot.transform, panelSize);

            var title = UIBuilder.Text(panel.transform, "Credits",
                fontSize: UITheme.FontSizeTitle,
                alignment: TextAnchor.MiddleCenter,
                style: FontStyle.Bold,
                richText: false,
                name: "Title");
            UIBuilder.Anchor(title.rectTransform,
                anchorMin: new Vector2(0f, 1f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(0f, -70f),
                offsetMax: new Vector2(0f, -10f));

            var scroll = UIBuilder.ScrollView(panel.transform, name: "CreditsScroll");
            UIBuilder.Anchor(scroll.Root,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(UITheme.PanelPadding, UITheme.PanelPadding),
                offsetMax: new Vector2(-UITheme.PanelPadding, -80f));

            UIBuilder.Text(scroll.Content, creditsBody,
                fontSize: UITheme.FontSizeBody,
                color: UITheme.TextSecondary,
                alignment: TextAnchor.UpperLeft,
                richText: true,
                name: "CreditsBody");

            BuildCloseButton(panel.transform);
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
    }
}
