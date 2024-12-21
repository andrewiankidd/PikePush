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
using PikePush.Utls;


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
                LogHelper.debug($"[SettingsMenuManager][Awake]controlOption: {controlOption}");
                TouchControlsDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(){ text = controlOption });
            }

        }

        // Start is called before the first frame update
        void Start () {

            int defaultControlScheme = 0;

            // todo lol
            if (
                Application.platform is RuntimePlatform.Android
                or RuntimePlatform.IPhonePlayer
                or RuntimePlatform.WebGLPlayer
                or RuntimePlatform
            ) {
                defaultControlScheme = 1;
            }

            int nNumber = int.TryParse(PlayerPrefs.GetString("TouchControlsDropdown"), out nNumber) ? nNumber : defaultControlScheme;
            LogHelper.debug($"[SettingsMenuManager][Start]TouchControlsDropdown.SetValueWithoutNotify: {nNumber}");
            TouchControlsDropdown.value = (nNumber);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnTouchControlsDropdownValueChanged() {
            LogHelper.debug($"[SettingsMenuManager][OnTouchControlsDropdownValueChanged]value: {TouchControlsDropdown.value}");
            PlayerPrefs.SetString("TouchControlsDropdown", TouchControlsDropdown.value.ToString());
        }

        public void BackMenu() {
            SceneManager.LoadScene("MainMenu");
        }
    }
}