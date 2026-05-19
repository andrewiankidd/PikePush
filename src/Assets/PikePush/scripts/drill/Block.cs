using System.Collections.Generic;
using PikePush.Utls;
using UnityEngine;

namespace PikePush.Drill
{
    public class Block : MonoBehaviour
    {
        [Header("Formation")]
        [SerializeField] public int ranks = 4;
        [SerializeField] public int files = 4;
        [SerializeField] public float baseFileSpacing = 1.2f;
        [SerializeField] public float baseRankSpacing = 1.2f;

        [Header("Motion")]
        [SerializeField] public float marchSpeed = 1.3f;
        [SerializeField] public float turnDegreesPerSecond = 180f;
        [SerializeField] public float spacingLerpRate = 3f;
        [SerializeField] public float braceCrouchYScale = 0.85f;

        [Header("Appearance")]
        [SerializeField] public string label = "Block";
        [SerializeField] public GameObject soldierPrefab;
        [SerializeField] public Color soldierColor = new Color(0.65f, 0.55f, 0.40f);

        public bool IsMarching { get; private set; }
        public bool IsBracing { get; private set; }
        public float GoalYawDegrees { get; private set; }
        public float CurrentYawDegrees { get; private set; }
        public float CurrentSpacingMultiplier { get; private set; } = 1f;

        const float SpacingOpen = 1.6f;
        const float SpacingClosed = 0.85f;
        const float SpacingClosest = 0.6f;
        float targetSpacingMultiplier = 1f;
        float targetYScale = 1f;

        readonly List<Soldier> members = new List<Soldier>();
        BoxCollider selectionCollider;

        void Awake()
        {
            CurrentYawDegrees = transform.eulerAngles.y;
            GoalYawDegrees = CurrentYawDegrees;
            selectionCollider = GetComponent<BoxCollider>();
            if (selectionCollider == null)
                selectionCollider = gameObject.AddComponent<BoxCollider>();
        }

        void Start()
        {
            BuildFormation();
            UpdateSelectionCollider();
        }

        void Update()
        {
            StepYaw();

            if (IsMarching)
                transform.position += transform.forward * marchSpeed * Time.deltaTime;

            CurrentSpacingMultiplier = Mathf.Lerp(
                CurrentSpacingMultiplier, targetSpacingMultiplier, Time.deltaTime * spacingLerpRate);

            var s = transform.localScale;
            s.y = Mathf.Lerp(s.y, targetYScale, Time.deltaTime * 4f);
            transform.localScale = s;
        }

        void StepYaw()
        {
            float diff = GoalYawDegrees - CurrentYawDegrees;
            float step = turnDegreesPerSecond * Time.deltaTime;
            if (Mathf.Abs(diff) <= step)
                CurrentYawDegrees = GoalYawDegrees;
            else
                CurrentYawDegrees += Mathf.Sign(diff) * step;

            transform.rotation = Quaternion.Euler(0f, CurrentYawDegrees, 0f);
        }

        public void Issue(DrillCommand cmd)
        {
            LogHelper.debug($"[Block:{label}][Issue] {cmd}");

            // Most movement commands cancel bracing.
            if (cmd != DrillCommand.PrepareForHorse && cmd != DrillCommand.Halt)
                ExitBracing();

            switch (cmd)
            {
                case DrillCommand.Halt:
                    IsMarching = false;
                    break;
                case DrillCommand.ForwardMarch:
                    IsMarching = true;
                    break;
                case DrillCommand.RightFace:
                    GoalYawDegrees += 90f;
                    break;
                case DrillCommand.LeftFace:
                    GoalYawDegrees -= 90f;
                    break;
                case DrillCommand.AboutFaceRight:
                    GoalYawDegrees += 180f;
                    break;
                case DrillCommand.AboutFaceLeft:
                    GoalYawDegrees -= 180f;
                    break;
                case DrillCommand.OpenOrder:
                    targetSpacingMultiplier = SpacingOpen;
                    break;
                case DrillCommand.CloseOrder:
                    targetSpacingMultiplier = SpacingClosed;
                    break;
                case DrillCommand.ClosestOrder:
                    targetSpacingMultiplier = SpacingClosest;
                    break;
                case DrillCommand.PrepareForHorse:
                    EnterBracing();
                    break;
            }
        }

        void EnterBracing()
        {
            IsBracing = true;
            IsMarching = false;
            targetYScale = braceCrouchYScale;
        }

        void ExitBracing()
        {
            if (!IsBracing) return;
            IsBracing = false;
            targetYScale = 1f;
        }

        public Vector3 LocalSlot(int rankIndex, int fileIndex)
        {
            float fileSpacing = baseFileSpacing * CurrentSpacingMultiplier;
            float rankSpacing = baseRankSpacing * CurrentSpacingMultiplier;

            float xCenterOffset = (files - 1) * 0.5f;
            float zCenterOffset = (ranks - 1) * 0.5f;

            float x = (fileIndex - xCenterOffset) * fileSpacing;
            float z = (zCenterOffset - rankIndex) * rankSpacing;
            return new Vector3(x, 0f, z);
        }

        void BuildFormation()
        {
            bool usingPikeman = soldierPrefab != null;
            for (int rank = 0; rank < ranks; rank++)
            {
                for (int file = 0; file < files; file++)
                {
                    var go = usingPikeman
                        ? Instantiate(soldierPrefab, transform)
                        : BuildPrimitiveSoldier();

                    go.name = $"{label}_R{rank}F{file}";
                    var slot = LocalSlot(rank, file);
                    go.transform.localPosition = slot;
                    go.transform.localRotation = Quaternion.identity;

                    var soldier = go.GetComponent<Soldier>();
                    if (soldier == null) soldier = go.AddComponent<Soldier>();
                    soldier.Configure(this, rank, file);

                    if (usingPikeman)
                        PikemanCustomizer.Customize(go);
                    else
                        ApplySoldierColor(go);

                    members.Add(soldier);
                }
            }
        }

        GameObject BuildPrimitiveSoldier()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.transform.SetParent(transform, worldPositionStays: false);
            var col = go.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            return go;
        }

        void ApplySoldierColor(GameObject go)
        {
            var renderer = go.GetComponentInChildren<Renderer>();
            if (renderer == null) return;
            var mat = new Material(renderer.sharedMaterial);
            mat.color = soldierColor;
            renderer.sharedMaterial = mat;
        }

        void UpdateSelectionCollider()
        {
            float width = files * baseFileSpacing + 1f;
            float depth = ranks * baseRankSpacing + 1f;
            selectionCollider.center = new Vector3(0f, 1f, 0f);
            selectionCollider.size = new Vector3(width, 2f, depth);
            selectionCollider.isTrigger = false;
        }
    }
}
