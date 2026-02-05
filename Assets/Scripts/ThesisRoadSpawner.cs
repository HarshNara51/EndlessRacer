using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Sequential Road Pattern (ORDER MATTERS)")]
    public List<GameObject> roadSequence;

    [Header("References")]
    public Transform playerCar;
    public Camera mainCamera;

    [Header("Generation Settings")]
    public int initialTiles = 1;
    public float spawnDistance = 15f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    // Runtime
    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform previousExitPoint;
    private bool gameStarted = false;
    private int sequenceIndex = 0;

    // ===================== UNITY =====================

    void Awake()
    {
        Debug.Log($"[RoadSpawner][{Time.time:F2}s] AWAKE");
    }

    void Start()
    {
        if (roadSequence == null || roadSequence.Count == 0)
        {
            Debug.LogError("[RoadSpawner] Road sequence is EMPTY!");
            return;
        }

        Debug.Log($"[RoadSpawner] Sequence size = {roadSequence.Count}");

        SpawnFirstTile();

        for (int i = 0; i < initialTiles; i++)
            SpawnNextTile();
    }

    void Update()
    {
        if (playerCar == null || previousExitPoint == null)
            return;

        if (!gameStarted)
        {
            if (Vector3.Distance(playerCar.position, Vector3.zero) > 3f)
            {
                gameStarted = true;
                Debug.Log($"[RoadSpawner][{Time.time:F2}s] Game started");
            }
            else
                return;
        }

        float dist = Vector3.Distance(playerCar.position, previousExitPoint.position);

        if (dist < spawnDistance)
        {
            SpawnNextTile();
        }
    }

    // ===================== SPAWN =====================

    void SpawnFirstTile()
    {
        GameObject tile = Instantiate(roadSequence[0]);
        tile.transform.position = Vector3.zero;
        tile.transform.rotation = Quaternion.identity;

        sequenceIndex = 1;
        FinalizeTile(tile);

        Debug.Log($"[SPAWN][{Time.time:F2}s] FIRST → {tile.name}");
    }

    void SpawnNextTile()
    {
        if (sequenceIndex >= roadSequence.Count)
            sequenceIndex = 0;

        GameObject tile = Instantiate(roadSequence[sequenceIndex]);
        sequenceIndex++;

        AlignTile(tile);
        FinalizeTile(tile);

        Debug.Log($"[SPAWN][{Time.time:F2}s] NEXT → {tile.name}");

        // 🔥 HARD CLEANUP: KEEP ONLY 2 TILES
        DestroyAllButLastTwo();
    }

    void FinalizeTile(GameObject tile)
    {
        tile.transform.SetParent(transform);
        activeTiles.Add(tile);
        previousExitPoint = GetChildRecursive(tile.transform, "ExitPoint");
    }

    // ===================== ALIGNMENT =====================

    void AlignTile(GameObject tile)
    {
        Transform entry = GetChildRecursive(tile.transform, "EntryPoint");
        if (entry == null || previousExitPoint == null)
            return;

        Quaternion rot = Quaternion.FromToRotation(entry.forward, previousExitPoint.forward);
        tile.transform.rotation = rot * tile.transform.rotation;

        Vector3 offset = previousExitPoint.position - entry.position;
        tile.transform.position += offset;
    }

    // ===================== HARD DESTROY =====================

    void DestroyAllButLastTwo()
    {
        while (activeTiles.Count > 2)
        {
            GameObject old = activeTiles[0];
            activeTiles.RemoveAt(0);
            Destroy(old);

            Debug.Log($"[FORCE DESTROY][{Time.time:F2}s] {old.name}");
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