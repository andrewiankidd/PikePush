using System;
using System.Collections.Generic;
using PikePush.Utls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PikePush.Drill
{
    public class BlockSelector : MonoBehaviour
    {
        Camera viewCamera;
        readonly List<Block> selected = new List<Block>();

        public IReadOnlyList<Block> Selected => selected;
        public Block Primary => selected.Count > 0 ? selected[0] : null;
        public event Action<IReadOnlyList<Block>> SelectionChanged;

        public void Initialize(Camera viewCamera)
        {
            this.viewCamera = viewCamera;
        }

        void Awake()
        {
            if (viewCamera == null) viewCamera = Camera.main;
        }

        void Update()
        {
            if (viewCamera == null) return;
            // Esc-to-clear is owned by DrillCommandPanel so a submenu-Back
            // doesn't also clobber the selection on the same frame.
            if (!Input.GetMouseButtonDown(0)) return;
            if (PointerOverUI()) return;

            bool additive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                var block = hit.collider.GetComponentInParent<Block>();
                if (block != null)
                {
                    if (additive) Toggle(block);
                    else SelectExclusive(block);
                    return;
                }
            }

            // Clicked empty ground — clear unless additive (shift-click on nothing is a no-op).
            if (!additive) Clear();
        }

        public void Notify()
        {
            SyncSelectionFlags();
            SelectionChanged?.Invoke(selected);
        }

        public void Clear()
        {
            if (selected.Count == 0) return;
            foreach (var b in selected) if (b != null) b.IsSelected = false;
            selected.Clear();
            LogHelper.debug("[BlockSelector] Cleared selection");
            SelectionChanged?.Invoke(selected);
        }

        public void Remove(Block block)
        {
            if (block == null) return;
            if (selected.Remove(block))
            {
                block.IsSelected = false;
                SelectionChanged?.Invoke(selected);
            }
        }

        void SelectExclusive(Block block)
        {
            if (selected.Count == 1 && ReferenceEquals(selected[0], block)) return;
            foreach (var b in selected) if (b != null) b.IsSelected = false;
            selected.Clear();
            selected.Add(block);
            block.IsSelected = true;
            LogHelper.debug($"[BlockSelector] Selected: {block.label}");
            SelectionChanged?.Invoke(selected);
        }

        void Toggle(Block block)
        {
            if (selected.Remove(block))
            {
                block.IsSelected = false;
                LogHelper.debug($"[BlockSelector] Deselected: {block.label}");
            }
            else
            {
                selected.Add(block);
                block.IsSelected = true;
                LogHelper.debug($"[BlockSelector] Added: {block.label}");
            }
            SelectionChanged?.Invoke(selected);
        }

        // Defensive — keep IsSelected flags coherent if external code mutated
        // the list directly (currently nobody does, but cheap safety).
        void SyncSelectionFlags()
        {
            foreach (var b in selected) if (b != null) b.IsSelected = true;
        }

        static bool PointerOverUI()
        {
            if (EventSystem.current == null) return false;

            if (Input.touchCount > 0)
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
