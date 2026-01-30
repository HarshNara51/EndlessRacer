using UnityEngine;

public class ThesisCameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public Vector3 defaultOffset = new Vector3(0, 5, -8); 
    
    [Header("Game Feel Tuning")]
    public float followSpeed = 0.2f;    // The "Laziness"
    public float rotationSpeed = 5.0f;  
    public float maxDistance = 15f;     // NEW: The "Leash" length

    private Vector3 activeOffset;
    private Vector3 currentVelocity;

    void Start()
    {
        activeOffset = defaultOffset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. POSITION (Drift)
        Vector3 desiredPosition = target.TransformPoint(activeOffset);
        
        // Move nicely...
        Vector3 nextPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSpeed);

        // ...BUT check the leash!
        // If the calculated next spot is too far from the car, pull it closer.
        float distanceToCar = Vector3.Distance(nextPosition, target.position);
        
        if (distanceToCar > maxDistance)
        {
            // Calculate the direction from car to camera
            Vector3 directionFromCar = (nextPosition - target.position).normalized;
            // Force the position to be exactly at the max distance
            nextPosition = target.position + (directionFromCar * maxDistance);
        }

        transform.position = nextPosition;

        // 2. ROTATION (Look)
        Vector3 directionToCarVec = target.position - transform.position;
        if (directionToCarVec != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToCarVec);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetZoneOffset(Vector3 newOffset)
    {
        activeOffset = newOffset;
    }

    public void ResetOffset()
    {
        activeOffset = defaultOffset;
    }
}