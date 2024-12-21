using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

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
            Debug.Log($"[ControlsManager][Awake] Starting up...");

            // find gameobject for each available controlscheme
            foreach (string controlScheme in controlSchemes)
            {
                var controlSchemeGameObject = GameObject.FindObjectsOfType<ControlScheme>(true).First(o => o.name == controlScheme);
                if (controlSchemeGameObject)
                {
                    Debug.Log($"[ControlsManager][Awake] Adding schema: {controlScheme}");
                    controlSchemeGameObject.gameObject.SetActive(false);
                    if (!controlSchemeInstances.ContainsKey(controlScheme))
                    {
                        controlSchemeInstances.Add(controlScheme, controlSchemeGameObject);
                    }
                }
                
            }

            // todo lol
            string touchControlsDropdown = PlayerPrefs.GetString("TouchControlsDropdown");
            if (touchControlsDropdown == "1")
            {
                if (controlSchemeInstances["GestureControlsSimple"].gameObject)
                {
                    controlSchemeInstances["GestureControlsSimple"].gameObject.SetActive(true);
                }
            }
            else if (touchControlsDropdown == "2")
            {
                if (controlSchemeInstances["ButtonControlsSimple"].gameObject)
                {
                    controlSchemeInstances["ButtonControlsSimple"].gameObject.SetActive(true);
                }
            }
            else if (touchControlsDropdown == "3")
            {
                if (controlSchemeInstances["TouchControlsSimple"].gameObject)
                {
                    controlSchemeInstances["TouchControlsSimple"].gameObject.SetActive(true);
                }
            }

            if (controlSchemeInstances["ExternalControlsSimple"] && controlSchemeInstances["ExternalControlsSimple"].gameObject)
            {
                controlSchemeInstances["ExternalControlsSimple"].gameObject.SetActive(true);
            }
        }

        public Controls InputCheck()
        {
            Controls activeControls = Controls.Idle;

            Debug.Log($"[ControlsManager][InputCheck]Available schemes: {string.Join(", ", controlSchemeInstances.Keys)}");
            foreach (var controlScheme in controlSchemeInstances)
            {
                Debug.Log($"[ControlsManager][InputCheck][{controlScheme.Key}]");

                if (controlScheme.Value && controlScheme.Value.isActiveAndEnabled)
                {
                    if (controlScheme.Value.controls.Left)
                    {
                        Debug.Log($"[ControlsManager][InputCheck][Left]");

                        activeControls |= ControlsManager.Controls.Left;
                    }
                    else if (controlScheme.Value.controls.Right)
                    {
                        Debug.Log($"[ControlsManager][InputCheck][Right]");

                        activeControls |= ControlsManager.Controls.Right;
                    }

                    if (controlScheme.Value.controls.Up)
                    {
                        Debug.Log($"[ControlsManager][InputCheck][Up]");

                        activeControls |= ControlsManager.Controls.Up;
                    }
                    else if (controlScheme.Value.controls.Down)
                    {
                        Debug.Log($"[ControlsManager][InputCheck][Down]");

                        activeControls |= ControlsManager.Controls.Down;
                    }

                    if (controlScheme.Value.controls.Escape)
                    {
                        Debug.Log($"[ControlsManager][InputCheck][Escape]");

                        activeControls |= ControlsManager.Controls.Escape;
                    }

                    if (controlScheme.Value.controls.Space)
                    {
                        Debug.Log($"[ControlsManager][InputCheck][Space]");

                        activeControls |= ControlsManager.Controls.Space;
                    }
                }
            }

            return activeControls;
        }
    }
}