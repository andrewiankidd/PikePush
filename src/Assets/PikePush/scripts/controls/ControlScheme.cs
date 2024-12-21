using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PikePush.Controls
{

    public class ControlScheme : MonoBehaviour
    {
        public ControlInputs controls = new ControlInputs();

        public bool Enabled(bool state)
        {
            Debug.Log($"Enabled: {state}");
            return state;
        }
    }
}