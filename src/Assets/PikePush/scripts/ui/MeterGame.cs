using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using PikePush.Combat;
using PikePush.Controls;
using PikePush.Utls;


namespace PikePush.UI {

    // The Push-of-Pike mash bar. Two modes:
    //
    //   Standalone (runner)        — set up in-scene with serialised UI refs.
    //                                Owns its MeterModel, polls ControlsManager
    //                                for input, resolves Won/Lost via OnSuccess/
    //                                OnFail, exposes async Show() for callers.
    //
    //   External-bound (drill etc) — built dynamically via BuildDynamic, given a
    //                                MeterModel by the caller (e.g. an
    //                                Engagement.MeterA). Pure renderer: no input,
    //                                no auto-resolve. The caller drives Tick.
    //
    // Single visual + percentage representation across both modes so a player
    // sees the same mash bar wherever push-of-pike happens.
    public class MeterGame : MonoBehaviour
    {
        [SerializeField] private ControlsManager controlsManager;

        // UI elements (inspector-wired for the runner; programmatically wired
        // by BuildDynamic for drill).
        [SerializeField] private Image meterFill;
        [SerializeField] private Text meterPercentageText;
        [SerializeField] private Slider meterSlider;
        [SerializeField] private Text titleText;

        // Tuning — kept on the MonoBehaviour so existing scene / prefab values
        // continue to override the runtime defaults set on MeterModel.
        [SerializeField] private float fillRate = 0.6f;
        [SerializeField] private float drainRate = 0.35f;
        [SerializeField] private float startValue = 0.5f;

        readonly MeterModel ownModel = new MeterModel();
        MeterModel boundModel;
        TaskCompletionSource<bool> tcs;

        public MeterModel Meter => boundModel ?? ownModel;
        bool IsExternal => boundModel != null;

        // Drill-mode entry point. Spins up the same visual the runner uses,
        // bound to an externally-owned MeterModel. Caller (DrillBootstrap)
        // ticks the model itself and disposes this instance when the
        // engagement ends.
        public static MeterGame BuildDynamic(Transform canvasParent, Font font, string title, MeterModel model)
            => BuildDynamic(canvasParent, font, title, model, new Color(0.30f, 0.65f, 0.95f));

