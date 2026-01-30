using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] roadPrefabs; 
    public Transform playerCar;

    [Header("Generation Settings")]
    public int initialTiles = 10;
    public float spawnDistance = 200f;
    public float destroyDistance = 200f;

    // State Variables
    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform previousExitPoint; 

    void Start()
    {
        // 1. Spawn the first tile manually at (0,0,0)
        SpawnTile(0, true); 

        // 2. Spawn the rest
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnRandomTile();
        }
    }

    void Update()
    {
        if (playerCar == null || activeTiles.Count == 0 || previousExitPoint == null) return;

        // Check distance to generate more
        float distanceToEnd = Vector3.Distance(playerCar.position, previousExitPoint.position);
        if (distanceToEnd < spawnDistance)
        {
            SpawnRandomTile();
        }

        // Cleanup old tiles
        GameObject oldestTile = activeTiles[0];
        if (Vector3.Distance(playerCar.position, oldestTile.transform.position) > destroyDistance)
        {
            activeTiles.RemoveAt(0);
            Destroy(oldestTile);
        }
    }

    void SpawnRandomTile()
    {
        int randomIndex = Random.Range(0, roadPrefabs.Length);
        SpawnTile(randomIndex);
    }

    void SpawnTile(int prefabIndex, bool isFirstTile = false)
    {
        GameObject tilePrefab = roadPrefabs[prefabIndex];
        GameObject newTile;

        if (isFirstTile)
        {
            // First tile: Spawn at world zero
            newTile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            // --- NEW ALIGNMENT LOGIC ---
            
            // 1. Create the object (position/rotation doesn't matter yet)
            newTile = Instantiate(tilePrefab); 

            // 2. Find the Entry Point
            Transform myEntry = GetChildRecursive(newTile.transform, "EntryPoint");

            if (myEntry != null)
            {
                // STEP A: MATCH ROTATION
                // We want: myEntry.rotation == previousExitPoint.rotation
                // So we rotate the root object by the difference
                Quaternion rotationDifference = Quaternion.Inverse(myEntry.localRotation);
                newTile.transform.rotation = previousExitPoint.rotation * rotationDifference;

                // STEP B: MATCH POSITION
                // Now that rotation is correct, we calculate the offset to snap positions
                // We move the root so that myEntry.position lands exactly on previousExitPoint.position
                Vector3 offset = myEntry.position - newTile.transform.position;
                newTile.transform.position = previousExitPoint.position - offset;
            }
            else
            {
                Debug.LogWarning("Tile " + newTile.name + " is missing 'EntryPoint'. Alignment will fail.");
                // Fallback: Just snap to position if entry is missing
                newTile.transform.position = previousExitPoint.position;
                newTile.transform.rotation = previousExitPoint.rotation;
            }
        }

        newTile.transform.SetParent(transform);
        activeTiles.Add(newTile);

        // --- PREPARE FOR NEXT TILE ---
        Transform myExit = GetChildRecursive(newTile.transform, "ExitPoint");

        if (myExit != null)
        {
            previousExitPoint = myExit;
        }
        else
        {
            Debug.LogError("Tile " + newTile.name + " is missing 'ExitPoint'! Spawning stopped.");
            Debug.Break();
        }
    }

    Transform GetChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = GetChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}