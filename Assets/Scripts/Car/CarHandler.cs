using UnityEngine;

public class CarHandler : MonoBehaviour
{
    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    Transform gameModel;

    //Max Values
    float maxSteerVelocity = 2;
    float maxForwardVelocity = 30;

    [Header("Settings")] // Added header for tidiness
    public float accelerationMultiplier = 3;
    public float breaksMultiplier = 15;
    public float steeringMultiplier = 5;

    Vector2 input = Vector2.zero;

    void Update()
    {
        //Rotate car model when "turning"
        gameModel.transform.rotation = Quaternion.Euler(0, rb.linearVelocity.x * 5, 0);
    }

    void FixedUpdate()
    {
        // Apply Acceleration
        if (input.y > 0)
        {
            Accelerate();
        }
        else
        {
            // Friction/Drag when not gas
            rb.linearDamping = 0.2f; 
        }

        // Apply Brakes
        if (input.y < 0)
            Brake();

        Steer();

        //Force car not to go backwards
        if (rb.linearVelocity.z <= 0)
        rb.linearVelocity = Vector3.zero;
    }

    void Accelerate()
    {
        rb.linearDamping = 0;

        //stay within speed limit
        if(rb.linearVelocity.z >= maxForwardVelocity)
        return;

        // ForceMode.Acceleration ignores mass, giving snappier control
        rb.AddForce(transform.forward * accelerationMultiplier * input.y, ForceMode.Acceleration);
    }

    void Brake()
    {
        if (rb.linearVelocity.z <= 0)
            return;

        rb.AddForce(transform.forward * breaksMultiplier * input.y, ForceMode.Acceleration);
    }

    void Steer()
    {
        if (Mathf.Abs(input.x) > 0)
        {
            //Move car Sideways
            float speedBaseSteerLimit = rb.linearVelocity.z / 5.0f;
            speedBaseSteerLimit = Mathf.Clamp01(speedBaseSteerLimit);

            // Using ForceMode.Acceleration for turning too
            rb.AddForce(transform.right * steeringMultiplier * input.x * speedBaseSteerLimit, ForceMode.Acceleration);

            //Normalize the X Velocity
            float normalizedX = rb.linearVelocity.x / maxSteerVelocity;

            //Ensure that we don't allow it to get bigger than 1 in magnitude.
            normalizedX = Mathf.Clamp(normalizedX, -1.0f, 1.0f);

            //Make sure we stay within the turn speed limit
            // FIXED: Changed '=' to '*' below so it multiplies instead of assigning
            rb.linearVelocity = new Vector3(normalizedX * maxSteerVelocity, 0, rb.linearVelocity.z);
        }

        else
        {
            //Auto Center Car
            // FIXED: Removed 'new' keyword before Vector3.Lerp
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, 0, rb.linearVelocity.z), Time.fixedDeltaTime * 3);
        }
    }

    public void SetInput(Vector2 inputVector)
    {
        // Normalize ensures holding Up+Right doesn't make you faster than just Up
        inputVector.Normalize();
        input = inputVector;
    }
}