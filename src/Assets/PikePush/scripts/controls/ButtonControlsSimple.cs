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

    public class ButtonControlsSimple : MonoBehaviour
    {

        // Instantiate the Controls class
        public ControlInputs controls = new ControlInputs();

        public bool Enabled(bool state)
        {
            Debug.Log($"Enabled: {state}");
            return state;
        }

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

        public void SwipeStart(BaseEventData eventData)
        {

        }

        public void SwipeEnd(BaseEventData eventData)
        {

        }
    }
}