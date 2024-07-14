using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchControlsSimple : MonoBehaviour
{
    // Encapsulate the Controls class within TouchControlsSimple
    public class Controls
    {
        public bool Left { get; set; } = false;
        public bool Right { get; set; } = false;
        public bool Up { get; set; } = false;
        public bool Down { get; set; } = false;

        public object this[string controlName]
        {
            get
            {
                Type controlType = typeof(Controls);
                PropertyInfo controlInfo = controlType.GetProperty(controlName);
                return controlInfo.GetValue(this, null);
            }
            set
            {
                Type controlType = typeof(Controls);
                PropertyInfo controlInfo = controlType.GetProperty(controlName);
                Debug.Log($"controlInfo: {controlName}, {controlInfo}");
                controlInfo.SetValue(this, value, null);
            }
        }
    }

    // Instantiate the Controls class
    public Controls controls = new Controls();

    // Method called on pointer down event
    public void OnPointerDown(BaseEventData eventData)
    {
        Debug.Log($"OnPointerDown: {eventData.selectedObject.name}");
        this.controls[eventData.selectedObject.name] = true;
    }

    // Method called on pointer up event
    public void OnPointerUp(BaseEventData eventData)
    {
        Debug.Log($"OnPointerUp: {eventData.selectedObject.name}");
        this.controls[eventData.selectedObject.name] = false;
    }
}