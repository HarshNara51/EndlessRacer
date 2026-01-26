using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public Vector3 zoneOffset = new Vector3(0, 3, -5); // Close & Low

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("✅ ENTERED TUNNEL!"); // <--- Check your Console for this!
            ThesisCameraFollow cam = FindFirstObjectByType<ThesisCameraFollow>();
            if (cam != null) cam.SetZoneOffset(zoneOffset);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("🚪 EXITED TUNNEL!");
            ThesisCameraFollow cam = FindFirstObjectByType<ThesisCameraFollow>();
            if (cam != null) cam.ResetOffset();
        }
    }
}