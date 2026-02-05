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
    public float raycastLength = 5.0f;     // Default length (increase in Inspector if needed)

    [Header("Visuals")]
    public Transform[] wheels;          
    public float wheelSpinSpeed = 100f;

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
            
            transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime * direction);
        }
    }

    void ApplyPhysics()
    {
        RaycastHit hit;
        
        // RAYCAST START POINT:
        // We start 1.0 unit ABOVE the car's pivot.
        // If your car is huge, the pivot might be underground. 
        // If you see the Red Line starting underground, increase this 1.0f to 2.0f or 3.0f!
        Vector3 rayOrigin = transform.position + (Vector3.up * 1.0f); 

        // --- VISUAL DEBUGGER (The Fix) ---
        // This draws a RED line in the Scene View.
        // If you don't see this line, the script is broken or Gizmos are off.
        Debug.DrawRay(rayOrigin, Vector3.down * raycastLength, Color.red);
        // ---------------------------------

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastLength))
        {
            // FOUND ROAD
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + rideHeightOffset; 
            
            // Snap to road smoothly
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, stickToRoadForce * Time.deltaTime);
            verticalVelocity = 0; 
        }
        else
        {
            // NO ROAD FOUND (Falling)
            verticalVelocity -= gravity * Time.deltaTime;
            // Apply Gravity
            transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);
        }

        // Move Forward
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