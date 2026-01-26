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
    public float laneSpeed = 40f; 
    public float roadWidth = 5f;

    [Header("Visuals")]
    public Transform visualModel; 
    public float rideHeight = 1.0f; 
    public float swayAmount = 30f; 
    public float turnSpeed = 15f;
    
    public Transform[] wheels; 
    // INCREASED DEFAULT: Make this 50 or 100 in Inspector for fast spin!
    public float wheelSpinSpeed = 50f; 

    [Header("UI")]
    public TMP_Text speedometerText;

    public float currentForwardSpeed = 0f;
    private float currentYAngle = 0f;

    void Update()
    {
        HandleSpeed();
        HandleSteering();
        ApplyMovement();
        AnimateWheels(); 
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

        float targetAngle = horizontalInput * swayAmount;
        currentYAngle = Mathf.MoveTowards(currentYAngle, targetAngle, turnSpeed * Time.deltaTime * 10f);

        if (visualModel != null)
            visualModel.localRotation = Quaternion.Euler(0, currentYAngle, 0);

        float movementFactor = currentYAngle / swayAmount; 
        Vector3 moveVector = Vector3.right * movementFactor * laneSpeed * Time.deltaTime;
        transform.Translate(moveVector, Space.World);

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -roadWidth, roadWidth);
        transform.position = clampedPosition;
    }

    void ApplyMovement()
    {
        transform.Translate(Vector3.forward * currentForwardSpeed * Time.deltaTime, Space.World);
        transform.position = new Vector3(transform.position.x, rideHeight, transform.position.z);
    }

    void AnimateWheels()
    {
        if (wheels == null || wheels.Length == 0) return;

        // THE FIX: "Torque"
        // We add the Input (key press) to the speed.
        // If you press W (Input=1), we add 50 fake speed units to the spin.
        // This makes wheels spin IMMEDIATELY when you press the key.
        float verticalInput = Input.GetAxis("Vertical");
        float torqueBoost = verticalInput * 50f; 

        // Calculate total spin
        float rotationAmount = (currentForwardSpeed + torqueBoost) * wheelSpinSpeed * Time.deltaTime;

        foreach (Transform wheel in wheels)
        {
            if (wheel != null)
                wheel.Rotate(Vector3.right, rotationAmount);
        }
    }
}