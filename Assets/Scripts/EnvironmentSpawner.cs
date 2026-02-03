using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject[] treePrefabs;
    public int maxTrees = 10;

    [Tooltip("Final scale applied to trees (prefab scale should be 1,1,1)")]
    public float minScale = 0.25f;
    public float maxScale = 0.45f;

    [Header("Ground Settings")]
    public float planeSize = 40f;

    [Tooltip("Half width of the road + safety buffer")]
    public float roadHalfWidth = 7f;

    [Header("Layers")]
    public LayerMask groundLayerMask;

    void Start()
    {
        SpawnTrees();
    }

    void SpawnTrees()
{
    if (treePrefabs == null || treePrefabs.Length == 0)
    {
        Debug.LogError("No tree prefabs assigned!", this);
        return;
    }

    int spawned = 0;
    int attempts = 0;
    int maxAttempts = maxTrees * 10;

    while (spawned < maxTrees && attempts < maxAttempts)
    {
        attempts++;

        float localX = Random.Range(-planeSize / 2f, planeSize / 2f);
        float localZ = Random.Range(-planeSize / 2f, planeSize / 2f);

        Vector3 rayOrigin = transform.TransformPoint(
            new Vector3(localX, 50f, localZ)
        );

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f))
        {
            // ❌ If we hit ROAD, skip
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Road"))
                continue;

            // ❌ If we didn’t hit ground, skip
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                continue;

            GameObject prefab =
                treePrefabs[Random.Range(0, treePrefabs.Length)];

            GameObject tree = Instantiate(
                prefab,
                hit.point,
                Quaternion.identity,
                transform
            );

            float scale = Random.Range(minScale, maxScale);
            tree.transform.localScale *= scale;
            tree.transform.Rotate(0f, Random.Range(0f, 360f), 0f);

            spawned++;
        }
    }
}
}