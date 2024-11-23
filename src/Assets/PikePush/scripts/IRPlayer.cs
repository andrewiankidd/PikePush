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
        public ButtonControlsSimple GestureControlsSimple;
        public ButtonControlsSimple ButtonControlsSimple;
        public ButtonControlsSimple TouchControlsSimple;
        private ControlsManager controlsManager;

        public static float gravity = 20.0f;
        public static float jumpHeight = 2.5f;
        public static float strafeSpeed = 2.75f;
        public static float movementSpeed = 4f;

        Rigidbody r;
        bool grounded = false;
        Vector3 defaultScale;
        bool crouching = false;

        public void Awake()
        {
            this.GestureControlsSimple = GameObject.FindObjectsOfType<ButtonControlsSimple>(true).First(o => o.name == "GestureControlsSimple");
            this.ButtonControlsSimple = GameObject.FindObjectsOfType<ButtonControlsSimple>(true).First(o => o.name == "ButtonControlsSimple");
            this.TouchControlsSimple = GameObject.FindObjectsOfType<ButtonControlsSimple>(true).First(o => o.name == "TouchControlsSimple");
            controlsManager = new ControlsManager();
        }

        // Start is called before the first frame update
        public void Start()
        {
            r = GetComponent<Rigidbody>();
            r.constraints = RigidbodyConstraints.FreezePositionZ;
            r.freezeRotation = true;
            r.useGravity = false;
            defaultScale = transform.localScale;

            // todo lol
            string touchControlsDropdown = PlayerPrefs.GetString("TouchControlsDropdown");
            Debug.Log($"[IRPlayer][Update]TouchControlsDropdown: {touchControlsDropdown}");

            this.GestureControlsSimple.gameObject.SetActive(false);
            this.ButtonControlsSimple.gameObject.SetActive(false);
            this.TouchControlsSimple.gameObject.SetActive(false);
            if (touchControlsDropdown == "1")
            {
                this.GestureControlsSimple.gameObject.SetActive(true);
            }
            else if (touchControlsDropdown == "2")
            {
                this.ButtonControlsSimple.gameObject.SetActive(true);
            }
            else if (touchControlsDropdown == "3")
            {
                this.TouchControlsSimple.gameObject.SetActive(true);
            }
        }

        public void Update()
        {
            #region control checks
            ControlsManager.Controls activeControls = ControlsManager.Controls.Idle;

            activeControls = this.controlsManager.InputCheck(this.GestureControlsSimple, activeControls);
            activeControls = this.controlsManager.InputCheck(this.ButtonControlsSimple, activeControls);
            activeControls = this.controlsManager.InputCheck(this.TouchControlsSimple, activeControls);

            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("[IRPlayer][Update][Controls.Up]");
                activeControls |= ControlsManager.Controls.Up;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Debug.Log("[IRPlayer][Update][Controls.Down]");
                activeControls |= ControlsManager.Controls.Down;
            }
            if (Input.GetKey(KeyCode.A))
            {
                Debug.Log("[IRPlayer][Update][Controls.Left]");
                activeControls |= ControlsManager.Controls.Left;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Debug.Log("[IRPlayer][Update][Controls.Right]");
                activeControls |= ControlsManager.Controls.Right;
            }
            #endregion

            // Debug.Log($"activeControls: {activeControls.HasFlag(Controls.Left)}");

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
