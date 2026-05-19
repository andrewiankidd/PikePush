using System.Collections.Generic;
using PikePush.Drill.UI;
using PikePush.Utls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PikePush.Drill
{
    public class DrillBootstrap : MonoBehaviour
    {
        [Header("Field")]
        [SerializeField] Color fieldColor = new Color(0.30f, 0.55f, 0.20f);
        [SerializeField] float fieldSize = 80f;

        [Header("Blocks")]
        [SerializeField] int blockCount = 1;
        [SerializeField] int ranks = 4;
        [SerializeField] int files = 4;
        [SerializeField] GameObject soldierPrefab;
        [SerializeField] Color[] blockColors = {
            new Color(0.75f, 0.30f, 0.30f),
            new Color(0.30f, 0.45f, 0.85f),
            new Color(0.85f, 0.75f, 0.30f),
        };

        readonly List<Block> spawnedBlocks = new List<Block>();

        void Awake()
        {
            EnsureLighting();
            EnsureField();
            var cam = EnsureCamera();
            var canvas = EnsureCanvas();
            EnsureEventSystem();

            var selector = EnsureSelector(cam);
            var panel = EnsureCommandPanel(canvas, selector);

            SpawnBlocks();
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
            var selector = go.AddComponent<BlockSelector>();
            selector.Initialize(cam);
            return selector;
        }

        DrillCommandPanel EnsureCommandPanel(Canvas canvas, BlockSelector selector)
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
            panel.Initialize(selector, layoutRect, DefaultUIFont());

            return panel;
        }

        void SpawnBlocks()
        {
            for (int i = 0; i < blockCount; i++)
            {
                var go = new GameObject($"Block_{i + 1}");
                go.transform.position = new Vector3((i - (blockCount - 1) * 0.5f) * 8f, 0f, 0f);
                var block = go.AddComponent<Block>();
                block.ranks = ranks;
                block.files = files;
                block.label = $"Block {i + 1}";
                block.soldierPrefab = soldierPrefab;
                block.soldierColor = blockColors[i % blockColors.Length];
                spawnedBlocks.Add(block);
            }
            LogHelper.debug($"[DrillBootstrap] Spawned {spawnedBlocks.Count} block(s)");
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
