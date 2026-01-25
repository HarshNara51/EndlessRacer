using UnityEngine;
using TMPro;

public class ThesisCarController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float maxSpeed = 100f;
    public float acceleration = 40f; 
    public float deceleration = 30f; 
    public float friction = 10f;      
    
    [Header("Steering Settings")]
    public float laneSpeed = 40f; // High speed, but controlled by rotation
    public float roadWidth = 5f;

    [Header("Visuals")]
    public Transform visualModel; 
    public float rideHeight = 1.0f; 
    
    // THE MAGIC SAUCE
    public float swayAmount = 30f; // Angle of turn
    public float turnSpeed = 15f;  // How fast the car rotates (Higher = Snappier)

    [Header("UI")]
    public TMP_Text speedometerText;

    public float currentForwardSpeed = 0f;
    
    // Private variables
    private float currentYAngle = 0f;

    void Update()
    {
        HandleSpeed();
        HandleSteering();
        ApplyMovement();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (speedometerText != null)
            speedometerText.text = Mathf.RoundToInt(currentForwardSpeed).ToString() + " MPH";
    }

    void HandleSpeed()
    {
        float verticalInput = Input.GetAxis("Vertical"); 

        if (verticalInput > 0)
            currentForwardSpeed += acceleration * verticalInput * Time.deltaTime;
        else if (verticalInput < 0)
            currentForwardSpeed += deceleration * verticalInput * Time.deltaTime;
        else
        {
            if (currentForwardSpeed > 0)
            {
                currentForwardSpeed -= friction * Time.deltaTime;
                if (currentForwardSpeed < 0) currentForwardSpeed = 0;
            }
            else if (currentForwardSpeed < 0)
            {
                currentForwardSpeed += friction * Time.deltaTime;
                if (currentForwardSpeed > 0) currentForwardSpeed = 0;
            }
        }
        currentForwardSpeed = Mathf.Clamp(currentForwardSpeed, -20f, maxSpeed);
    }

    void HandleSteering()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal"); 

        // 1. ROTATE FIRST
        // Calculate where we WANT to face
        float targetAngle = horizontalInput * swayAmount;

        // Smoothly rotate towards that angle
        // 'MoveTowards' is linear and predictable (better than Lerp for this)
        currentYAngle = Mathf.MoveTowards(currentYAngle, targetAngle, turnSpeed * Time.deltaTime * 10f);

        // Apply rotation to the visual model
        if (visualModel != null)
        {
            visualModel.localRotation = Quaternion.Euler(0, currentYAngle, 0);
        }

        // 2. MOVE BASED ON ROTATION
        // We calculate movement percent based on how much we are currently turned.
        // If angle is 0, movement is 0. If angle is Max, movement is Max.
        float movementFactor = currentYAngle / swayAmount; // Returns value between -1 and 1
        
        // Move sideways
        Vector3 moveVector = Vector3.right * movementFactor * laneSpeed * Time.deltaTime;
        transform.Translate(moveVector, Space.World);

        // Clamp to Road
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -roadWidth, roadWidth);
        transform.position = clampedPosition;
    }

    void ApplyMovement()
    {
        transform.Translate(Vector3.forward * currentForwardSpeed * Time.deltaTime, Space.World);
        transform.position = new Vector3(transform.position.x, rideHeight, transform.position.z);
    }
}