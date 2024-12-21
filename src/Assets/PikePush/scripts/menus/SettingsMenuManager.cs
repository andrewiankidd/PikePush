#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PikePush.Controls;

namespace PikePush.Menus {

    public class SettingsMenuManager : MonoBehaviour
    {

        public TMPro.TMP_Dropdown TouchControlsDropdown;

        void Awake () {
            // TouchControlsDropdown
            TouchControlsDropdown = GameObject.Find("TouchControlsDropdown").GetComponent<TMPro.TMP_Dropdown>();
            // TouchControlsDropdown.options.Clear();
            foreach (string controlOption in Controls.ControlsManager.controlSchemes)
            {
                Debug.Log($"[SettingsMenuManager][Awake]controlOption: {controlOption}");
                TouchControlsDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(){ text = controlOption });
            }

        }

        // Start is called before the first frame update
        void Start () {

            int defaultControlScheme = 0;

            if (Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer or RuntimePlatform.WebGLPlayer or RuntimePlatform) {
                defaultControlScheme = 2;
            }

            int nNumber = int.TryParse(PlayerPrefs.GetString("TouchControlsDropdown"), out nNumber) ? nNumber : defaultControlScheme;
            Debug.Log($"[SettingsMenuManager][Start]TouchControlsDropdown.SetValueWithoutNotify: {nNumber}");
            TouchControlsDropdown.value = (nNumber);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnTouchControlsDropdownValueChanged() {
            Debug.Log($"[SettingsMenuManager][OnTouchControlsDropdownValueChanged]value: {TouchControlsDropdown.value}");
            PlayerPrefs.SetString("TouchControlsDropdown", TouchControlsDropdown.value.ToString());
        }

        public void BackMenu() {
            SceneManager.LoadScene("MainMenu");
        }
    }
}