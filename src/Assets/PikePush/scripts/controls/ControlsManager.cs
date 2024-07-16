using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PikePush.Controls {

    public class ControlsManager
    {

        [Flags]
        public enum Controls {
            Idle,
            Left,
            Right,
            Up,
            Down
        }

        public static List<string> ControlSchemes = new List<string>() {
            "GestureControlsSimple",
            "ButtonControlsSimple",
            "TouchControlsSimple"
        };

        public Controls testicle(ButtonControlsSimple ButtonControlsSimpleInstance)
        {
            ControlsManager.Controls activeControls = ControlsManager.Controls.Idle;

            if (ButtonControlsSimpleInstance.isActiveAndEnabled)
            {
                if (ButtonControlsSimpleInstance.controls.Left)
                {
                    Debug.Log(ButtonControlsSimpleInstance.controls.Left);

                    activeControls |= ControlsManager.Controls.Left;
                }
                else if (ButtonControlsSimpleInstance.controls.Right)
                {
                    Debug.Log(ButtonControlsSimpleInstance.controls.Right);

                    activeControls |= ControlsManager.Controls.Right;
                }
                if (ButtonControlsSimpleInstance.controls.Up)
                {
                    Debug.Log(ButtonControlsSimpleInstance.controls.Up);

                    activeControls |= ControlsManager.Controls.Up;
                }
                else if (ButtonControlsSimpleInstance.controls.Down)
                {
                    Debug.Log(ButtonControlsSimpleInstance.controls.Down);

                    activeControls |= ControlsManager.Controls.Down;
                }
            }

            return activeControls;
        }
    }
}