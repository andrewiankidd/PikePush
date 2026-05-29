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

        public readonly struct Entry
        {
            public readonly DrillCommand Command;
            public readonly string Label;
            public readonly KeyCode Key;

            public Entry(DrillCommand cmd, string label, KeyCode key)
            {
                Command = cmd;
                Label = label;
                Key = key;
            }
        }

        // Working set surfaced on the flat bar. The full categorised palette
        // (postures, doublings, wheeling, etc.) is the next UI iteration —
        // [docs/backlog.md] "Categorised command palette".
        public static readonly Entry[] CommandSet =
        {
            new Entry(DrillCommand.Halt,                 "Halt",              KeyCode.H),
            new Entry(DrillCommand.ForwardMarch,         "Forward March",     KeyCode.M),
            new Entry(DrillCommand.LeftHandFace,         "Left Face",         KeyCode.A),
            new Entry(DrillCommand.RightHandFace,        "Right Face",        KeyCode.D),
            new Entry(DrillCommand.LeftHandAboutFace,    "About Face L",      KeyCode.Q),
            new Entry(DrillCommand.RightHandAboutFace,   "About Face R",      KeyCode.E),
            new Entry(DrillCommand.OpenOrder,            "Open Order",        KeyCode.O),
            new Entry(DrillCommand.CloseOrder,           "Close Order",       KeyCode.C),
            new Entry(DrillCommand.ClosestOrder,         "Closest Order",     KeyCode.V),
            new Entry(DrillCommand.ChargeForHorse,       "Charge for Horse",  KeyCode.B),
            new Entry(DrillCommand.AdvanceYourPike,      "Advance Pike",      KeyCode.Alpha1),
            new Entry(DrillCommand.Reform,               "Reform",            KeyCode.R),
        };

        readonly List<DrillCommandButton> buttons = new List<DrillCommandButton>();
        IReadOnlyList<Block> currentBlocks = System.Array.Empty<Block>();

        public void Initialize(BlockSelector selector, RectTransform buttonContainer, Font buttonFont)
        {
            this.selector = selector;
            this.buttonContainer = buttonContainer;
            this.buttonFont = buttonFont;

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

            foreach (var entry in CommandSet)
            {
                if (Input.GetKeyDown(entry.Key))
                {
                    LogHelper.debug($"[DrillCommandPanel] Key {entry.Key} → {entry.Command}");
                    IssueToAll(entry.Command);
                }
            }

            RefreshGating();
        }

        void OnSelectionChanged(IReadOnlyList<Block> blocks)
        {
            LogHelper.debug($"[DrillCommandPanel] OnSelectionChanged: {blocks.Count} block(s)");
            currentBlocks = blocks;
            SetVisible(blocks.Count > 0);

            if (blocks.Count == 0)
            {
                ClearButtons();
                return;
            }

            if (buttons.Count == 0) BuildButtons();
            RefreshGating();
        }

        void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        void ClearButtons()
        {
            foreach (var b in buttons)
                if (b != null) Destroy(b.gameObject);
            buttons.Clear();
        }

        void BuildButtons()
        {
            if (buttonContainer == null) return;

            for (int i = 0; i < CommandSet.Length; i++)
            {
                var entry = CommandSet[i];
                var btn = DrillCommandButton.Build(buttonContainer, entry.Command, entry.Label, entry.Key,
                    buttonFont, OnButtonPressed);
                buttons.Add(btn);
            }
        }

        void RefreshGating()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var entry = CommandSet[i];
                bool allowed = AllBlocksAllow(entry.Command);
                buttons[i].SetInteractable(allowed);
            }
        }

        bool AllBlocksAllow(DrillCommand cmd)
        {
            for (int i = 0; i < currentBlocks.Count; i++)
            {
                if (!currentBlocks[i].AllowsCommand(cmd)) return false;
            }
            return currentBlocks.Count > 0;
        }

        void IssueToAll(DrillCommand cmd)
        {
            for (int i = 0; i < currentBlocks.Count; i++)
            {
                currentBlocks[i].Issue(cmd);
            }
        }

        void OnButtonPressed(DrillCommand cmd)
        {
            IssueToAll(cmd);
        }
    }
}
