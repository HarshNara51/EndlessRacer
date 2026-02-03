using UnityEngine;

public class RoadEnvironmentSpawner : MonoBehaviour
{
    [Header("Tree Prefabs")]
    public GameObject[] treePrefabs;
    
    [Header("Spawn Settings")]
    public int treeCount = 15;
    public float spawnAreaWidth = 50f;   // How wide left/right from road center
    public float spawnAreaLength = 50f;  // How far forward/back from road center
    
    [Header("Road Avoidance")]
    public float roadAvoidanceDistance = 8f;  // How far from road center to stay away
    
    [Header("Tree Size")]
    public float minScale = 0.5f;
    public float maxScale = 1.5f;
    
    [Header("Layers")]
    public LayerMask groundLayer;  // The GroundPlane layer
    public LayerMask roadLayer;    // The Road layer
    
    [Header("Debug")]
    public bool showDebugGizmos = true;

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogWarning("No tree prefabs assigned!");
            return;
        }

        int spawned = 0;
        int maxAttempts = treeCount * 50;  // Safety limit
        int attempts = 0;

        while (spawned < treeCount && attempts < maxAttempts)
        {
            attempts++;

            // 1. Generate random position in local space
            float x = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
            float z = Random.Range(-spawnAreaLength / 2f, spawnAreaLength / 2f);
            
            Vector3 localPos = new Vector3(x, 0, z);
            
            // 2. Convert to world space
            Vector3 worldPos = transform.TransformPoint(localPos);
            
            // 3. CHECK: Is this too close to the road center (Z-axis)?
            // Since roads run along Z, we check the X distance from center
            if (Mathf.Abs(x) < roadAvoidanceDistance)
            {
                continue;  // Too close to road center, skip
            }
            
            // 4. Raycast DOWN to find the ground
            Vector3 rayStart = worldPos + Vector3.up * 100f;
            
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 200f, groundLayer))
            {
                // 5. Double-check: No road nearby using sphere check
                if (Physics.CheckSphere(hit.point, 3f, roadLayer))
                {
                    continue;  // Road detected nearby, skip
                }
                
                // 6. Safe to spawn!
                PlaceTree(hit.point);
                spawned++;
            }
        }

        if (showDebugGizmos)
            Debug.Log($"[RoadEnvironmentSpawner] Spawned {spawned} trees out of {treeCount} attempts: {attempts}");
    }

    void PlaceTree(Vector3 position)
    {
        // Pick random tree
        GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        
        // Random rotation
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        // Spawn as child of this road tile
        GameObject tree = Instantiate(treePrefab, position, rotation, transform);
        
        // Random scale
        float scale = Random.Range(minScale, maxScale);
        tree.transform.localScale = Vector3.one * scale;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Draw spawn area
        Gizmos.color = Color.green;
        Vector3 size = new Vector3(spawnAreaWidth, 5f, spawnAreaLength);
        Gizmos.DrawWireCube(transform.position, size);
        
        // Draw road avoidance zone (red)
        Gizmos.color = Color.red;
        Vector3 avoidSize = new Vector3(roadAvoidanceDistance * 2f, 5f, spawnAreaLength);
        Gizmos.DrawWireCube(transform.position, avoidSize);
    }
}