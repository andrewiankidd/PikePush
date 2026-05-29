using System.Collections.Generic;
using PikePush.Combat;
using PikePush.Drill.UI;
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

        BlockSelector selector;
        BlockCountPanel friendlyPanel;
        BlockCountPanel enemyPanel;
        EngagementOverviewPanel engagementPanel;
        Font uiFont;

        public IReadOnlyList<Engagement> Engagements => engagements;

        void Awake()
        {
            EnsureLighting();
            EnsureField();
            var cam = EnsureCamera();
            var canvas = EnsureCanvas();
            EnsureEventSystem();
            uiFont = DefaultUIFont();

            selector = EnsureSelector(cam);
            EnsureCommandPanel(canvas, selector);

            friendlyPanel = BlockCountPanel.Build(canvas.transform, uiFont,
                "Friendly", new Color(0.6f, 0.8f, 1f),
                new Vector2(-20f, -20f),
                () => friendly.Count, SpawnFriendly, RemoveLastFriendly,
                MinFriendlyBlocks, MaxBlocksPerFaction);

            enemyPanel = BlockCountPanel.Build(canvas.transform, uiFont,
                "Enemy", new Color(1f, 0.6f, 0.6f),
                new Vector2(-20f, -86f),
                () => enemy.Count, SpawnEnemy, RemoveLastEnemy,
                0, MaxBlocksPerFaction);

            engagementPanel = EngagementOverviewPanel.Build(canvas.transform, uiFont, engagements);

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
            go.transform.position = new Vector3((index - (MaxBlocksPerFaction - 1) * 0.5f) * BlockSpacingX, 0f, z);
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            var block = go.AddComponent<Block>();
            block.ranks = ranks;
            block.files = files;
            block.label = label;
            block.soldierPrefab = soldierPrefab;
            block.soldierColor = palette[index % palette.Length];
            block.faction = faction;

            roster.Add(block);
            LogHelper.debug($"[DrillBootstrap] Spawned {label} (faction total={roster.Count})");
        }

        void RemoveLast(List<Block> roster)
        {
            int last = roster.Count - 1;
            if (last < 0) return;

            var block = roster[last];
            roster.RemoveAt(last);
            RemoveBlock(block);
        }

        void RemoveBlock(Block block)
        {
            // Drop any engagements this block is part of, then the selector,
            // then the GameObject. Dangling state must not outlive the block.
            for (int i = engagements.Count - 1; i >= 0; i--)
            {
                var eng = engagements[i];
                if (eng.A == block || eng.B == block)
                    engagements.RemoveAt(i);
            }
            if (selector != null) selector.Remove(block);
            if (block != null) Destroy(block.gameObject);
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
            f.Issue(DrillCommand.Halt);
            e.Issue(DrillCommand.Halt);
            engagements.Add(new Engagement(f, e));
        }

        void TickEngagements()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < engagements.Count; i++)
            {
                var eng = engagements[i];
                bool pushingA = ShouldPush(eng.A);
                bool pushingB = ShouldPush(eng.B);
                eng.Tick(dt, pushingA, pushingB);
            }
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
                OnEngagementResolved(eng);
                engagements.RemoveAt(i);
            }
        }

        void OnEngagementResolved(Engagement eng)
        {
            LogHelper.debug($"[DrillBootstrap] WINNER: {eng.Winner?.label}  loser: {eng.Loser?.label}");

            // The loser breaks — remove from the field. The winner stays put.
            var loser = eng.Loser;
            if (loser != null)
            {
                if (!friendly.Remove(loser)) enemy.Remove(loser);
                RemoveBlock(loser);
            }
            friendlyPanel.Refresh();
            enemyPanel.Refresh();
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
            panel.Initialize(sel, layoutRect, uiFont);

            return panel;
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
