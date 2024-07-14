using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Rigidbody))]

public class IRPlayer : MonoBehaviour
{
    public TouchControlsSimple touchControlsSimple;
    // public Button[] TouchControlsSimple;

    [Flags]
    enum Controls {
        Idle,
        Left,
        Right,
        Up,
        Down
    }

    public float gravity = 20.0f;
    public float jumpHeight = 2.5f;
    public float strafeSpeed = 2.75f;

    Rigidbody r;
    bool grounded = false;
    Vector3 defaultScale;
    bool crouch = false;

    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<Rigidbody>();
        r.constraints = RigidbodyConstraints.FreezePositionZ;
        r.freezeRotation = true;
        r.useGravity = false;
        defaultScale = transform.localScale;

        // todo if
        this.touchControlsSimple = GameObject.Find("TouchControlsSimple").GetComponent<TouchControlsSimple>();

    }

    void Update()
    {
        #region control checks
        Controls activeControls = Controls.Idle;

        if (touchControlsSimple.controls.Left)
        {
            Debug.Log(touchControlsSimple.controls.Left);

            activeControls |= Controls.Left;
        }
        else if (touchControlsSimple.controls.Right)
        {
            Debug.Log(touchControlsSimple.controls.Right);

            activeControls |= Controls.Right;
        }
        if (touchControlsSimple.controls.Up)
        {
            Debug.Log(touchControlsSimple.controls.Up);

            activeControls |= Controls.Up;
        }
        else if (touchControlsSimple.controls.Down)
        {
            Debug.Log(touchControlsSimple.controls.Down);

            activeControls |= Controls.Down;
        }

        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            if (touch.position.y > (Screen.width/3))
            {
                activeControls |= Controls.Up;
            } else if (touch.position.y < (Screen.width/3))
            {
                activeControls |= Controls.Down;
            }
            if (touch.position.x < (Screen.width/3))
            {
                activeControls |= Controls.Left;
            }
            else if (touch.position.x > (Screen.width/3))
            {
                activeControls |= Controls.Right;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("[IRPlayer][Update][Controls.Up]");
            activeControls |= Controls.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Debug.Log("[IRPlayer][Update][Controls.Down]");
            activeControls |= Controls.Down;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("[IRPlayer][Update][Controls.Left]");
            activeControls |= Controls.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("[IRPlayer][Update][Controls.Right]");
            activeControls |= Controls.Right;
        }
        #endregion

        // Debug.Log($"activeControls: {activeControls.HasFlag(Controls.Left)}");

        #region game controls
        if (grounded) {
            if (activeControls.HasFlag(Controls.Up))
            {
                Debug.Log("[IRPlayer][Update][Jump]");
                r.velocity = new Vector3(r.velocity.x, CalculateJumpVerticalSpeed(), r.velocity.z);
            }
            else if (activeControls.HasFlag(Controls.Down))
            {
                Debug.Log("[IRPlayer][Update][Crouch]");
                transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(defaultScale.x, defaultScale.y * 0.4f, defaultScale.z), Time.deltaTime * 7);
            }
            else if (activeControls.HasFlag(Controls.Left) && r.position.x > -strafeSpeed)
            {
                Debug.Log("[IRPlayer][Update][StrafeLeft]");
                r.position = new Vector3(r.position.x - (Time.deltaTime * strafeSpeed), r.position.y, r.position.z);
            }
            else if (activeControls.HasFlag(Controls.Right) && r.position.x < strafeSpeed)
            {
                Debug.Log("[IRPlayer][Update][StrafeRight]");
                r.position = new Vector3(r.position.x + (Time.deltaTime * strafeSpeed), r.position.y, r.position.z);
            }
            else
            {
                // Debug.Log("[IRPlayer][Update][UnCrouch]");
                transform.localScale = Vector3.Lerp(transform.localScale, defaultScale, Time.deltaTime * 7);
            }

        }

        #endregion
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // We apply gravity manually for more tuning control
        r.AddForce(new Vector3(0, -gravity * r.mass, 0));

        grounded = false;
    }

    void OnCollisionStay()
    {
        grounded = true;
    }

    float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Finish")
        {
            try {
                GroundGenerator.instance.gameOver = true;
            } catch {
                Debug.Log("Failed to stop game!");
                Debug.Log(GroundGenerator.instance);
                Application.Quit();
            }
        }
    }
}