using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject[] treePrefabs;  // Assign your tree prefabs here
    public int maxTrees = 10;          // Max trees per spawn
    public float minScale = 0.5f;      // Min tree scale
    public float maxScale = 1.5f;      // Max tree scale

    [Header("Terrain & Road Settings")]
    public float planeSize = 40f;      // Width and length of your plane (assumed square)
    public float roadHalfWidth = 5f;   // Half-width of the road (adjust to your road width)

    [Header("Layers")]
    public LayerMask groundLayerMask;  // Layer mask for the ground/plane

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
    {
        int spawned = 0;
        int maxAttempts = maxTrees * 10;  // Prevent infinite loops
        int attempts = 0;

        while (spawned < maxTrees && attempts < maxAttempts)
        {
            attempts++;

            // Random position on the plane
            float x = Random.Range(-planeSize / 2f, planeSize / 2f);
            float z = Random.Range(-planeSize / 2f, planeSize / 2f);

            // Skip positions that fall inside road boundaries
            if (Mathf.Abs(x) < roadHalfWidth)
                continue;

            Vector3 rayOrigin = new Vector3(x, 100f, z);
            RaycastHit hit;

            // Raycast down to find exact ground height
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 200f, groundLayerMask))
            {
                Vector3 spawnPos = hit.point;

                // Choose a random tree prefab
                GameObject treePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

                // Instantiate the tree
                GameObject treeInstance = Instantiate(treePrefab, spawnPos, Quaternion.identity, transform);

                // Randomize tree scale uniformly
                float scale = Random.Range(minScale, maxScale);
                treeInstance.transform.localScale = Vector3.one * scale;

                // Optional: Random rotation around Y axis for variety
                float rotY = Random.Range(0f, 360f);
                treeInstance.transform.Rotate(0f, rotY, 0f);

                spawned++;
            }
            // If raycast missed ground, discard and try again
        }
    }
}