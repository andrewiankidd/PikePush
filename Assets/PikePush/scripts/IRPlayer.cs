using UnityEngine;
[RequireComponent(typeof(Rigidbody))]

public class IRPlayer : MonoBehaviour
{
    public float gravity = 20.0f;
    public float jumpHeight = 2.5f;

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
    }

    void Update()
    {
        if (grounded) {

            // jump?
            if (Input.GetKeyDown(KeyCode.W))
            {
                r.velocity = new Vector3(r.velocity.x, CalculateJumpVerticalSpeed(), r.velocity.z);
            }

            // strafe
            if (Input.GetKey(KeyCode.A) && r.position.x > -2.75f)
            {
                r.position = new Vector3((r.position.x - (Time.deltaTime * 2.75f)), r.position.y, r.position.z);
            }
            else if (Input.GetKey(KeyCode.D) && r.position.x < 2.75f)
            {
                r.position = new Vector3((r.position.x + (Time.deltaTime * 2.75f)), r.position.y, r.position.z);
            }
        }
        // Jump
        

        //Crouch
        crouch = Input.GetKey(KeyCode.S);
        if (crouch)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(defaultScale.x, defaultScale.y * 0.4f, defaultScale.z), Time.deltaTime * 7);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, defaultScale, Time.deltaTime * 7);
        }
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
            GroundGenerator.instance.gameOver = true;
        }
    }
}