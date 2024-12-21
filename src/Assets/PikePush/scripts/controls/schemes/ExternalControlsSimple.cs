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

    public class ExternalControlsSimple : ControlScheme
    {

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("[ExternalControlsSimple][Update][W]");
                this.controls.Up = true;
            }
            else
            {
                this.controls.Up = false;

            }

            if (Input.GetKey(KeyCode.S))
            {
                Debug.Log("[ExternalControlsSimple][Update][S]");
                this.controls.Down = true;
            }
            else
            {
                this.controls.Down = false;

            }

            if (Input.GetKey(KeyCode.A))
            {
                Debug.Log("[ExternalControlsSimple][Update][A]");
                this.controls.Left = true;
            }
            else
            {
                this.controls.Left = false;

            }

            if (Input.GetKey(KeyCode.D))
            {
                Debug.Log("[ExternalControlsSimple][Update][D]");
                this.controls.Right = true;
            }
            else
            {
                this.controls.Right = false;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[ExternalControlsSimple][Update][Esc]");
                this.controls.Escape = true;
            }
            else
            {
                this.controls.Escape = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[ExternalControlsSimple][Update][Space]");
                this.controls.Space = true;
            }
            else
            {
                this.controls.Space = false;
            }

        }
    }
}