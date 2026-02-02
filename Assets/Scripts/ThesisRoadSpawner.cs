using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Road Prefabs")]
    public GameObject[] roadPrefabs; // 0 = Straight
    public Transform playerCar;

    [Header("Generation Settings")]
    public int initialTiles = 5;
    public float spawnDistance = 150f;
    public float destroyDistance = 50f;

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public float statsInterval = 30f;

    // State
    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform previousExitPoint;

    // Shuffle bag (fair randomness)
    private List<int> shuffleBag = new List<int>();

    // Stats
    private Dictionary<int, int> spawnCounts = new Dictionary<int, int>();
    private float statsTimer;

    void Start()
    {
        InitializeStats();
        RefillShuffleBag();

        SpawnTile(0, true); // first straight
        for (int i = 0; i < initialTiles; i++)
            SpawnNextTile();
    }

    void Update()
    {
        if (playerCar == null || previousExitPoint == null)
            return;

        if (Vector3.Distance(playerCar.position, previousExitPoint.position) < spawnDistance)
            SpawnNextTile();

        DestroyOldTiles();
        UpdateStatsTimer();
    }

    // ---------------- SPAWN ----------------

    void SpawnNextTile()
    {
        if (shuffleBag.Count == 0)
            RefillShuffleBag();

        int index = shuffleBag[0];
        shuffleBag.RemoveAt(0);

        GameObject tile = Instantiate(roadPrefabs[index]);
        AlignTileUsingEntryExit(tile);
        Physics.SyncTransforms();

        FinalizeTile(tile, index);
    }

    void FinalizeTile(GameObject tile, int index)
    {
        tile.transform.SetParent(transform);
        activeTiles.Add(tile);

        previousExitPoint = GetChildRecursive(tile.transform, "ExitPoint");
        spawnCounts[index]++;

        if (enableDebugLogs)
            Debug.Log($"[SPAWNED] {roadPrefabs[index].name} | Count: {spawnCounts[index]}");
    }

    // ---------------- ALIGNMENT ----------------

    void AlignTileUsingEntryExit(GameObject tile)
    {
        Transform entry = GetChildRecursive(tile.transform, "EntryPoint");
        if (entry == null || previousExitPoint == null) return;

        Quaternion rotDiff = Quaternion.Inverse(entry.localRotation);
        tile.transform.rotation = previousExitPoint.rotation * rotDiff;

        Vector3 offset = entry.position - tile.transform.position;
        tile.transform.position = previousExitPoint.position - offset;
    }

    // ---------------- DESTROY ----------------

    void DestroyOldTiles()
    {
        if (activeTiles.Count == 0) return;

        GameObject oldest = activeTiles[0];
        Transform exit = GetChildRecursive(oldest.transform, "ExitPoint");

        if (exit != null && playerCar.position.z > exit.position.z + destroyDistance)
        {
            activeTiles.RemoveAt(0);
            Destroy(oldest);
        }
    }

    // ---------------- SHUFFLE BAG ----------------

    void RefillShuffleBag()
    {
        shuffleBag.Clear();
        for (int i = 0; i < roadPrefabs.Length; i++)
            shuffleBag.Add(i);

        Shuffle(shuffleBag);

        if (enableDebugLogs)
            Debug.Log("[BAG] Refilled & shuffled");
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    // ---------------- STATS ----------------

    void InitializeStats()
    {
        for (int i = 0; i < roadPrefabs.Length; i++)
            spawnCounts[i] = 0;
    }

    void UpdateStatsTimer()
    {
        statsTimer += Time.deltaTime;
        if (statsTimer >= statsInterval)
        {
            statsTimer = 0f;
            PrintStats();
        }
    }

    void PrintStats()
    {
        Debug.Log("===== ROAD SPAWN STATS =====");
        for (int i = 0; i < roadPrefabs.Length; i++)
            Debug.Log($"{roadPrefabs[i].name} → {spawnCounts[i]}");
    }

    // ---------------- UTIL ----------------

    void SpawnTile(int index, bool isFirst)
    {
        GameObject tile = Instantiate(roadPrefabs[index]);

        if (isFirst)
        {
            tile.transform.position = Vector3.zero;
            tile.transform.rotation = Quaternion.identity;
        }

        Physics.SyncTransforms();
        FinalizeTile(tile, index);
    }

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