using System.Collections.Generic;
using PikePush.Drill;
using PikePush.Utls;
using UnityEngine;

namespace PikePush.Drill.UI
{
    public class DrillCommandPanel : MonoBehaviour
    {
        BlockSelector selector;
        RectTransform buttonContainer;
        Font buttonFont;
        DrillToast toast;

        readonly List<DrillCommandButton> commandButtons = new List<DrillCommandButton>();
        readonly List<GameObject> groupButtons = new List<GameObject>();
        GameObject backButton;
        IReadOnlyList<Block> currentBlocks = System.Array.Empty<Block>();

        DrillCommandGroup? activeGroup;
        bool needsRebuild;

        // Keyboard scheme:
        //  Top-level → letter shortcuts (DrillCommandCatalog.HotKey + GroupHotKey)
        //  Submenu   → numeric 1-9 picks the Nth command in the group,
        //              Esc backs out to top-level.
        // Same source feeds the listener and the per-button hint label.

        public void Initialize(BlockSelector selector, RectTransform buttonContainer, Font buttonFont, DrillToast toast = null)
        {
            this.selector = selector;
            this.buttonContainer = buttonContainer;
            this.buttonFont = buttonFont;
            this.toast = toast;

            selector.SelectionChanged += OnSelectionChanged;
            OnSelectionChanged(selector.Selected);
            LogHelper.debug($"[DrillCommandPanel] Initialized — subscribed to {selector.name}");
        }

        void OnDestroy()
        {
            if (selector != null) selector.SelectionChanged -= OnSelectionChanged;
        }

        void Update()
        {
            if (currentBlocks.Count == 0) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscape();
                return;
            }

            if (activeGroup == null) HandleTopLevelHotkeys();
            else                     HandleSubmenuHotkeys(activeGroup.Value);

            if (needsRebuild) RebuildButtons();
            RefreshGating();
        }

        void HandleTopLevelHotkeys()
        {
            // Command hotkeys (Halt / Forward March / Reform).
            foreach (var cmd in DrillCommandCatalog.TopLevelCommands)
            {
                var key = DrillCommandCatalog.HotKey(cmd);
                if (key != KeyCode.None && Input.GetKeyDown(key))
                {
                    LogHelper.debug($"[DrillCommandPanel] Key {key} → {cmd}");
                    IssueToAll(cmd);
                    return;
                }
            }
            // Group-opener hotkeys (P / D / F / B / I / C / W).
            foreach (var group in DrillCommandCatalog.TopLevelGroups)
            {
                var key = DrillCommandCatalog.GroupHotKey(group);
                if (key != KeyCode.None && Input.GetKeyDown(key))
                {
                    LogHelper.debug($"[DrillCommandPanel] Key {key} → open {group}");
                    OpenGroup(group);
                    return;
                }
            }
        }

