﻿using UnityEngine;
using PikePush.Utls;

namespace PikePush.Controls
{

    public class ControlScheme : MonoBehaviour
    {
        public ControlInputs controls = new ControlInputs();

        public bool Enabled(bool state)
        {
            LogHelper.debug($"Enabled: {state}");
            return state;
        }
    }
}