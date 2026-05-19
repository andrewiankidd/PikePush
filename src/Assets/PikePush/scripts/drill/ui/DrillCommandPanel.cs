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

        public static readonly Entry[] CommandSet =
        {
            new Entry(DrillCommand.Halt,            "Halt",              KeyCode.H),
            new Entry(DrillCommand.ForwardMarch,    "Forward March",     KeyCode.M),
            new Entry(DrillCommand.LeftFace,        "Left Face",         KeyCode.A),
            new Entry(DrillCommand.RightFace,       "Right Face",        KeyCode.D),
            new Entry(DrillCommand.AboutFaceLeft,   "About Face L",      KeyCode.Q),
            new Entry(DrillCommand.AboutFaceRight,  "About Face R",      KeyCode.E),
            new Entry(DrillCommand.OpenOrder,       "Open Order",        KeyCode.O),
            new Entry(DrillCommand.CloseOrder,      "Close Order",       KeyCode.C),
            new Entry(DrillCommand.ClosestOrder,    "Closest Order",     KeyCode.V),
            new Entry(DrillCommand.PrepareForHorse, "Prepare for Horse", KeyCode.B),
        };

        Block currentBlock;
        readonly List<DrillCommandButton> buttons = new List<DrillCommandButton>();

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
            if (currentBlock == null) return;
            foreach (var entry in CommandSet)
            {
                if (Input.GetKeyDown(entry.Key))
                {
                    LogHelper.debug($"[DrillCommandPanel] Key {entry.Key} → {entry.Command}");
                    currentBlock.Issue(entry.Command);
                }
            }
        }

        void OnSelectionChanged(Block block)
        {
            LogHelper.debug($"[DrillCommandPanel] OnSelectionChanged: {(block != null ? block.label : "<none>")}");
            currentBlock = block;
            SetVisible(block != null);

            ClearButtons();
            if (block == null) return;

            BuildButtons();
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

            foreach (var entry in CommandSet)
            {
                var btn = DrillCommandButton.Build(buttonContainer, entry.Command, entry.Label, entry.Key,
                    buttonFont, OnButtonPressed);
                buttons.Add(btn);
            }
        }

        void OnButtonPressed(DrillCommand cmd)
        {
            if (currentBlock != null) currentBlock.Issue(cmd);
        }
    }
}
