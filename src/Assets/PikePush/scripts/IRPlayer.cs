using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PikePush.Controls;
using PikePush.UI;

namespace PikePush {

    [RequireComponent(typeof(Rigidbody))]

    public class IRPlayer : MonoBehaviour
    {
        [SerializeField]
        private ControlsManager controlsManager;

        public static float gravity = 20.0f;
        public static float jumpHeight = 2.5f;
        public static float strafeSpeed = 2.75f;
        public static float movementSpeed = 4f;

        Rigidbody r;
        bool grounded = false;
        Vector3 defaultScale;
        bool crouching = false;

        // Start is called before the first frame update
        public void Start()
        {
            r = GetComponent<Rigidbody>();
            r.constraints = RigidbodyConstraints.FreezePositionZ;
            r.freezeRotation = true;
            r.useGravity = false;
            defaultScale = transform.localScale;
        }

        public void Update()
        {
            #region control checks
            ControlsManager.Controls activeControls = this.controlsManager.InputCheck();
            #endregion

            #region game controls
            if (grounded) {
                if (activeControls.HasFlag(ControlsManager.Controls.Up))
                {
                    Debug.Log("[IRPlayer][Update][Jump]");
                    r.velocity = new Vector3(r.velocity.x, CalculateJumpVerticalSpeed(), r.velocity.z);
                }
                else {
                    if (activeControls.HasFlag(ControlsManager.Controls.Down))
                    {
                        Debug.Log("[IRPlayer][Update][Crouch]");
                        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(defaultScale.x, defaultScale.y * 0.4f, defaultScale.z), Time.deltaTime * 7);

                        if (!this.crouching)
                        {
                            IRPlayer.movementSpeed = IRPlayer.movementSpeed / 2;
                            this.crouching = true;
                        }
                        Debug.Log($"[IRPlayer][Update][Crouch] IRPlayer.movementSpeed: {IRPlayer.movementSpeed}");
                    }
                    else
                    {
                        transform.localScale = Vector3.Lerp(transform.localScale, defaultScale, Time.deltaTime * 7);

                        if (this.crouching)
                        {
                            Debug.Log("[IRPlayer][Update][UnCrouch]");
                            IRPlayer.movementSpeed = IRPlayer.movementSpeed * 2;
                            this.crouching = false;
                        }
                    }
                }

                if (activeControls.HasFlag(ControlsManager.Controls.Left) && r.position.x > -strafeSpeed)
                {
                    Debug.Log("[IRPlayer][Update][StrafeLeft]");
                    r.position = new Vector3(r.position.x - (Time.deltaTime * strafeSpeed), r.position.y, r.position.z);
                }
                else if (activeControls.HasFlag(ControlsManager.Controls.Right) && r.position.x < strafeSpeed)
                {
                    Debug.Log("[IRPlayer][Update][StrafeRight]");
                    r.position = new Vector3(r.position.x + (Time.deltaTime * strafeSpeed), r.position.y, r.position.z);
                }
            }

            #endregion
        }

        // Update is called once per frame
        public void FixedUpdate()
        {
            // We apply gravity manually for more tuning control
            r.AddForce(new Vector3(0, -gravity * r.mass, 0));
            grounded = false;
        }

        public void OnCollisionStay()
        {
            grounded = true;
        }

        float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed
            // for the character to reach at the apex.
            return Mathf.Sqrt(2 * jumpHeight * gravity);
        }

        public void OnCollisionEnter(Collision collision)
        {
            try {
                Debug.Log($"[IRPlayer][OnCollisionEnter]: {collision.gameObject.tag}");
                switch (collision.gameObject.tag)
                {
                    case "Finish":
                        MainGame.instance.gameOver = true;
                        break;
                    case "Fight":
                        MainGame.instance.startFight(collision.gameObject);
                        break;
                    default:
                        Debug.LogWarning($"Collision with unknown object");
                        break;
                }
            } catch {
                Debug.Log("Error handling collision");
                Debug.Log(MainGame.instance);
                Application.Quit();
            }
        }
    }
}
