using System.Collections.Generic;
using PikePush.Combat;
using PikePush.Drill.UI;
using PikePush.UI;
using PikePush.Utls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PikePush.Drill
{
    public class DrillBootstrap : MonoBehaviour
    {
        public const int MinFriendlyBlocks = 1;
        public const int MaxBlocksPerFaction = 4;
        const float BlockSpacingX = 8f;
        const float FriendlyZ = -10f;
        const float EnemyZ = 10f;
        const float MinContactRadius = 5f;
        // Block centroid halts this many units short of the visual field edge.
        const float FieldEdgeMargin = 5f;

        // HUD stacking constants.
        const float HudPerSideHeight = 180f;   // each MeterGame strip on screen
        const float HudPairSpacing   = 8f;     // gap between friendly/enemy bars within a pair
        const float HudEngagementGap = 24f;    // gap between engagement pairs
        const float HudBaseY         = 140f;   // bottom edge of stack, sits above the command panel
        const string MeterGamePrefabResourcePath = "MeterGame";

        [Header("Field")]
        [SerializeField] Color fieldColor = new Color(0.30f, 0.55f, 0.20f);
        [SerializeField] float fieldSize = 80f;

        [Header("Blocks")]
        [SerializeField] int initialFriendlyBlockCount = 1;
        [SerializeField] int initialEnemyBlockCount = 0;
        [SerializeField] int ranks = 4;
        [SerializeField] int files = 4;
        [SerializeField] GameObject soldierPrefab;

        [Header("Palettes")]
        [SerializeField] Color[] friendlyPalette = {
            new Color(0.30f, 0.45f, 0.85f), // Covenanter blue
            new Color(0.85f, 0.75f, 0.30f), // mustard
            new Color(0.40f, 0.75f, 0.45f), // sage
            new Color(0.65f, 0.55f, 0.40f), // hodden grey-brown
        };
        [SerializeField] Color[] enemyPalette = {
            new Color(0.75f, 0.20f, 0.20f), // royalist red
            new Color(0.55f, 0.15f, 0.35f), // burgundy
            new Color(0.45f, 0.20f, 0.45f), // plum
            new Color(0.85f, 0.45f, 0.20f), // ochre
        };

        readonly List<Block> friendly = new List<Block>();
        readonly List<Block> enemy = new List<Block>();
        readonly List<Engagement> engagements = new List<Engagement>();
        readonly Dictionary<Engagement, EngagementHud> huds = new Dictionary<Engagement, EngagementHud>();

        BlockSelector selector;
        BlockCountPanel friendlyPanel;
        BlockCountPanel enemyPanel;
        DrillToast toast;
        DrillContextHelp contextHelp;
        Canvas hudCanvas;
        Font uiFont;
        GameObject meterGamePrefab;

        public IReadOnlyList<Engagement> Engagements => engagements;

        struct EngagementHud
        {
            public MeterGame Friendly;
            public MeterGame Enemy;
            public Text FriendlyAdvantage;
            public Text EnemyAdvantage;
        }

        void Awake()
        {
            EnsureLighting();
            EnsureField();
            var cam = EnsureCamera();
            hudCanvas = EnsureCanvas();
            EnsureEventSystem();
            uiFont = DefaultUIFont();

            selector = EnsureSelector(cam);
            toast = DrillToast.Build(hudCanvas.transform, uiFont);
            EnsureCommandPanel(hudCanvas, selector);

            meterGamePrefab = Resources.Load<GameObject>(MeterGamePrefabResourcePath);
            if (meterGamePrefab == null)
            {
                ShowFatalBanner(hudCanvas, uiFont,
                    "MeterGame prefab missing\n\n" +
                    "Run Unity menu:\nPikePush  ▸  Regenerate MeterGame Prefab from Runner\n\n" +
                    "(should auto-generate on script reload — if you see this after a reload, the\n" +
                    "Game.unity scene didn't yield a MeterGame GameObject and the generator logged an error)");
            }

            friendlyPanel = BlockCountPanel.Build(hudCanvas.transform, uiFont,
                "Friendly", new Color(0.6f, 0.8f, 1f),
                new Vector2(-20f, -20f),
                () => friendly.Count, SpawnFriendly, RemoveLastFriendly,
                MinFriendlyBlocks, MaxBlocksPerFaction);

            enemyPanel = BlockCountPanel.Build(hudCanvas.transform, uiFont,
                "Enemy", new Color(1f, 0.6f, 0.6f),
                new Vector2(-20f, -86f),
                () => enemy.Count, SpawnEnemy, RemoveLastEnemy,
                0, MaxBlocksPerFaction);

            contextHelp = DrillContextHelp.Build(hudCanvas.transform, uiFont, selector, engagements);

            int nF = Mathf.Clamp(initialFriendlyBlockCount, MinFriendlyBlocks, MaxBlocksPerFaction);
            for (int i = 0; i < nF; i++) SpawnFriendly();

            int nE = Mathf.Clamp(initialEnemyBlockCount, 0, MaxBlocksPerFaction);
            for (int i = 0; i < nE; i++) SpawnEnemy();

            friendlyPanel.Refresh();
            enemyPanel.Refresh();
        }

        void Update()
        {
            DetectEngagements();
            TickEngagements();
            ResolveFinishedEngagements();
        }

        public int FriendlyCount => friendly.Count;
        public int EnemyCount => enemy.Count;

        public void SpawnFriendly() => Spawn(friendly, Faction.Friendly, friendlyPalette, FriendlyZ, yaw: 0f);
        public void SpawnEnemy()    => Spawn(enemy,    Faction.Enemy,    enemyPalette,    EnemyZ,    yaw: 180f);

        public void RemoveLastFriendly() { if (friendly.Count > MinFriendlyBlocks) RemoveLast(friendly); }
        public void RemoveLastEnemy()    { if (enemy.Count > 0)                     RemoveLast(enemy); }

        void Spawn(List<Block> roster, Faction faction, Color[] palette, float z, float yaw)
        {
            if (roster.Count >= MaxBlocksPerFaction) return;

            int index = roster.Count;
            string label = $"{faction} {index + 1}";
            var go = new GameObject(label.Replace(' ', '_'));
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            var block = go.AddComponent<Block>();
            block.ranks = ranks;
            block.files = files;
            block.label = label;
            block.soldierPrefab = soldierPrefab;
            block.soldierColor = palette[index % palette.Length];
            block.faction = faction;
            block.fieldHalfExtent = (fieldSize * 0.5f) - FieldEdgeMargin;

            roster.Add(block);
            RecenterRoster(roster, z);
            LogHelper.debug($"[DrillBootstrap] Spawned {label} (faction total={roster.Count})");
        }

        void RemoveLast(List<Block> roster)
        {
            int last = roster.Count - 1;
            if (last < 0) return;

            var block = roster[last];
            float z = block != null ? block.transform.position.z : 0f;
            roster.RemoveAt(last);
            RemoveBlock(block);
            RecenterRoster(roster, z);
        }

        // Distribute the roster symmetrically around x=0 along the faction's
        // z-line so the camera (which looks at the origin) stays framed on
        // the actual blocks. Engaged blocks stay put — they're committed to
        // their fight and shouldn't teleport.
        static void RecenterRoster(List<Block> roster, float z)
        {
            int n = roster.Count;
            for (int i = 0; i < n; i++)
            {
                var b = roster[i];
                if (b == null) continue;
                if (b.IsEngaged) continue;
                float x = (i - (n - 1) * 0.5f) * BlockSpacingX;
                b.transform.position = new Vector3(x, 0f, z);
            }
        }

        void RemoveBlock(Block block)
        {
            // Drop any engagements this block is part of, unlocking the other
            // side and tearing down the HUD. Then the selector, then the
            // GameObject. Dangling state must not outlive the block.
            for (int i = engagements.Count - 1; i >= 0; i--)
            {
                var eng = engagements[i];
                if (eng.A != block && eng.B != block) continue;

                var other = eng.A == block ? eng.B : eng.A;
                if (other != null) other.IsEngaged = false;
                DestroyHud(eng);
                engagements.RemoveAt(i);
            }
            if (selector != null) selector.Remove(block);
            if (block != null) Destroy(block.gameObject);
            RepositionHuds();
        }

        void DetectEngagements()
        {
            // O(N*M) over the two rosters; both capped at 4 = at most 16 checks
            // per frame. Skip pairs already engaged.
            for (int i = 0; i < friendly.Count; i++)
            {
                var f = friendly[i];
                if (f == null) continue;
                for (int j = 0; j < enemy.Count; j++)
                {
                    var e = enemy[j];
                    if (e == null) continue;
                    if (AlreadyEngaged(f, e)) continue;

                    if (FactionContact.InContact(f, e, MinContactRadius))
                        StartEngagement(f, e);
                }
            }
        }

        bool AlreadyEngaged(Block f, Block e)
        {
            for (int i = 0; i < engagements.Count; i++)
            {
                var eng = engagements[i];
                if ((eng.A == f && eng.B == e) || (eng.A == e && eng.B == f))
                    return true;
            }
            return false;
        }

        void StartEngagement(Block f, Block e)
        {
            LogHelper.debug($"[DrillBootstrap] ENGAGEMENT: {f.label} vs {e.label}");
            // Halt before flipping IsEngaged so the Halt command isn't gated
            // by the engagement lock that's about to come up.
            f.Issue(DrillCommand.Halt);
            e.Issue(DrillCommand.Halt);
            f.IsEngaged = true;
            e.IsEngaged = true;

            var eng = new Engagement(f, e);
            engagements.Add(eng);
            SpawnHud(eng);
            RepositionHuds();
        }

        // Each engagement gets two MeterGame instances — instantiated from the
        // exact same prefab the runner uses — one bound to each side's meter.
        // Friendly bar on top, enemy beneath. Multiple engagements stack
        // vertically above the command panel. Each bar is tinted with its
        // own block's soldierColor so the two halves read as the right side.
        void SpawnHud(Engagement eng)
        {
            if (hudCanvas == null || meterGamePrefab == null) return;
            var f = InstantiateHud(eng.A.label, eng.MeterA, eng.A.soldierColor);
            var e = InstantiateHud(eng.B.label, eng.MeterB, eng.B.soldierColor);
            huds[eng] = new EngagementHud
            {
                Friendly = f,
                Enemy = e,
                FriendlyAdvantage = f != null ? CreateAdvantageText(f.transform) : null,
                EnemyAdvantage    = e != null ? CreateAdvantageText(e.transform) : null,
            };
        }

        MeterGame InstantiateHud(string title, MeterModel model, Color fillColor)
        {
            var go = Object.Instantiate(meterGamePrefab, hudCanvas.transform, false);
            go.name = $"MeterGame_{title}";
            go.SetActive(true);

            var mg = go.GetComponent<MeterGame>();
            if (mg == null)
            {
                LogHelper.warn("[DrillBootstrap] MeterGame prefab missing MeterGame component — regenerate it");
                return null;
            }
            mg.BindExternal(model);
            mg.SetTitle(title);
            mg.SetFillColor(fillColor);
            return mg;
        }

        // Red advantage caption sitting just under the slider, fed each frame
        // from EngagementAdvantage. Empty when this side has no edge — so
        // matching stances on both blocks visually cancel out.
        Text CreateAdvantageText(Transform parent)
        {
            var go = new GameObject("Advantage");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
            // Sits below the slider (which the runner prefab anchors at y≈200).
            rt.anchoredPosition = new Vector2(0f, 130f);
            rt.sizeDelta = new Vector2(820f, 30f);

            var txt = go.AddComponent<Text>();
            txt.font = uiFont;
            txt.fontSize = 22;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(0.95f, 0.45f, 0.45f);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = string.Empty;
            txt.raycastTarget = false;
            return txt;
        }

        void DestroyHud(Engagement eng)
        {
            if (!huds.TryGetValue(eng, out var hud)) return;
            // Advantage texts are children of the MeterGame objects, so
            // destroying those takes the texts down too. Just null the
            // dictionary entry.
            if (hud.Friendly != null) Destroy(hud.Friendly.gameObject);
            if (hud.Enemy != null) Destroy(hud.Enemy.gameObject);
            huds.Remove(eng);
        }

        void RepositionHuds()
        {
            float y = HudBaseY;
            for (int i = 0; i < engagements.Count; i++)
            {
                if (!huds.TryGetValue(engagements[i], out var hud)) continue;
                SetAnchoredY(hud.Enemy,    y);                                  // enemy on bottom
                SetAnchoredY(hud.Friendly, y + HudPerSideHeight + HudPairSpacing);
                y += (HudPerSideHeight * 2f) + HudPairSpacing + HudEngagementGap;
            }
        }

        static void SetAnchoredY(MeterGame mg, float y)
        {
            if (mg == null) return;
            var rt = (RectTransform)mg.transform;
            var pos = rt.anchoredPosition;
            pos.y = y;
            rt.anchoredPosition = pos;
        }

        void TickEngagements()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < engagements.Count; i++)
            {
                var eng = engagements[i];

                // Live-recompute both fill and drain multipliers each frame so
                // a mid-engagement spacing change (e.g. forming Closest Order
                // to push back harder AND hold ground longer) takes effect
                // immediately.
                ApplyMultipliers(eng.MeterA, eng.A);
                ApplyMultipliers(eng.MeterB, eng.B);

                UpdateAdvantageTexts(eng);

                bool pushingA = ShouldPush(eng.A);
                bool pushingB = ShouldPush(eng.B);
                eng.Tick(dt, pushingA, pushingB);
            }
        }

        void UpdateAdvantageTexts(Engagement eng)
        {
            if (!huds.TryGetValue(eng, out var hud)) return;

            var (advA, advB) = EngagementAdvantage.Compute(
                eng.A.Posture, eng.A.Spacing, AttackType.PikePush,
                eng.B.Posture, eng.B.Spacing, AttackType.PikePush);

            ApplyAdvantage(hud.FriendlyAdvantage, advA);
            ApplyAdvantage(hud.EnemyAdvantage,    advB);
        }

        static void ApplyAdvantage(Text txt, EngagementAdvantage.Side side)
        {
            if (txt == null) return;
            if (!side.HasAdvantage) { txt.text = string.Empty; return; }

            string push = side.PushDelta > 0.5f ? $"Push +{Mathf.RoundToInt(side.PushDelta)}%" : null;
            string hold = side.HoldDelta > 0.5f ? $"Hold +{Mathf.RoundToInt(side.HoldDelta)}%" : null;
            if (push != null && hold != null) txt.text = $"{push}   ·   {hold}";
            else                              txt.text = push ?? hold;
        }

        // Both sides of a drill-mode engagement are pike blocks; the attacker
        // type is always PikePush. Campaign will compute the attacker type
        // per opposing block (cavalry mixed in).
        static void ApplyMultipliers(MeterModel meter, Block b)
        {
            if (meter == null || b == null) return;
            meter.FillRateMultiplier  = CounterMatrix.FillRateMultiplier(b.Posture, b.Spacing, AttackType.PikePush);
            meter.DrainRateMultiplier = CounterMatrix.DrainRateMultiplier(b.Posture, b.Spacing, AttackType.PikePush);
        }

        // Drill spar mode has no AI — both sides are player-driven via selection.
        // Push happens when the block is in the current selection AND the player
        // holds Space. Unselected blocks drain. Switching selection is part of
        // the mechanic — you have to choose where your push goes.
        bool ShouldPush(Block b)
        {
            if (b == null || selector == null) return false;
            if (!SelectionContains(b)) return false;
            return Input.GetKey(KeyCode.Space);
        }

        bool SelectionContains(Block b)
        {
            var sel = selector.Selected;
            for (int i = 0; i < sel.Count; i++)
                if (ReferenceEquals(sel[i], b)) return true;
            return false;
        }

        void ResolveFinishedEngagements()
        {
            for (int i = engagements.Count - 1; i >= 0; i--)
            {
                var eng = engagements[i];
                if (!eng.IsResolved) continue;

                // Remove first, then resolve. OnEngagementResolved calls
                // RemoveBlock, which scans `engagements` for entries that
                // contain the loser and removes them — so this entry has
                // to be out of the list first to avoid a double-remove
                // / index-shift bug.
                engagements.RemoveAt(i);
                OnEngagementResolved(eng);
            }
        }

        void OnEngagementResolved(Engagement eng)
        {
            LogHelper.debug($"[DrillBootstrap] WINNER: {eng.Winner?.label}  loser: {eng.Loser?.label}");

            // Tear down the HUD for this engagement first so it doesn't keep
            // showing a frozen meter.
            DestroyHud(eng);

            // Winner unlocks first — order matters because RemoveBlock can
            // strip the loser from the rosters and selector.
            if (eng.Winner != null) eng.Winner.IsEngaged = false;

            var loser = eng.Loser;
            if (loser != null)
            {
                if (!friendly.Remove(loser)) enemy.Remove(loser);
                RemoveBlock(loser);
            }
            friendlyPanel.Refresh();
            enemyPanel.Refresh();
            RepositionHuds();
        }

        void EnsureLighting()
        {
            if (FindAnyObjectByType<Light>() != null) return;

            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.97f, 0.88f);
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.65f, 0.80f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.55f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.22f, 0.18f);
        }

        void EnsureField()
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Field";
            plane.transform.localScale = Vector3.one * (fieldSize / 10f);

            var renderer = plane.GetComponent<Renderer>();
            var mat = new Material(renderer.sharedMaterial);
            mat.color = fieldColor;
            renderer.sharedMaterial = mat;
        }

        Camera EnsureCamera()
        {
            var existing = Camera.main;
            if (existing != null)
            {
                if (existing.GetComponent<DrillCamera>() == null)
                    existing.gameObject.AddComponent<DrillCamera>();
                return existing;
            }

            var go = new GameObject("Drill Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 50f;
            go.AddComponent<DrillCamera>();
            go.AddComponent<AudioListener>();
            return cam;
        }

        Canvas EnsureCanvas()
        {
            var existing = FindAnyObjectByType<Canvas>();
            if (existing != null) return existing;

            var go = new GameObject("Drill Canvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        BlockSelector EnsureSelector(Camera cam)
        {
            var existing = FindAnyObjectByType<BlockSelector>();
            if (existing != null) return existing;

            var go = new GameObject("BlockSelector");
            var s = go.AddComponent<BlockSelector>();
            s.Initialize(cam);
            return s;
        }

        DrillCommandPanel EnsureCommandPanel(Canvas canvas, BlockSelector sel)
        {
            var existing = FindAnyObjectByType<DrillCommandPanel>();
            if (existing != null) return existing;

            var panelGo = new GameObject("CommandPanel");
            panelGo.transform.SetParent(canvas.transform, false);
            var rect = panelGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 20f);
            rect.sizeDelta = new Vector2(-40f, 110f);

            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.55f);

            var layoutGo = new GameObject("Buttons");
            layoutGo.transform.SetParent(panelGo.transform, false);
            var layoutRect = layoutGo.AddComponent<RectTransform>();
            layoutRect.anchorMin = Vector2.zero;
            layoutRect.anchorMax = Vector2.one;
            layoutRect.offsetMin = new Vector2(10f, 10f);
            layoutRect.offsetMax = new Vector2(-10f, -10f);

            var layout = layoutGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var panel = panelGo.AddComponent<DrillCommandPanel>();
            panel.Initialize(sel, layoutRect, uiFont, toast);

            return panel;
        }

        // Center-of-screen red banner for fatal-but-recoverable bootstrap
        // issues (e.g. missing prefab). Silent warnings in the console are
        // why iterating on this mode has been painful — make it loud.
        static void ShowFatalBanner(Canvas canvas, Font font, string message)
        {
            LogHelper.warn($"[DrillBootstrap][FATAL] {message}");
            if (canvas == null) return;

            var go = new GameObject("DrillFatalBanner");
            go.transform.SetParent(canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(960f, 320f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.55f, 0.08f, 0.08f, 0.96f);
            bg.raycastTarget = false;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.AddComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(24f, 24f);
            trt.offsetMax = new Vector2(-24f, -24f);
            var txt = textGo.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = message;
            txt.raycastTarget = false;
        }

        static Font DefaultUIFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) return font;
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

    }
}
