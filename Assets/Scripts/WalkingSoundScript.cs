using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingSoundScript : MonoBehaviour
{
    public bool isWalking;
    public AudioSource audioSource;
    public AudioClip[] terrainSounds;

    private void Update()
    {
        if (isWalking)
        {
            int terrainIndex = GetActiveTerrainTexture(transform.position);
            if (terrainIndex >= 0 && terrainIndex < terrainSounds.Length)
            {
                audioSource.clip = terrainSounds[terrainIndex];
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
        }
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
