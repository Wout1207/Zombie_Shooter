using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public List<GameObject> zombies = new List<GameObject>();
    public List<int> amountToSpawn = new List<int> ();
    public int round = 0;
    public TargetShooter targetShooter;
    public float time;
    // Start is called before the first frame update

    public enum SpawningState
    {
        start,
        newRoundDelay,
        spawning,
        timeBetweenSpawns,
        waitForEndRound,
        newRound
    }
    public SpawningState state;
    void Start()
    {
        state = SpawningState.start;
        amountToSpawn = new List<int> { 0, 0, 0, 0 };
        time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case SpawningState.start:
                if(Time.time - time > 5)
                {
                    round++;
                    state = SpawningState.newRound;
                }
                break;
            case SpawningState.newRound:
                startNewRound();
                state = SpawningState.spawning;
                break;
            case SpawningState.spawning:
                spawnEnemy();
                int leftToSpawn = 0;
                foreach (int i in amountToSpawn)
                {
                    leftToSpawn += i;
                }
                if(leftToSpawn == 0)
                {
                    state = SpawningState.waitForEndRound;
                }
                else
                {
                    time = Time.time;
                    state = SpawningState.timeBetweenSpawns;
                }
                break;
            case SpawningState.timeBetweenSpawns:
                if (Time.time - time > 5)
                {
                    state = SpawningState.spawning;
                }
                break;
            case SpawningState.waitForEndRound:
                if (transform.childCount == 0)
                {
                    time = Time.time;
                    round++;
                    state = SpawningState.newRoundDelay;
                }
                break;
            case SpawningState.newRoundDelay:
                if (Time.time - time > 10)
                {
                    state = SpawningState.newRound;
                }
                break;
        }
    }

    public void startNewRound()
    {
        amountToSpawn[0] = round * 9;
        amountToSpawn[1] = round / 3;
        amountToSpawn[2] = (round / 2) * 4;
        amountToSpawn[3] = (round / 3) * 4;
    }

    public void spawnEnemy()
    {
        int randomIndex = Random.Range(0, zombies.Count);
        while (amountToSpawn[randomIndex] == 0)
        {
            randomIndex = Random.Range(0, zombies.Count);
        }
        float minRadius = 75f;
        float maxRadius = 90f;
        float angle = Random.Range(0f, Mathf.PI * 2);
        float radius = Random.Range(minRadius, maxRadius);
        Vector3 spawnPosition = targetShooter.transform.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        int floorIndex = GetActiveTerrainTexture(spawnPosition);
        while (floorIndex == 3 || floorIndex == 5)
        {
            angle = Random.Range(0f, Mathf.PI * 2);
            radius = Random.Range(minRadius, maxRadius);
            spawnPosition = targetShooter.transform.position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            floorIndex = GetActiveTerrainTexture(spawnPosition);
        }
        GameObject obj = Instantiate(zombies[randomIndex], transform);
        obj.transform.position = spawnPosition;
        amountToSpawn[randomIndex] --;
    }

    int GetActiveTerrainTexture(Vector3 position)
    {
        Terrain terrain = Terrain.activeTerrain;
        TerrainData terrainData = terrain.terrainData;

        Vector3 terrainPosition = position - terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        float xCoord = terrainPosition.x / terrainSize.x;
        float zCoord = terrainPosition.z / terrainSize.z;

        float[,,] splatmapData = terrainData.GetAlphamaps(
            (int)(xCoord * terrainData.alphamapWidth),
            (int)(zCoord * terrainData.alphamapHeight),
            1, 1
        );

        int maxIndex = 0;
        float maxWeight = 0f;

        for (int i = 0; i < terrainData.alphamapLayers; i++)
        {
            if (splatmapData[0, 0, i] > maxWeight)
            {
                maxWeight = splatmapData[0, 0, i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }
}
