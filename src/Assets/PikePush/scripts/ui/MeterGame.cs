using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using PikePush.Combat;
using PikePush.Controls;
using PikePush.Utls;


namespace PikePush.UI {

    // The Push-of-Pike mash bar. Two modes:
    //
    //   Standalone (runner) — set up in-scene with serialised UI refs.
    //     Owns its MeterModel, polls ControlsManager for input, resolves
    //     Won/Lost via OnSuccess/OnFail, exposes async Show() for callers.
    //
    //   External-bound (drill / campaign) — instantiated from the MeterGame
    //     prefab and given a MeterModel by the caller. Pure renderer: no
    //     input, no auto-resolve. Caller drives Tick on its own model.
    //
    // Same prefab in both modes so the visual is identical.
    public class MeterGame : MonoBehaviour
    {
        [SerializeField] private ControlsManager controlsManager;

        // UI elements — inspector-wired on the prefab. titleText is optional
        // (the runner's prefab leaves it null because the title is a static
        // "Push!" sibling label; drill sets it per engagement via SetTitle).
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

        // Drill / campaign call this after instantiating the prefab. From this
        // point on, MeterGame is a pure view of the supplied model — no input,
        // no auto-resolve, no async Show().
        public void BindExternal(MeterModel external)
        {
            boundModel = external;
        }

        public void SetTitle(string title)
        {
            if (titleText != null) titleText.text = title;
        }

        // Tint the slider fill so two stacked meters in a drill engagement
        // read as their respective blocks (blue friendly vs red enemy etc).
        // Handles both the directly-wired meterFill (when one is set on the
        // prefab) and the inner Image on the Slider's fillRect (the runner
        // prefab uses a Slider, not a raw fill Image).
        public void SetFillColor(Color color)
        {
            if (meterFill != null) meterFill.color = color;
            if (meterSlider != null && meterSlider.fillRect != null)
            {
                var img = meterSlider.fillRect.GetComponent<Image>();
                if (img != null) img.color = color;
            }
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
