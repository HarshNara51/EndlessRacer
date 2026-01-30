using UnityEngine;
using TMPro;

public class ThesisCarController : MonoBehaviour
{
    [Header("Engine Specs")]
    public float maxSpeed = 80f;        
    public float acceleration = 30f;    
    public float friction = 10f;        
    public float brakePower = 50f;      

    [Header("Handling")]
    public float turnSpeed = 100f;      
    public float gravity = 20f;         
    public float stickToRoadForce = 10f; 
    
    [Header("Suspension")]
    public float rideHeightOffset = 0.5f; 
    public float raycastLength = 3.0f;    

    [Header("Visuals")]
    public Transform[] wheels;          
    public float wheelSpinSpeed = 100f;
    // Removed "bodyTiltAmount" and "visualModel" since we don't want tilt anymore

    [Header("UI")]
    public TMP_Text speedometerText;

    // Internal Variables
    private float currentSpeed = 0f;
    private float verticalVelocity = 0f; 

    void Update()
    {
        HandleEngine();
        HandleSteering();
        ApplyPhysics();
        AnimateVisuals();
        UpdateUI();
    }

    void HandleEngine()
    {
        float gasInput = Input.GetAxis("Vertical"); 

        if (gasInput > 0) currentSpeed += acceleration * gasInput * Time.deltaTime;
        else if (gasInput < 0) currentSpeed += brakePower * gasInput * Time.deltaTime;
        else
        {
            if (currentSpeed > 0) currentSpeed -= friction * Time.deltaTime;
            else if (currentSpeed < 0) currentSpeed += friction * Time.deltaTime;
            
            if(Mathf.Abs(currentSpeed) < 1f) currentSpeed = 0;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -30f, maxSpeed);
    }

    void HandleSteering()
    {
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnInput = Input.GetAxis("Horizontal"); 
            float direction = currentSpeed > 0 ? 1 : -1;
            
            // Just rotate the car normally. No "Visual Tilt" code here anymore.
            transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime * direction);
        }
    }

    void ApplyPhysics()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + (Vector3.up * 1.0f); 

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastLength))
        {
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + rideHeightOffset; 
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, stickToRoadForce * Time.deltaTime);
            verticalVelocity = 0; 
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);
        }

        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
    }

    void AnimateVisuals()
    {
        if (wheels != null)
        {
            float spin = currentSpeed * wheelSpinSpeed * Time.deltaTime;
            foreach (Transform wheel in wheels)
            {
                if(wheel != null) wheel.Rotate(Vector3.right, spin);
            }
        }
    }

    void UpdateUI()
    {
        if (speedometerText != null)
            speedometerText.text = Mathf.RoundToInt(currentSpeed).ToString() + " MPH";
    }
}