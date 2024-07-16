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
            "None",
            "GestureControlsSimple",
            "ButtonControlsSimple"
        };
    }
}