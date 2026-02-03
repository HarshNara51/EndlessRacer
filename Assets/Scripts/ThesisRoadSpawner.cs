using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Road Prefabs")]
    public GameObject straightPrefab;
    public GameObject[] curvePrefabs;

    [Header("References")]
    public Transform playerCar;
    public Camera mainCamera;

    [Header("Generation Settings")]
    public int initialTiles = 5;
    public float spawnDistance = 150f;
    public float destroyDistance = 40f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    // Runtime
    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform previousExitPoint;
    private bool gameStarted = false;

    // Shuffle bag (curves only)
    private List<int> shuffleBag = new List<int>();

    // ===================== UNITY =====================

    void Start()
    {
        RefillShuffleBag();

        SpawnFirstTile();

        for (int i = 0; i < initialTiles; i++)
            SpawnNextTile();
    }

    void Update()
    {
        if (playerCar == null || previousExitPoint == null || mainCamera == null)
            return;

        // Prevent start-frame chaos
        if (!gameStarted)
        {
            if (Vector3.Distance(playerCar.position, Vector3.zero) > 3f)
                gameStarted = true;
            else
                return;
        }

        // Spawn ahead
        if (Vector3.Distance(playerCar.position, previousExitPoint.position) < spawnDistance)
            SpawnNextTile();

        DestroyOldTiles();
    }

    // ===================== SPAWN =====================

    void SpawnFirstTile()
    {
        GameObject tile = Instantiate(straightPrefab);
        tile.transform.position = Vector3.zero;
        tile.transform.rotation = Quaternion.identity;

        FinalizeTile(tile);
    }

    void SpawnNextTile()
    {
        if (shuffleBag.Count == 0)
            RefillShuffleBag();

        int index = shuffleBag[0];
        shuffleBag.RemoveAt(0);

        GameObject tile = Instantiate(curvePrefabs[index]);
        AlignTile(tile);
        FinalizeTile(tile);
    }

    void FinalizeTile(GameObject tile)
    {
        tile.transform.SetParent(transform);
        activeTiles.Add(tile);

        previousExitPoint = GetChildRecursive(tile.transform, "ExitPoint");

        if (enableDebugLogs)
            Debug.Log("[SPAWNED] " + tile.name);
    }

    // ===================== ALIGNMENT =====================

    void AlignTile(GameObject tile)
    {
        Transform entry = GetChildRecursive(tile.transform, "EntryPoint");
        if (entry == null || previousExitPoint == null)
            return;

        // Rotate so entry forward matches previous exit forward
        Quaternion rotation = Quaternion.FromToRotation(entry.forward, previousExitPoint.forward);
        tile.transform.rotation = rotation * tile.transform.rotation;

        // Move so entry position matches exit position
        Vector3 offset = previousExitPoint.position - entry.position;
        tile.transform.position += offset;
    }

    // ===================== DESTROY (HYBRID LOGIC) =====================

    void DestroyOldTiles()
    {
        if (activeTiles.Count == 0)
            return;

        GameObject oldest = activeTiles[0];
        Transform exit = GetChildRecursive(oldest.transform, "ExitPoint");
        if (exit == null)
            return;

        Vector3 toPlayer = playerCar.position - exit.position;

        bool playerPassedExit = Vector3.Dot(exit.forward, toPlayer) > 0f;
        bool farEnough = toPlayer.magnitude > destroyDistance;
        bool notVisible = !IsTileVisible(oldest);

        if (playerPassedExit && farEnough && notVisible)
        {
            activeTiles.RemoveAt(0);
            Destroy(oldest);

            if (enableDebugLogs)
                Debug.Log("[DESTROYED] " + oldest.name);
        }
    }

    // ===================== CAMERA VISIBILITY =====================

    bool IsTileVisible(GameObject tile)
    {
        Renderer[] renderers = tile.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        foreach (Renderer r in renderers)
        {
            if (GeometryUtility.TestPlanesAABB(planes, r.bounds))
                return true;
        }

        return false;
    }

    // ===================== SHUFFLE BAG =====================

    void RefillShuffleBag()
    {
        shuffleBag.Clear();
        for (int i = 0; i < curvePrefabs.Length; i++)
            shuffleBag.Add(i);

        for (int i = 0; i < shuffleBag.Count; i++)
        {
            int rnd = Random.Range(i, shuffleBag.Count);
            (shuffleBag[i], shuffleBag[rnd]) = (shuffleBag[rnd], shuffleBag[i]);
        }
    }

    // ===================== UTIL =====================

    Transform GetChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = GetChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
