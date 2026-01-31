using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] roadPrefabs; 
    public Transform playerCar;

    [Header("Generation Settings")]
    public int initialTiles = 5;       // Start with a small buffer
    public float spawnDistance = 150f; // Lowered to prevent massive knots
    public float destroyDistance = 150f;
    
    [Header("Overlap Protection")]
    public LayerMask roadLayer;        // Assign "Default" or a new "Road" layer
    public float overlapRadius = 5f;   // Size of the safety bubble check

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
            SpawnRandomTileWithSafety();
        }
    }

    void Update()
    {
        if (playerCar == null || activeTiles.Count == 0 || previousExitPoint == null) return;

        // Check distance to generate more
        float distanceToEnd = Vector3.Distance(playerCar.position, previousExitPoint.position);
        if (distanceToEnd < spawnDistance)
        {
            SpawnRandomTileWithSafety();
        }

        // Cleanup old tiles
        GameObject oldestTile = activeTiles[0];
        if (Vector3.Distance(playerCar.position, oldestTile.transform.position) > destroyDistance)
        {
            activeTiles.RemoveAt(0);
            Destroy(oldestTile);
        }
    }

    // Tries to spawn a valid tile. If it overlaps, it retries.
    void SpawnRandomTileWithSafety()
    {
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 5; // Don't freeze the game trying forever

        while (!validPositionFound && attempts < maxAttempts)
        {
            int randomIndex = Random.Range(0, roadPrefabs.Length);
            
            // PRE-CALCULATE: Where would this tile go?
            // We need to simulate the position without actually spawning perfectly yet.
            // This is complex, so for a Thesis level, we use a simpler approach:
            // "Spawn, Check, Delete if Bad".
            
            GameObject tempTile = Instantiate(roadPrefabs[randomIndex]);
            AlignTile(tempTile); // Put it in place

            // CHECK FOR OVERLAP
            // We check a box around the new tile's position
            // Note: This requires your roads to have Colliders!
            Collider[] hits = Physics.OverlapBox(tempTile.transform.position, Vector3.one * overlapRadius, tempTile.transform.rotation, roadLayer);
            
            // "hits" will always hit the tile itself (tempTile), so we check if hits > 1
            // OR we ignore the tempTile specifically.
            bool hitOtherRoad = false;
            foreach(var hit in hits)
            {
                if (hit.transform.root != tempTile.transform && hit.transform.root != previousExitPoint.root)
                {
                    // We hit a road that ISN'T ourself and ISN'T the one we just connected to
                    hitOtherRoad = true;
                    break;
                }
            }

            if (hitOtherRoad)
            {
                // Bad spot! Destroy and try again.
                Destroy(tempTile);
                attempts++;
            }
            else
            {
                // Good spot! Keep it.
                tempTile.transform.SetParent(transform);
                activeTiles.Add(tempTile);
                
                // Update the exit point
                Transform myExit = GetChildRecursive(tempTile.transform, "ExitPoint");
                if (myExit != null) previousExitPoint = myExit;
                
                validPositionFound = true;
            }
        }

        // FAILSAFE: If we failed 5 times, force a straight road (Prefab 0)
        // assuming Prefab[0] is your straight road.
        if (!validPositionFound)
        {
            GameObject safeTile = Instantiate(roadPrefabs[0]);
            AlignTile(safeTile);
            safeTile.transform.SetParent(transform);
            activeTiles.Add(safeTile);
            Transform myExit = GetChildRecursive(safeTile.transform, "ExitPoint");
            if (myExit != null) previousExitPoint = myExit;
        }
    }

    // Refactored Alignment Logic (Reuse code)
    void AlignTile(GameObject tile)
    {
        Transform myEntry = GetChildRecursive(tile.transform, "EntryPoint");

        if (myEntry != null)
        {
            Quaternion rotationDifference = Quaternion.Inverse(myEntry.localRotation);
            tile.transform.rotation = previousExitPoint.rotation * rotationDifference;

            Vector3 offset = myEntry.position - tile.transform.position;
            tile.transform.position = previousExitPoint.position - offset;
        }
    }

    // Helper for manual first spawn
    void SpawnTile(int index, bool isFirst = false)
    {
        GameObject t = Instantiate(roadPrefabs[index]);
        if(isFirst) 
        {
            t.transform.position = Vector3.zero;
            t.transform.rotation = Quaternion.identity;
        }
        else 
        { 
            AlignTile(t); 
        }
        
        t.transform.SetParent(transform);
        activeTiles.Add(t);
        Transform exit = GetChildRecursive(t.transform, "ExitPoint");
        if (exit != null) previousExitPoint = exit;
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