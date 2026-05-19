using UnityEngine;

namespace PikePush.Drill
{
    public class DrillCamera : MonoBehaviour
    {
        [SerializeField] Vector3 lookTarget = Vector3.zero;
        [SerializeField] float distance = 18f;
        [SerializeField] float pitchDegrees = 55f;
        [SerializeField] float yawDegrees = 0f;
        [SerializeField] float panSpeed = 0.06f;
        [SerializeField] float zoomSpeed = 6f;
        [SerializeField] float minDistance = 6f;
        [SerializeField] float maxDistance = 40f;

        void LateUpdate()
        {
            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * zoomSpeed, minDistance, maxDistance);

            // Pan only via middle-mouse drag — keyboard (WASD/arrows) is reserved for
            // command-panel keybindings so we don't accidentally control the camera
            // while typing commands.
            if (Input.GetMouseButton(2))
            {
                Vector3 forward = Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.forward;
                Vector3 right = Quaternion.Euler(0f, yawDegrees, 0f) * Vector3.right;
                float mx = Input.GetAxis("Mouse X");
                float my = Input.GetAxis("Mouse Y");
                lookTarget -= (right * mx + forward * my) * panSpeed * distance;
            }

            Quaternion rot = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);
            transform.position = lookTarget - rot * Vector3.forward * distance;
            transform.rotation = rot;
        }
    }
}
