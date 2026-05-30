using System.Collections.Generic;
using PikePush.Combat;
using PikePush.Drill;
using UnityEngine;
using UnityEngine.UI;

namespace PikePush.Drill.UI
{
    // Right-side permanent help panel. Shows context-sensitive controls so
    // the player always knows what they can do given the current state
    // (no selection / selection / engagement). Re-renders each frame —
    // cheap (string set + comparison) and keeps the state model simple.
    public class DrillContextHelp : MonoBehaviour
    {
        BlockSelector selector;
        IReadOnlyList<Engagement> engagements;

        Image background;
        Text title;
        Text body;

        public static DrillContextHelp Build(Transform canvasParent, Font font,
            BlockSelector selector, IReadOnlyList<Engagement> engagements)
        {
            var go = new GameObject("DrillContextHelp");
            go.transform.SetParent(canvasParent, false);

            var rt = go.AddComponent<RectTransform>();
            // Bottom-right corner, sitting just above the command toolbar
            // (toolbar is anchored at y=20 with height 110 => top of toolbar at y=130).
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20f, 150f);
            rt.sizeDelta = new Vector2(320f, 340f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);
            bg.raycastTarget = false;

            // Title at the top, body fills the rest. VerticalLayoutGroup keeps
            // it tidy if either text grows.
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(go.transform, false);
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 20;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(1f, 0.85f, 0.55f);
            titleTxt.alignment = TextAnchor.UpperLeft;
            titleTxt.raycastTarget = false;
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 26f;

            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(go.transform, false);
            var bodyTxt = bodyGo.AddComponent<Text>();
            bodyTxt.font = font;
            bodyTxt.fontSize = 14;
            bodyTxt.color = Color.white;
            bodyTxt.alignment = TextAnchor.UpperLeft;
            bodyTxt.lineSpacing = 1.1f;
            bodyTxt.raycastTarget = false;
            bodyTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyTxt.verticalOverflow = VerticalWrapMode.Overflow;

            var help = go.AddComponent<DrillContextHelp>();
            help.selector = selector;
            help.engagements = engagements;
            help.background = bg;
            help.title = titleTxt;
            help.body = bodyTxt;
            help.Refresh();
            return help;
        }

        void Update()
        {
            Refresh();
        }

        void Refresh()
        {
            bool anyEngagement = engagements != null && engagements.Count > 0;
            bool selectedEngaged = AnySelectedIsEngaged();
            int selectedCount = selector != null ? selector.Selected.Count : 0;

            if (anyEngagement && selectedEngaged)
            {
                title.text = "Push of Pike!";
                body.text =
                    "Hold SPACE to push.\n" +
                    "Input applies to every selected block.\n\n" +
                    "You can still change:\n" +
                    "  · Posture  (Postures ▸)\n" +
                    "  · Spacing  (Distancing ▸)\n" +
                    "Closest Order pushes harder\n" +
                    "and drains slower.\n\n" +
                    "Reform breaks the engagement.";
            }
            else if (anyEngagement)
            {
                title.text = "Engagements Active";
                body.text =
                    "Click an engaged block, then\n" +
                    "hold SPACE to push on its side.\n\n" +
                    "An engaged block with no input\n" +
                    "drains — keep at least one in\n" +
                    "your selection while mashing.";
            }
            else if (selectedCount > 0)
            {
                title.text = selectedCount == 1 && selector.Primary != null
                    ? $"Selected: {selector.Primary.label}"
                    : $"Selected: {selectedCount} blocks";
                body.text =
                    "Pick a command from the bar.\n\n" +
                    "Top-level keys:\n" +
                    BuildHotkeyList() +
                    "\nSubmenu keys:\n" +
                    BuildGroupHotkeyList() +
                    "  1–9 inside, Esc back out\n" +
                    "\nShift+Click to add blocks.\n" +
                    "Esc / empty click clears.";
            }
            else
            {
                title.text = "Controls";
                body.text =
                    "Click a block to select it.\n" +
                    "Shift+Click to multi-select.\n" +
                    "Esc clears the selection.\n\n" +
                    "Spawn forces with the +/−\n" +
                    "panels in the top-right.\n\n" +
                    "Middle mouse to pan camera.\n" +
                    "Scroll wheel to zoom.";
            }
        }

        // Pulls every top-level command's hotkey from the catalog so the help
        // panel stays in sync with whatever's actually wired on the buttons.
        static string BuildHotkeyList()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var cmd in DrillCommandCatalog.TopLevelCommands)
            {
                var key = DrillCommandCatalog.HotKey(cmd);
                if (key == KeyCode.None) continue;
                sb.Append("  ").Append(key).Append(" — ").Append(DrillCommandCatalog.Label(cmd)).Append('\n');
            }
            return sb.ToString();
        }

        // Same idea for the submenu openers — letter → group label.
        static string BuildGroupHotkeyList()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var g in DrillCommandCatalog.TopLevelGroups)
            {
                var key = DrillCommandCatalog.GroupHotKey(g);
                if (key == KeyCode.None) continue;
                sb.Append("  ").Append(key).Append(" — ").Append(DrillCommandCatalog.GroupLabel(g)).Append('\n');
            }
            return sb.ToString();
        }

        bool AnySelectedIsEngaged()
        {
            if (selector == null) return false;
            var sel = selector.Selected;
            for (int i = 0; i < sel.Count; i++)
                if (sel[i] != null && sel[i].IsEngaged) return true;
            return false;
        }
    }
}
