using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Road Prefabs")]
    public GameObject[] roadPrefabs;   // 0 = Straight, 1 = Curve, etc.
    public Transform playerCar;

    [Header("Generation Settings")]
    public int initialTiles = 5;
    public float spawnDistance = 150f;
    public float destroyDistance = 120f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    // Runtime
    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform previousExitPoint;

    // Fair randomness
    private List<int> shuffleBag = new List<int>();

    void Start()
    {
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
    }

    // ===================== SPAWN =====================

    void SpawnNextTile()
    {
        if (shuffleBag.Count == 0)
            RefillShuffleBag();

        int index = shuffleBag[0];
        shuffleBag.RemoveAt(0);

        GameObject tile = Instantiate(roadPrefabs[index]);
        AlignTileUsingEntryExit(tile);
        Physics.SyncTransforms();

        FinalizeTile(tile);
    }

    void FinalizeTile(GameObject tile)
    {
        tile.transform.SetParent(transform);
        activeTiles.Add(tile);

        previousExitPoint = GetChildRecursive(tile.transform, "ExitPoint");

        if (enableDebugLogs)
            Debug.Log("[SPAWNED] Road tile: " + tile.name);
    }

    // ===================== ALIGNMENT =====================

    void AlignTileUsingEntryExit(GameObject tile)
    {
        Transform entry = GetChildRecursive(tile.transform, "EntryPoint");
        if (entry == null || previousExitPoint == null)
            return;

        Quaternion rotationDiff = Quaternion.Inverse(entry.localRotation);
        tile.transform.rotation = previousExitPoint.rotation * rotationDiff;

        Vector3 offset = entry.position - tile.transform.position;
        tile.transform.position = previousExitPoint.position - offset;
    }

    // ===================== DESTROY =====================

    void DestroyOldTiles()
    {
        if (activeTiles.Count == 0)
            return;

        GameObject oldest = activeTiles[0];
        Transform exit = GetChildRecursive(oldest.transform, "ExitPoint");
        if (exit == null)
            return;

        float dist = Vector3.Distance(playerCar.position, exit.position);

        if (dist > destroyDistance)
        {
            activeTiles.RemoveAt(0);
            Destroy(oldest);

            if (enableDebugLogs)
                Debug.Log("[DESTROYED] Road tile");
        }
    }

    // ===================== SHUFFLE BAG =====================

    void RefillShuffleBag()
    {
        shuffleBag.Clear();
        for (int i = 0; i < roadPrefabs.Length; i++)
            shuffleBag.Add(i);

        Shuffle(shuffleBag);
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    // ===================== UTIL =====================

    void SpawnTile(int index, bool isFirst)
    {
        GameObject tile = Instantiate(roadPrefabs[index]);

        if (isFirst)
        {
            tile.transform.position = Vector3.zero;
            tile.transform.rotation = Quaternion.identity;
        }

        Physics.SyncTransforms();
        FinalizeTile(tile);
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