using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] roadPrefabs; 
    public Transform playerCar;

    [Header("Generation Settings")]
    public int initialTiles = 5;       
    public float spawnDistance = 150f; 
    public float destroyDistance = 150f;
    
    [Header("Overlap Protection")]
    public LayerMask roadLayer;        
    public float overlapRadius = 5f;   

    // State Variables
    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform previousExitPoint; 

    void Start()
    {
        SpawnTile(0, true); // First tile manual
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnRandomTileWithSafety();
        }
    }

    void Update()
    {
        if (playerCar == null || activeTiles.Count == 0 || previousExitPoint == null) return;

        float distanceToEnd = Vector3.Distance(playerCar.position, previousExitPoint.position);
        if (distanceToEnd < spawnDistance)
        {
            SpawnRandomTileWithSafety();
        }

        GameObject oldestTile = activeTiles[0];
        if (Vector3.Distance(playerCar.position, oldestTile.transform.position) > destroyDistance)
        {
            activeTiles.RemoveAt(0);
            Destroy(oldestTile);
        }
    }

    void SpawnRandomTileWithSafety()
    {
        bool validPositionFound = false;
        int attempts = 0;
        int maxAttempts = 5; 

        while (!validPositionFound && attempts < maxAttempts)
        {
            int randomIndex = Random.Range(0, roadPrefabs.Length);
            
            // 1. Create Temp Tile
            GameObject tempTile = Instantiate(roadPrefabs[randomIndex]);
            AlignTile(tempTile); 

            // 2. Check Overlap
            Collider[] hits = Physics.OverlapBox(tempTile.transform.position, Vector3.one * overlapRadius, tempTile.transform.rotation, roadLayer);
            
            bool hitOtherRoad = false;
            foreach(var hit in hits)
            {
                if (hit.transform.root != tempTile.transform && hit.transform.root != previousExitPoint.root)
                {
                    hitOtherRoad = true;
                    break;
                }
            }

            if (hitOtherRoad)
            {
                Destroy(tempTile);
                attempts++;
            }
            else
            {
                // Success!
                tempTile.transform.SetParent(transform);
                activeTiles.Add(tempTile);
                Transform myExit = GetChildRecursive(tempTile.transform, "ExitPoint");
                if (myExit != null) previousExitPoint = myExit;
                validPositionFound = true;
            }
        }

        // Failsafe: If all random tries failed, force a Straight Road (Index 0)
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