        public static MeterGame BuildDynamic(Transform canvasParent, Font font, string title, MeterModel model, Color fillColor)
        {
            var go = new GameObject($"MeterGame_{title}");
            go.transform.SetParent(canvasParent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 150f);
            rect.sizeDelta = new Vector2(800f, 200f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(go.transform, false);
            var trect = titleGo.AddComponent<RectTransform>();
            trect.anchorMin = new Vector2(0f, 1f);
            trect.anchorMax = new Vector2(1f, 1f);
            trect.pivot = new Vector2(0.5f, 1f);
            trect.anchoredPosition = new Vector2(0f, -10f);
            trect.sizeDelta = new Vector2(0f, 48f);
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = font;
            titleTxt.fontSize = 28;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = Color.white;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.text = title;
            titleTxt.raycastTarget = false;

            // The mash bar itself — built as a Slider so it visually matches
            // the runner. Interactable off — we render, the model drives.
            var sliderGo = new GameObject("Slider");
            sliderGo.transform.SetParent(go.transform, false);
            var srect = sliderGo.AddComponent<RectTransform>();
            srect.anchorMin = new Vector2(0.5f, 0.5f);
            srect.anchorMax = new Vector2(0.5f, 0.5f);
            srect.pivot = new Vector2(0.5f, 0.5f);
            srect.anchoredPosition = new Vector2(0f, -10f);
            srect.sizeDelta = new Vector2(720f, 60f);

            var sBg = new GameObject("Background");
            sBg.transform.SetParent(sliderGo.transform, false);
            var sBgRect = sBg.AddComponent<RectTransform>();
            sBgRect.anchorMin = Vector2.zero;
            sBgRect.anchorMax = Vector2.one;
            sBgRect.offsetMin = Vector2.zero;
            sBgRect.offsetMax = Vector2.zero;
            var sBgImg = sBg.AddComponent<Image>();
            sBgImg.color = new Color(0.10f, 0.10f, 0.12f, 0.95f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            var faRect = fillArea.AddComponent<RectTransform>();
            faRect.anchorMin = new Vector2(0f, 0f);
            faRect.anchorMax = new Vector2(1f, 1f);
            faRect.offsetMin = new Vector2(4f, 4f);
            faRect.offsetMax = new Vector2(-4f, -4f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillArea.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.raycastTarget = false;

            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = model != null ? model.Value : 0.5f;
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            var pctGo = new GameObject("Percent");
            pctGo.transform.SetParent(sliderGo.transform, false);
            var prect = pctGo.AddComponent<RectTransform>();
            prect.anchorMin = Vector2.zero;
            prect.anchorMax = Vector2.one;
            prect.offsetMin = Vector2.zero;
            prect.offsetMax = Vector2.zero;
            var pctTxt = pctGo.AddComponent<Text>();
            pctTxt.font = font;
            pctTxt.fontSize = 22;
            pctTxt.fontStyle = FontStyle.Bold;
            pctTxt.color = Color.white;
            pctTxt.alignment = TextAnchor.MiddleCenter;
            pctTxt.raycastTarget = false;

            var hintGo = new GameObject("Hint");
            hintGo.transform.SetParent(go.transform, false);
            var hrect = hintGo.AddComponent<RectTransform>();
            hrect.anchorMin = new Vector2(0f, 0f);
            hrect.anchorMax = new Vector2(1f, 0f);
            hrect.pivot = new Vector2(0.5f, 0f);
            hrect.anchoredPosition = new Vector2(0f, 10f);
            hrect.sizeDelta = new Vector2(0f, 30f);
            var hintTxt = hintGo.AddComponent<Text>();
            hintTxt.font = font;
            hintTxt.fontSize = 16;
            hintTxt.color = new Color(0.85f, 0.85f, 0.85f);
            hintTxt.alignment = TextAnchor.MiddleCenter;
            hintTxt.text = "Mash [SPACE]";
            hintTxt.raycastTarget = false;

            var mg = go.AddComponent<MeterGame>();
            mg.titleText = titleTxt;
            mg.meterFill = fillImg;
            mg.meterPercentageText = pctTxt;
            mg.meterSlider = slider;
            mg.BindExternal(model);
            return mg;
        }

        public void BindExternal(MeterModel external)
        {
            boundModel = external;
        }

        public void SetTitle(string title)
        {
            if (titleText != null) titleText.text = title;
        }

        public async Task<bool> Show()
        {
            if (IsExternal)
            {
                LogHelper.warn("[MeterGame] Show() is for standalone mode; bound instances are display-only");
                return false;
            }

            if (!this.gameObject.activeInHierarchy)
            {
                this.ResetGame();
                this.gameObject.SetActive(true);
            }
            LogHelper.debug($"[MeterGame][Show]");

            tcs = new TaskCompletionSource<bool>();
            bool result = await tcs.Task;

            this.gameObject.SetActive(false);
            return result;
        }

        public void Update()
        {
            if (!this.gameObject.activeInHierarchy) return;

            if (!IsExternal)
            {
                if (controlsManager == null) return; // not yet configured
                // Standalone: poll input + tick + auto-resolve.
                ControlsManager.Controls activeControls = this.controlsManager.InputCheck();
                bool pushing = activeControls.HasFlag(ControlsManager.Controls.Space);
                ownModel.Tick(Time.deltaTime, pushing);

                UpdateUI();

                switch (ownModel.Result)
                {
                    case MeterResult.Won: OnSuccess(); break;
                    case MeterResult.Lost: OnFail(); break;
                }
            }
            else
            {
                // Bound: caller ticks the model; we only render.
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            float value = Meter.Value;
            if (meterFill != null) meterFill.fillAmount = value;
            if (meterPercentageText != null) meterPercentageText.text = $"{(int)(value * 100)}%";
            if (meterSlider != null) meterSlider.value = value;
        }

        private void OnSuccess()
        {
            LogHelper.debug("[MeterGame][OnSuccess]");
            tcs?.TrySetResult(true);
            this.gameObject.SetActive(false);
        }

        private void OnFail()
        {
            LogHelper.debug("[MeterGame][OnFail]");
            tcs?.TrySetResult(false);
            this.gameObject.SetActive(false);
        }

        public void ResetGame()
        {
            ownModel.FillRate = fillRate;
            ownModel.DrainRate = drainRate;
            ownModel.StartValue = startValue;
            ownModel.Reset();
            UpdateUI();
        }

        public bool IsMeterFull() => Meter.Result == MeterResult.Won;
        public bool IsMeterEmpty() => Meter.Result == MeterResult.Lost;
    }

}
