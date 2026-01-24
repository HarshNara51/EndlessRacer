using UnityEngine;

public class ThesisCarController : MonoBehaviour
{
    [Header("Thesis Variables")]
    // We will change these via code later for Easy/Hard modes
    public float forwardSpeed = 20f; 
    public float laneSpeed = 10f; 
    
    [Header("Limits")]
    public float roadWidth = 5f; // Stops player from going off-road

    void Update()
    {
        MoveForward();
        HandleSteering();
    }

    void MoveForward()
    {
        // 1. Move Forward constantly (Endless Runner style)
        // Time.deltaTime ensures smooth movement on all computers
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    void HandleSteering()
    {
        // 2. Get Input (Left/Right arrows or A/D)
        float horizontalInput = Input.GetAxis("Horizontal");

        // 3. Move Left/Right
        Vector3 moveVector = Vector3.right * horizontalInput * laneSpeed * Time.deltaTime;
        transform.Translate(moveVector);

        // 4. Clamp Position (Keep car on the road)
        // We act directly on the position to strictly enforce boundaries
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -roadWidth, roadWidth);
        transform.position = clampedPosition;

        // 5. Visual Tilt (Optional Juice)
        // Tilts the car slightly when turning for visual feedback
        float tiltAngle = -horizontalInput * 10f;
        // We only rotate the visual model, not the movement direction
        // (Assumes the car model is a child of this object, or we rotate the mesh inside)
        // For now, we rotate the whole object slightly on Z axis
        Quaternion targetRotation = Quaternion.Euler(0, 0, tiltAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
}