        void HandleSubmenuHotkeys(DrillCommandGroup group)
        {
            // 1..9 picks the Nth command in the group (matching the on-button
            // hint). Commands past the 9th are mouse-only.
            var cmds = DrillCommandCatalog.CommandsInGroup(group);
            int max = Mathf.Min(9, cmds.Length);
            for (int i = 0; i < max; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    LogHelper.debug($"[DrillCommandPanel] Submenu key {i + 1} → {cmds[i]}");
                    IssueToAll(cmds[i]);
                    CollapseToTopLevel();
                    return;
                }
            }
        }

        void HandleEscape()
        {
            if (activeGroup != null)
            {
                CollapseToTopLevel();
                return;
            }
            // At top-level, Esc clears selection (which hides the panel).
            selector.Clear();
        }

        void OnSelectionChanged(IReadOnlyList<Block> blocks)
        {
            LogHelper.debug($"[DrillCommandPanel] OnSelectionChanged: {blocks.Count} block(s)");
            currentBlocks = blocks;
            SetVisible(blocks.Count > 0);

            // A new selection always lands you at top-level.
            activeGroup = null;
            needsRebuild = true;

            if (blocks.Count == 0)
            {
                ClearButtons();
                return;
            }

            RebuildButtons();
            RefreshGating();
        }

        void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        void ClearButtons()
        {
            foreach (var b in commandButtons)
                if (b != null) Destroy(b.gameObject);
            commandButtons.Clear();

            foreach (var g in groupButtons)
                if (g != null) Destroy(g);
            groupButtons.Clear();

            if (backButton != null) { Destroy(backButton); backButton = null; }
        }

        void RebuildButtons()
        {
            ClearButtons();
            if (buttonContainer == null) return;

            if (activeGroup == null) BuildTopLevel();
            else BuildSubmenu(activeGroup.Value);

            needsRebuild = false;
        }

        void BuildTopLevel()
        {
            foreach (var cmd in DrillCommandCatalog.TopLevelCommands)
                AddCommandButton(cmd, DrillCommandCatalog.HotKey(cmd));

            foreach (var group in DrillCommandCatalog.TopLevelGroups)
                AddGroupOpener(group);
        }

        void BuildSubmenu(DrillCommandGroup group)
        {
            backButton = BuildNavButton(buttonContainer, buttonFont, "◀ Back", "(Esc)", CollapseToTopLevel);

            var cmds = DrillCommandCatalog.CommandsInGroup(group);
            for (int i = 0; i < cmds.Length; i++)
            {
                // Numerical hotkey for the first nine entries; nothing for the rest.
                var key = i < 9 ? (KeyCode)((int)KeyCode.Alpha1 + i) : KeyCode.None;
                AddCommandButton(cmds[i], key);
            }
        }

        void AddCommandButton(DrillCommand cmd, KeyCode hint)
        {
            string label = DrillCommandCatalog.Label(cmd);
            var btn = DrillCommandButton.Build(buttonContainer, cmd, label, hint,
                buttonFont, OnCommandButtonPressed);
            commandButtons.Add(btn);
        }

        void AddGroupOpener(DrillCommandGroup group)
        {
            string label = DrillCommandCatalog.GroupLabel(group) + " ▸";
            KeyCode key = DrillCommandCatalog.GroupHotKey(group);
            string hint = key == KeyCode.None ? null : $"({key})";
            var go = BuildNavButton(buttonContainer, buttonFont, label, hint, () => OpenGroup(group));
            groupButtons.Add(go);
        }

        void OpenGroup(DrillCommandGroup group)
        {
            LogHelper.debug($"[DrillCommandPanel] Opening submenu: {group}");
            activeGroup = group;
            needsRebuild = true;
        }

        void CollapseToTopLevel()
        {
            LogHelper.debug("[DrillCommandPanel] Collapse to top-level");
            activeGroup = null;
            needsRebuild = true;
        }

        void RefreshGating()
        {
            foreach (var btn in commandButtons)
            {
                if (btn == null) continue;
                bool allowed = AllBlocksAllow(btn.Command);
                btn.SetInteractable(allowed);
            }
        }

        bool AllBlocksAllow(DrillCommand cmd)
        {
            for (int i = 0; i < currentBlocks.Count; i++)
                if (!currentBlocks[i].AllowsCommand(cmd)) return false;
            return currentBlocks.Count > 0;
        }

        void IssueToAll(DrillCommand cmd)
        {
            WarnIfStub(cmd);
            for (int i = 0; i < currentBlocks.Count; i++)
                currentBlocks[i].Issue(cmd);
        }

        // If the command is wired logically but has no visible effect yet,
        // toast the player so a no-op press doesn't look like a broken UI.
        void WarnIfStub(DrillCommand cmd)
        {
            if (toast == null) return;
            if (DrillCommandCatalog.IsImplemented(cmd)) return;
            toast.Show($"TODO — '{DrillCommandCatalog.Label(cmd)}' has no visual yet (state still updates)");
        }

        void OnCommandButtonPressed(DrillCommand cmd)
        {
            IssueToAll(cmd);
            // Spec: selecting a sub-option fires the command and auto-collapses.
            if (activeGroup != null) CollapseToTopLevel();
        }

        // Navigation button — Back, group openers. Same two-row layout as
        // DrillCommandButton (label on top, optional hint on bottom) so the
        // toolbar looks consistent.
        static GameObject BuildNavButton(Transform parent, Font font, string label, string hint, System.Action onClick)
        {
            var go = new GameObject($"NavButton_{label}");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.22f, 0.20f, 0.16f, 0.95f);

            var btn = go.AddComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            bool hasHint = !string.IsNullOrEmpty(hint);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, hasHint ? 0.4f : 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelTxt = labelGo.AddComponent<UnityEngine.UI.Text>();
            labelTxt.font = font;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.color = Color.white;
            labelTxt.fontSize = 18;
            labelTxt.text = label;
            labelTxt.raycastTarget = false;

            if (hasHint)
            {
                var hintGo = new GameObject("Hint");
                hintGo.transform.SetParent(go.transform, false);
                var hintRect = hintGo.AddComponent<RectTransform>();
                hintRect.anchorMin = new Vector2(0f, 0f);
                hintRect.anchorMax = new Vector2(1f, 0.4f);
                hintRect.offsetMin = Vector2.zero;
                hintRect.offsetMax = Vector2.zero;
                var hintTxt = hintGo.AddComponent<UnityEngine.UI.Text>();
                hintTxt.font = font;
                hintTxt.alignment = TextAnchor.MiddleCenter;
                hintTxt.color = new Color(0.75f, 0.75f, 0.75f);
                hintTxt.fontSize = 14;
                hintTxt.text = hint;
                hintTxt.raycastTarget = false;
            }

            return go;
        }
    }
}
