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

        // Keyboard shortcuts only fire at top-level (no open submenu). The
        // mapping lives in DrillCommandCatalog.HotKey so the same source
        // feeds both this listener and the per-button hint label.

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

            // Hotkeys fire only at top-level; in a submenu the player has
            // committed to that group and the buttons are the navigation surface.
            if (activeGroup == null)
            {
                foreach (var cmd in DrillCommandCatalog.TopLevelCommands)
                {
                    var key = DrillCommandCatalog.HotKey(cmd);
                    if (key != KeyCode.None && Input.GetKeyDown(key))
                    {
                        LogHelper.debug($"[DrillCommandPanel] Key {key} → {cmd}");
                        IssueToAll(cmd);
                    }
                }
            }

            if (needsRebuild) RebuildButtons();
            RefreshGating();
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
                AddCommandButton(cmd);

            foreach (var group in DrillCommandCatalog.TopLevelGroups)
                AddGroupOpener(group);
        }

        void BuildSubmenu(DrillCommandGroup group)
        {
            backButton = BuildPlainButton(buttonContainer, buttonFont, "◀ Back", CollapseToTopLevel);

            foreach (var cmd in DrillCommandCatalog.CommandsInGroup(group))
                AddCommandButton(cmd);
        }

        void AddCommandButton(DrillCommand cmd)
        {
            string label = DrillCommandCatalog.Label(cmd);
            KeyCode hint = DrillCommandCatalog.HotKey(cmd);
            var btn = DrillCommandButton.Build(buttonContainer, cmd, label, hint,
                buttonFont, OnCommandButtonPressed);
            commandButtons.Add(btn);
        }

        void AddGroupOpener(DrillCommandGroup group)
        {
            string label = DrillCommandCatalog.GroupLabel(group) + " ▸";
            var go = BuildPlainButton(buttonContainer, buttonFont, label, () => OpenGroup(group));
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

        // Plain navigation button (Back, group openers). Uses Unity's Button so
        // disabled visuals come for free if we later gate openers (e.g.
        // "Postures" while braced is technically always available, so probably not).
        static GameObject BuildPlainButton(Transform parent, Font font, string label, System.Action onClick)
        {
            var go = new GameObject($"NavButton_{label}");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.22f, 0.20f, 0.16f, 0.95f);

            var btn = go.AddComponent<UnityEngine.UI.Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var trect = textGo.AddComponent<RectTransform>();
            trect.anchorMin = Vector2.zero;
            trect.anchorMax = Vector2.one;
            trect.offsetMin = Vector2.zero;
            trect.offsetMax = Vector2.zero;
            var txt = textGo.AddComponent<UnityEngine.UI.Text>();
            txt.font = font;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 18;
            txt.text = label;
            txt.raycastTarget = false;

            return go;
        }
    }
}
