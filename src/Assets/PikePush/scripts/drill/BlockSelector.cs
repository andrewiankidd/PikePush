using System;
using PikePush.Utls;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PikePush.Drill
{
    public class BlockSelector : MonoBehaviour
    {
        Camera viewCamera;

        public Block Selected { get; private set; }
        public event Action<Block> SelectionChanged;

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
            if (!Input.GetMouseButtonDown(0)) return;
            if (PointerOverUI()) return;

            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                var block = hit.collider.GetComponentInParent<Block>();
                Select(block);
            }
            else
            {
                Select(null);
            }
        }

        void Select(Block block)
        {
            if (ReferenceEquals(block, Selected)) return;
            LogHelper.debug($"[BlockSelector] Selected: {(block != null ? block.label : "<none>")}");
            Selected = block;
            SelectionChanged?.Invoke(block);
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
