using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [Header("Zone Settings")]
    // Where should the camera go when inside this zone?
    // Example for Tunnel: (0, 2, -4) -> Lower and Closer
    public Vector3 zoneOffset = new Vector3(0, 2, -4);

    void OnTriggerEnter(Collider other)
    {
        // 1. Check if the PLAYER hit this box
        if (other.CompareTag("Player"))
        {
            // 2. Find the camera and tell it to switch views
            ThesisCameraFollow cam = FindFirstObjectByType<ThesisCameraFollow>();
            if (cam != null)
            {
                cam.SetZoneOffset(zoneOffset);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 1. When player leaves, go back to normal
        if (other.CompareTag("Player"))
        {
            ThesisCameraFollow cam = FindFirstObjectByType<ThesisCameraFollow>();
            if (cam != null)
            {
                cam.ResetOffset();
            }
        }
    }
}