using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("What to Spawn")]
    public GameObject[] obstaclePrefabs; // Rocks, Barriers, Crates

    [Header("Where to Spawn")]
    // Drag your empty "Marker" objects here in the Inspector
    public Transform[] spawnPoints; 

    [Header("Settings")]
    [Range(0f, 1f)]
    public float spawnChance = 0.3f; // 30% chance per point to spawn something

    void Start()
    {
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        if (obstaclePrefabs.Length == 0 || spawnPoints.Length == 0) return;

        foreach (Transform point in spawnPoints)
        {
            // Roll the dice (e.g., 30% chance)
            if (Random.value < spawnChance)
            {
                // Pick a random obstacle
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

                // Spawn it as a CHILD of the spawn point (keeps it tidy)
                Instantiate(prefab, point.position, point.rotation, point);
            }
        }
    }
}