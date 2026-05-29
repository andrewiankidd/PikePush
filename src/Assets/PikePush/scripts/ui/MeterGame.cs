using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using PikePush.Combat;
using PikePush.Controls;
using PikePush.Utls;


namespace PikePush.UI {

    public class MeterGame : MonoBehaviour
    {
        [SerializeField]
        private ControlsManager controlsManager;

        // UI elements
        [SerializeField] private Image meterFill;
        [SerializeField] private Text meterPercentageText;
        [SerializeField] private Slider meterSlider;

        // Tuning — kept on the MonoBehaviour so existing scene / prefab values
        // continue to override the runtime defaults set on MeterModel.
        [SerializeField] private float fillRate = 0.6f;
        [SerializeField] private float drainRate = 0.35f;
        [SerializeField] private float startValue = 0.5f;

        readonly MeterModel meter = new MeterModel();
        TaskCompletionSource<bool> tcs;

        public async Task<bool> Show()
        {
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

            ControlsManager.Controls activeControls = this.controlsManager.InputCheck();
            bool pushing = activeControls.HasFlag(ControlsManager.Controls.Space);
            meter.Tick(Time.deltaTime, pushing);

            UpdateUI();
            LogHelper.debug($"[MeterGame][Update]: {meter.Value}");

            switch (meter.Result)
            {
                case MeterResult.Won: OnSuccess(); break;
                case MeterResult.Lost: OnFail(); break;
            }
        }

        private void UpdateUI()
        {
            float value = meter.Value;
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
            meter.FillRate = fillRate;
            meter.DrainRate = drainRate;
            meter.StartValue = startValue;
            meter.Reset();
            UpdateUI();
        }

        public bool IsMeterFull() => meter.Result == MeterResult.Won;
        public bool IsMeterEmpty() => meter.Result == MeterResult.Lost;
    }

}
