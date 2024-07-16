
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace PikePush.Controls {

    // Encapsulate the Controls class within GestureControlsSimple
    public class ControlInputs
    {
        public bool Idle { get; set; } = false;
        public bool Left { get; set; } = false;
        public bool Right { get; set; } = false;
        public bool Up { get; set; } = false;
        public bool Down { get; set; } = false;

        public object this[string controlName]
        {
            get
            {
                Type controlType = typeof(ControlInputs);
                PropertyInfo controlInfo = controlType.GetProperty(controlName);
                return controlInfo.GetValue(this, null);
            }
            set
            {
                Type controlType = typeof(ControlInputs);
                PropertyInfo controlInfo = controlType.GetProperty(controlName);
                controlInfo.SetValue(this, value, null);
            }
        }
    }
}