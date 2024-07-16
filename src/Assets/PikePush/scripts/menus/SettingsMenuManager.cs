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

        public TMP_Dropdown TouchControlsDropdown;

        void Awake () {
            // TouchControlsDropdown
            TouchControlsDropdown = GameObject.Find("TouchControlsDropdown").GetComponent<TMP_Dropdown>();
            // TouchControlsDropdown.options.Clear();
            foreach (string controlOption in Controls.ControlsManager.ControlSchemes)
            {
                Debug.Log($"[SettingsMenuManager][Awake]controlOption: {controlOption}");
                TouchControlsDropdown.options.Add(new TMP_Dropdown.OptionData(){ text = controlOption });
            }

        }

        // Start is called before the first frame update
        void Start () {
            int nNumber = int.TryParse(PlayerPrefs.GetString("TouchControlsDropdown"), out nNumber) ? nNumber : 0;
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