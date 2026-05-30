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
        [SerializeField] public float wheelDegreesPerSecond = 30f;
        [SerializeField] public float spacingLerpRate = 3f;
        [SerializeField] public float braceCrouchYScale = 0.85f;

        [Header("Appearance")]
        [SerializeField] public string label = "Block";
        [SerializeField] public GameObject soldierPrefab;
        [SerializeField] public Color soldierColor = new Color(0.65f, 0.55f, 0.40f);

        [Header("Faction")]
        [SerializeField] public Faction faction = Faction.Friendly;
        public Faction Faction => faction;

        [Header("Field")]
        // X/Z half-extent of the playable field, set by the spawner. A
        // marching block whose next centroid position would exceed this on
        // either axis halts itself. 999 = effectively unbounded if unset.
        [SerializeField] public float fieldHalfExtent = 999f;

        public bool IsMarching { get; private set; }
        public PikePosture Posture { get; private set; } = PikePosture.Order;
        public SpacingOrder Spacing { get; private set; } = SpacingOrder.Order;
        public bool IsWheeling { get; private set; }
        public bool IsBracing => Posture == PikePosture.ChargeForHorse;
        // Set by the engagement system (DrillBootstrap / future Campaign). When
        // true, AllowsCommand locks out movement and facings — you can only
        // change posture or spacing, halt, or reform.
        public bool IsEngaged { get; set; }
        // Toggled by BlockSelector to drive the on-ground highlight ring.
        public bool IsSelected { get; set; }

        public float GoalYawDegrees { get; private set; }
        public float CurrentYawDegrees { get; private set; }
        public float CurrentSpacingMultiplier { get; private set; } = 1f;

        public BlockState State => new BlockState
        {
            Posture = Posture,
            Spacing = Spacing,
            IsWheeling = IsWheeling,
            IsMarching = IsMarching,
            IsEngaged = IsEngaged,
        };

        float targetSpacingMultiplier = 1f;
        float targetYScale = 1f;
        int wheelDirection;

        readonly List<Soldier> members = new List<Soldier>();
        BoxCollider selectionCollider;
        LineRenderer selectionRing;

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
            BuildSelectionRing();
        }

        void Update()
        {
            if (IsWheeling)
                GoalYawDegrees += wheelDirection * wheelDegreesPerSecond * Time.deltaTime;

            StepYaw();

            if (IsMarching)
            {
                Vector3 next = transform.position + transform.forward * marchSpeed * Time.deltaTime;
                if (FieldBounds.IsOutside(next, fieldHalfExtent))
                {
                    // At the edge — halt cleanly. Player has to rotate the
                    // block back toward the field and re-issue Forward March.
                    IsMarching = false;
                    IsWheeling = false;
                    LogHelper.debug($"[Block:{label}] Halted at field boundary");
                }
                else
                {
                    transform.position = next;
                }
            }

            CurrentSpacingMultiplier = Mathf.Lerp(
                CurrentSpacingMultiplier, targetSpacingMultiplier, Time.deltaTime * spacingLerpRate);

            var s = transform.localScale;
            s.y = Mathf.Lerp(s.y, targetYScale, Time.deltaTime * 4f);
            transform.localScale = s;

            if (selectionRing != null) selectionRing.enabled = IsSelected;
        }

        // A thin coloured rectangle on the ground around the block — the
        // selection cue BlockSelector flips on/off via IsSelected. Sits
        // slightly above y=0 so it doesn't z-fight with the field plane.
        void BuildSelectionRing()
        {
            var go = new GameObject("SelectionRing");
            go.transform.SetParent(transform, false);

            var width = files * baseFileSpacing + 1.4f;
            var depth = ranks * baseRankSpacing + 1.4f;
            var hx = width * 0.5f;
            var hz = depth * 0.5f;
            const float y = 0.05f;

            selectionRing = go.AddComponent<LineRenderer>();
            selectionRing.useWorldSpace = false;
            selectionRing.loop = true;
            selectionRing.positionCount = 4;
            selectionRing.SetPosition(0, new Vector3(-hx, y, -hz));
            selectionRing.SetPosition(1, new Vector3( hx, y, -hz));
            selectionRing.SetPosition(2, new Vector3( hx, y,  hz));
            selectionRing.SetPosition(3, new Vector3(-hx, y,  hz));
            selectionRing.startWidth = 0.18f;
            selectionRing.endWidth = 0.18f;
            selectionRing.material = new Material(Shader.Find("Sprites/Default"));
            // Brighten the block's own colour so the ring reads against the field
            // but still tracks the faction theming.
            var c = Color.Lerp(soldierColor, Color.white, 0.4f);
            selectionRing.startColor = c;
            selectionRing.endColor = c;
            selectionRing.enabled = false;
        }

        void StepYaw()
        {
            float diff = GoalYawDegrees - CurrentYawDegrees;
            float step = (IsWheeling ? wheelDegreesPerSecond : turnDegreesPerSecond) * Time.deltaTime;
            if (Mathf.Abs(diff) <= step)
                CurrentYawDegrees = GoalYawDegrees;
            else
                CurrentYawDegrees += Mathf.Sign(diff) * step;

            transform.rotation = Quaternion.Euler(0f, CurrentYawDegrees, 0f);
        }

        public bool AllowsCommand(DrillCommand cmd) => BlockRules.AllowsCommand(cmd, State);

        public void Issue(DrillCommand cmd)
        {
            if (!AllowsCommand(cmd))
            {
                LogHelper.debug($"[Block:{label}][Issue] {cmd} — disallowed by current state, ignoring");
                return;
            }
            LogHelper.debug($"[Block:{label}][Issue] {cmd}");

            if (BlockRules.IsPosture(cmd)) { ApplyPosture(cmd); return; }
            if (BlockRules.IsSpacing(cmd)) { ApplySpacing(cmd); return; }
            if (BlockRules.IsFacing(cmd)) { ApplyFacing(cmd); return; }

            switch (cmd)
            {
                case DrillCommand.Halt:
                    IsMarching = false;
                    IsWheeling = false;
                    break;
                case DrillCommand.ForwardMarch:
                    IsMarching = true;
                    break;
                case DrillCommand.MarchOn:
                    IsWheeling = false;
                    IsMarching = true;
                    break;
                case DrillCommand.RightHandWheel:
                    IsWheeling = true;
                    IsMarching = true;
                    wheelDirection = 1;
                    break;
                case DrillCommand.LeftHandWheel:
                    IsWheeling = true;
                    IsMarching = true;
                    wheelDirection = -1;
                    break;
                case DrillCommand.WheelMidstRight:
                    IsWheeling = true;
                    IsMarching = true;
                    wheelDirection = 1;
                    // TODO: midst-pivot positioning offset
                    break;
                case DrillCommand.WheelMidstLeft:
                    IsWheeling = true;
                    IsMarching = true;
                    wheelDirection = -1;
                    // TODO: midst-pivot positioning offset
                    break;
                case DrillCommand.Reform:
                    // Reset to a clean state: at-Order spacing, Advance posture, halted.
                    IsMarching = false;
                    IsWheeling = false;
                    ApplySpacing(DrillCommand.OrderSpacing);
                    ApplyPosture(DrillCommand.AdvanceYourPike);
                    break;
                case DrillCommand.PrepareToCountermarchMaintainingGround:
                case DrillCommand.PrepareToCountermarchLosingGround:
                case DrillCommand.PrepareToCountermarchGainingGround:
                    // TODO: stage flag for the next Countermarch
                    break;
                case DrillCommand.Countermarch:
                    GoalYawDegrees += 180f;
                    break;
                default:
                    // V1 state-only stubs: doublings, filing, inversion.
                    // Recorded in [docs/backlog.md] — visuals follow with the animation suite.
                    break;
            }
        }

        void ApplyPosture(DrillCommand cmd)
        {
            switch (cmd)
            {
                case DrillCommand.OrderYourPike:     Posture = PikePosture.Order;          break;
                case DrillCommand.AdvanceYourPike:   Posture = PikePosture.Advance;        break;
                case DrillCommand.ShoulderYourPike:  Posture = PikePosture.Shoulder;       break;
                case DrillCommand.ChargeYourPike:    Posture = PikePosture.Charge;         break;
                case DrillCommand.ChargeToTheRear:   Posture = PikePosture.ChargeRear;     break;
                case DrillCommand.Port:              Posture = PikePosture.Port;           break;
                case DrillCommand.LowPortYourPike:   Posture = PikePosture.LowPort;        break;
                case DrillCommand.ShortenYourPike:   Posture = PikePosture.Shorten;        break;
                case DrillCommand.ChargeForHorse:    Posture = PikePosture.ChargeForHorse; break;
                case DrillCommand.FormCircle:        Posture = PikePosture.FormCircle;     break;
                case DrillCommand.TrailYourPike:     Posture = PikePosture.Trail;          break;
            }

            // ChargeForHorse holds you in a braced crouch; everything else stands tall.
            targetYScale = Posture == PikePosture.ChargeForHorse ? braceCrouchYScale : 1f;
            if (Posture == PikePosture.ChargeForHorse) IsMarching = false;
        }

        void ApplySpacing(DrillCommand cmd)
        {
            switch (cmd)
            {
                case DrillCommand.ClosestOrder:           Spacing = SpacingOrder.Closest;             break;
                case DrillCommand.CloseOrder:             Spacing = SpacingOrder.Close;               break;
                case DrillCommand.OrderSpacing:           Spacing = SpacingOrder.Order;               break;
                case DrillCommand.OpenOrder:              Spacing = SpacingOrder.Open;                break;
                case DrillCommand.DoubleDistance:         Spacing = SpacingOrder.DoubleDistance;      break;
                case DrillCommand.TwiceDoubleDistance:    Spacing = SpacingOrder.TwiceDoubleDistance; break;
                // Directional file/rank variants land at the same end-state spacing for V1.
                case DrillCommand.FilesOpenOrder:
                case DrillCommand.FilesOpenOrderFromLeft:
                case DrillCommand.FilesOpenOrderFromMidst:
                case DrillCommand.RanksOpenOrder:
                case DrillCommand.RanksOpenOrderFromRear:
                case DrillCommand.RanksAndFilesOpenOrder:
                    Spacing = SpacingOrder.Open;
                    break;
            }
            targetSpacingMultiplier = SpacingMultiplier(Spacing);
        }

        void ApplyFacing(DrillCommand cmd)
        {
            switch (cmd)
            {
                case DrillCommand.RightHandFace:        GoalYawDegrees += 90f;  break;
                case DrillCommand.LeftHandFace:         GoalYawDegrees -= 90f;  break;
                case DrillCommand.LeftHandAboutFace:    GoalYawDegrees -= 180f; break;
                case DrillCommand.RightHandAboutFace:   GoalYawDegrees += 180f; break;
                case DrillCommand.FaceToTheFront:       /* no-op, intent is "all face same way" */ break;
                case DrillCommand.RightHandIncline:     /* on-march drift — TODO */ break;
                case DrillCommand.LeftHandIncline:      /* on-march drift — TODO */ break;
                case DrillCommand.FaceToFrontAndRear:   /* split-face — TODO */ break;
                case DrillCommand.FaceToBothFlanks:     /* split-face — TODO */ break;
            }
        }

        static float SpacingMultiplier(SpacingOrder s)
        {
            switch (s)
            {
                case SpacingOrder.Closest:             return 0.60f;
                case SpacingOrder.Close:               return 0.85f;
                case SpacingOrder.Order:               return 1.00f;
                case SpacingOrder.Open:                return 1.60f;
                case SpacingOrder.DoubleDistance:      return 2.40f;
                case SpacingOrder.TwiceDoubleDistance: return 4.00f;
            }
            return 1.0f;
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
