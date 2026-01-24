using UnityEngine;

public class ThesisCameraFollow : MonoBehaviour
{
    public Transform target; // The Car
    public Vector3 offset = new Vector3(0, 5, -10); // Standard racing view
    public float smoothSpeed = 0.125f; // Small delay for "weight" feel

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate where the camera SHOULD be
        // We only follow the car's Z (forward) and X (steering), 
        // but we keep our own steady Y (height) logic or just follow completely.
        Vector3 desiredPosition = target.position + offset;

        // 2. Smoothly slide to that position (Lerp)
        // This prevents the camera from feeling "robotic"
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // 3. Apply position
        // We lock X to a tighter range if you don't want the camera swinging too wildy,
        // But for now, simple following is best.
        transform.position = smoothedPosition;

        // 4. Look at the car (Optional)
        // Helps keep the car centered even if the camera lags slightly
        transform.LookAt(target);
    }
}