using UnityEngine;
using PikePush.Utls;

namespace PikePush.Controls {

    public class ExternalControlsSimple : ControlScheme
    {

        public void Update()
        {
            this.controls = new ControlInputs();
            LogHelper.debug("[ExternalControlsSimple][Update]");

            if (Input.GetKeyDown(KeyCode.W))
            {
                LogHelper.debug("[ExternalControlsSimple][Update][W]");
                this.controls.Up = true;
            }

            if (Input.GetKey(KeyCode.S))
            {
                LogHelper.debug("[ExternalControlsSimple][Update][S]");
                this.controls.Down = true;
            }

            if (Input.GetKey(KeyCode.A))
            {
                LogHelper.debug("[ExternalControlsSimple][Update][A]");
                this.controls.Left = true;
            }

            if (Input.GetKey(KeyCode.D))
            {
                LogHelper.debug("[ExternalControlsSimple][Update][D]");
                this.controls.Right = true;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LogHelper.debug("[ExternalControlsSimple][Update][Esc]");
                this.controls.Escape = true;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                LogHelper.debug("[ExternalControlsSimple][Update][Space]");
                this.controls.Space = true;
            }

        }
    }
}