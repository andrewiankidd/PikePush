
using System;
using System.Reflection;
namespace PikePush.Controls {

    // Encapsulate the Controls class within GestureControlsSimple
    public class ControlInputs
    {
        public bool Idle { get; set; } = false;
        public bool Left { get; set; } = false;
        public bool Right { get; set; } = false;
        public bool Up { get; set; } = false;
        public bool Down { get; set; } = false;
        public bool Space { get; set; } = false;
        public bool Escape { get; set; } = false;

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