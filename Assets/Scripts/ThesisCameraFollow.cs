using UnityEngine;

public class ThesisCameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 defaultOffset = new Vector3(0, 5, -8); // The "Normal" view
    public float smoothTime = 0.05f; 
    
    // Private variable that tracks where the camera WANTS to be right now
    private Vector3 activeOffset;
    private Vector3 currentVelocity;

    void Start()
    {
        activeOffset = defaultOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // We use 'activeOffset' which might change if we enter a tunnel
        Vector3 targetPosition = target.position + activeOffset;

        // Smoothly move there
        // Note: We use a separate dampener for the Offset itself to transition smoothly
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        transform.LookAt(target);
    }

    // FUNCTION: Call this to zoom in (Tunnel Mode)
    public void SetZoneOffset(Vector3 newOffset)
    {
        activeOffset = newOffset;
    }

    // FUNCTION: Call this to reset (Normal Mode)
    public void ResetOffset()
    {
        activeOffset = defaultOffset;
    }
}