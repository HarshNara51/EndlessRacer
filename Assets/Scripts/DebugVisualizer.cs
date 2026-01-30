using UnityEngine;

public class DebugVisualizer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        // Draw Entry Point (RED)
        Transform entry = transform.Find("EntryPoint");
        if (entry != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(entry.position, 0.5f);
        }

        // Draw Exit Point (GREEN)
        Transform exit = transform.Find("ExitPoint");
        if (exit != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(exit.position, 0.5f);
            
            // Draw a line showing the direction
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(exit.position, exit.position + exit.forward * 2);
        }
    }
}