using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using PikePush.Utls;


namespace PikePush.Controls {

    public class ControlsManager : MonoBehaviour
    {

        [Flags]
        public enum Controls {
            Idle = 0,
            Left = 1,
            Right = 2,
            Up = 4,
            Down = 8,
            Space = 16,
            Escape = 32
        }

        // list of all available control schemes
        public static string[] controlSchemes = new string[] {
            "ButtonControlsSimple",
            "ExternalControlsSimple",
            "GestureControlsSimple",
            "TouchControlsSimple"
        };

        public static Dictionary<string, ControlScheme> controlSchemeInstances = new Dictionary<string, ControlScheme>() { };

        public void Awake()
        {
            LogHelper.debug($"[ControlsManager][Awake] Starting up...");

            // Snapshot every ControlScheme in the scene once (including
            // inactive — the GameObjects are deliberately disabled until
            // selected), then look up by name. Single Find call instead of
            // one per scheme.
            var allSchemes = UnityEngine.Object.FindObjectsByType<ControlScheme>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            var schemeByName = allSchemes.ToDictionary(s => s.name);

            foreach (string controlScheme in controlSchemes)
            {
                if (schemeByName.TryGetValue(controlScheme, out var instance))
                {
                    LogHelper.debug($"[ControlsManager][Awake] Adding schema: {controlScheme}");
                    instance.gameObject.SetActive(false);
                    controlSchemeInstances[controlScheme] = instance;
                }
            }

            // Auto-detect platform when no saved preference exists.
            int touchControlsDropdown = Int32.Parse(PlayerPrefs.GetString("TouchControlsDropdown", "0"));
            if (touchControlsDropdown == 0)
            {
                if (Application.platform == RuntimePlatform.Android
                    || Application.platform == RuntimePlatform.IPhonePlayer
                    || (Application.platform == RuntimePlatform.WebGLPlayer && IsTouchDevice()))
                {
                    touchControlsDropdown = 1; // ButtonControlsSimple
                    LogHelper.debug("[ControlsManager][Awake] Auto-detected touch platform, defaulting to ButtonControlsSimple");
                }
            }

            if (touchControlsDropdown > 0)
            {
                var x = controlSchemes[touchControlsDropdown - 1];
                var selectedControlScheme = controlSchemeInstances.First(o => o.Key == x);
                if (selectedControlScheme.Value && selectedControlScheme.Value.gameObject)
                {
                    LogHelper.debug($"[ControlsManager][Awake] activating schema '{touchControlsDropdown}': {selectedControlScheme.Key}");
                    selectedControlScheme.Value.gameObject.SetActive(true);
                }
            }

            // always enable external controls
            var scheme = "ExternalControlsSimple";
            if (controlSchemeInstances[scheme].gameObject)
            {
                LogHelper.debug($"[ControlsManager][Awake] activating schema: {scheme}");
                controlSchemeInstances[scheme].gameObject.SetActive(true);
            }
        }


#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool JS_IsTouchDevice();
#endif

        private static bool IsTouchDevice()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return JS_IsTouchDevice();
#else
            return SystemInfo.deviceType == DeviceType.Handheld;
#endif
        }

        public Controls InputCheck()
        {
            Controls activeControls = Controls.Idle;

            LogHelper.debug($"[ControlsManager][InputCheck]Available schemes: {string.Join(", ", controlSchemeInstances.Keys)}");
            foreach (var controlScheme in controlSchemeInstances)
            {
                LogHelper.debug($"[ControlsManager][InputCheck][{controlScheme.Key}]");

                if (controlScheme.Value && controlScheme.Value.isActiveAndEnabled)
                {
                    if (controlScheme.Value.controls.Left)
                    {
                        LogHelper.debug($"[ControlsManager][InputCheck][Left]");

                        activeControls |= ControlsManager.Controls.Left;
                    }
                    else if (controlScheme.Value.controls.Right)
                    {
                        LogHelper.debug($"[ControlsManager][InputCheck][Right]");

                        activeControls |= ControlsManager.Controls.Right;
                    }

                    if (controlScheme.Value.controls.Up)
                    {
                        LogHelper.debug($"[ControlsManager][InputCheck][Up]");

                        activeControls |= ControlsManager.Controls.Up;
                    }
                    else if (controlScheme.Value.controls.Down)
                    {
                        LogHelper.debug($"[ControlsManager][InputCheck][Down]");

                        activeControls |= ControlsManager.Controls.Down;
                    }

                    if (controlScheme.Value.controls.Escape)
                    {
                        LogHelper.debug($"[ControlsManager][InputCheck][Escape]");

                        activeControls |= ControlsManager.Controls.Escape;
                    }

                    if (controlScheme.Value.controls.Space)
                    {
                        LogHelper.debug($"[ControlsManager][InputCheck][Space]");

                        activeControls |= ControlsManager.Controls.Space;
                    }
                }
            }

            return activeControls;
        }
    }
}