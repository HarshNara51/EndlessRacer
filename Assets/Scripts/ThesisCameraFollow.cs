using UnityEngine;

public class ThesisCameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 5, -8); // Slightly closer default
    
    // LOWER number = Stiffer, less drift (Fixes "Too far away")
    // 0.05 is very snappy. 0.2 is loose.
    public float smoothTime = 0.05f; 
    
    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculate ideal spot
        Vector3 targetPosition = target.position + offset;

        // 2. Override Z (Forward/Back) to prevent "Rubber Banding"
        // At high speeds, we snap the Z axis harder so the car doesn't run away.
        Vector3 finalPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        
        // OPTIONAL: If the camera is still too jittery at 100mph, uncomment the line below.
        // It forces the camera to lock EXACTLY to the car's forward speed, smoothing only the side-to-side.
        // finalPosition.z = target.position.z + offset.z;

        transform.position = finalPosition;

        // 3. Look at car (Optional - disable if it feels dizzy)
        transform.LookAt(target);
    }
}