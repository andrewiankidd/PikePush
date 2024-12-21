using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using PikePush.Controls;

namespace PikePush.UI {

    public class MeterGame : MonoBehaviour
    {
        [SerializeField]
        private ControlsManager controlsManager;

        // UI element to represent the meter
        [SerializeField] private Image meterFill; // Image used for the meter
        [SerializeField] private Text meterPercentageText; // Text to show the percentage
        [SerializeField] private Slider meterSlider; // Optional Slider as another representation

        // The rate at which the meter increases when the player presses the space key
        [SerializeField] private float fillRate = 0.1f;

        // The rate at which the meter decreases over time
        [SerializeField] private float drainRate = 0.5f;

        // The value of the meter (0 to 1)
        private float meterValue = 0.5f;

        private TaskCompletionSource<bool> tcs; // Used to await the user's choice

        public async Task<bool> Show()
        {
            if (!this.gameObject.activeInHierarchy)
            {
                this.ResetGame();
                this.gameObject.SetActive(true);
            }
            Debug.Log($"[MeterGame][Show]");

            tcs = new TaskCompletionSource<bool>();
            bool result = await tcs.Task;

            this.gameObject.SetActive(false);

            return result;
        }

        public void Update()
        {
            if (!this.gameObject.activeInHierarchy)
                return;

            // get inputs
            ControlsManager.Controls activeControls = this.controlsManager.InputCheck();

            if (activeControls.HasFlag(ControlsManager.Controls.Space))
            {
                IncreaseMeter(fillRate);
            }
            else
            {
                DecreaseMeter(drainRate);
            }

            // Update the UI elements
            UpdateUI();
            Debug.Log($"[MeterGame][Update]: {meterValue}");

            // Check if the player has failed (meter reaches 0)
            if (IsMeterEmpty())
            {
                OnFail();
            }
            else if (IsMeterFull())
            {
                OnSuccess();
            }
        }

        // Updates the UI elements to reflect the current meter value
        private void UpdateUI()
        {
            if (meterFill != null)
            {
                Debug.Log($"[MeterGame][UpdateUI][meterFill]: {meterValue}");
                meterFill.fillAmount = meterValue; // For Image-based representation
            }

            if (meterPercentageText != null)
            {
                Debug.Log($"[MeterGame][UpdateUI][meterPercentageText]: {meterValue}");
                meterPercentageText.text = $"{(int)(meterValue * 100)}%"; // Show percentage
            }

            if (meterSlider != null)
            {
                Debug.Log($"[MeterGame][UpdateUI][meterSlider]: {meterValue}");
                meterSlider.value = meterValue; // For Slider-based representation
            }
        }

        // Called when the player fails
        private void OnSuccess()
        {
            Debug.Log("[MeterGame][OnSuccess]");
            tcs.SetResult(true);
            this.gameObject.SetActive(false);
        }

        // Called when the player fails
        private void OnFail()
        {
            Debug.Log("[MeterGame][OnFail]");
            tcs.SetResult(false);
            this.gameObject.SetActive(false);
        }

        // Public method to reset the meter
        public void ResetGame()
        {
            meterValue = 0.5f;
            UpdateUI();
        }

        // Public method to increase the meter value
        public void IncreaseMeter(float amount)
        {
            if (!IsMeterFull())
            {
                meterValue += amount;
            }
        }

        // Public method to decrease the meter value
        public void DecreaseMeter(float amount)
        {
            if (!IsMeterEmpty())
            {
                Debug.Log($"[MeterGame][DecreaseMeter] '{amount}'");
                meterValue -= amount % Time.deltaTime;
            }

        }

        // // Public method to set the meter value directly
        // public void SetMeterValue(float value)
        // {
        //     meterValue = (value);
        //     UpdateUI();
        // }

        // Public method to check if the meter is full
        public bool IsMeterFull()
        {
            return meterValue >= 1;
        }

        // Public method to check if the meter is empty
        public bool IsMeterEmpty()
        {
            return meterValue <= 0;
        }
    }

}