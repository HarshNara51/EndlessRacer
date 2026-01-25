using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] roadPrefabs;
    public Transform playerCar;

    [Header("Fine Tuning")]
    public float tileLength = 30f;   // Set this to your prefab's Z length
    public int tilesOnScreen = 10;   // How many tiles to generate ahead
    public float spawnZ = 0f;
    
    [Header("Cleanup Settings")]
    public float safeZone = 100f;    // Distance to keep road BEHIND the player

    // We use a List so we can easily add to the front and remove from the back
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        // 1. Backfill: Spawn a few tiles BEHIND the player first so you don't start on an edge
        // This ensures the player starts in the middle of a road, not at the cliff edge.
        spawnZ = playerCar.position.z - (tileLength * 3); 

        // 2. Spawn the initial batch
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile();
        }
    }

    void Update()
    {
        // LOGIC 1: SPAWN AHEAD
        // If the end of the road is getting too close to the player...
        // (We want to maintain 'tilesOnScreen' amount of road ahead)
        if (spawnZ - playerCar.position.z < (tilesOnScreen * tileLength))
        {
            SpawnTile();
        }

        // LOGIC 2: DELETE BEHIND
        // Only delete the tile if it is WAY behind the player
        if (activeTiles.Count > 0)
        {
            // Calculate distance from player to the oldest tile
            float distanceToOldest = playerCar.position.z - activeTiles[0].transform.position.z;

            // If the oldest tile is further back than our safeZone...
            if (distanceToOldest > safeZone)
            {
                DeleteOldTile();
            }
        }
    }

    void SpawnTile()
    {
        GameObject go = Instantiate(roadPrefabs[0]);
        go.transform.SetParent(transform);
        go.transform.position = Vector3.forward * spawnZ;
        activeTiles.Add(go);
        spawnZ += tileLength;
    }

    void DeleteOldTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}