using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public List<GameObject> zombies;
    public Vector3 spawnAreaSize;
    public List<int> amountToSpawn = new List<int>();
    public bool spawnObjects;
    private float time;
    public SpawnManager manager;

    void Start()
    {
        for(int i = 0; i < zombies.Count; i++)
        {
            amountToSpawn.Add(0);
        }
        time = Time.time;
        spawnObjects = false;
    }
    private void Update()
    {
        int totalToSpawn = 0;
        foreach (int i in amountToSpawn)
        {
            totalToSpawn += i;
        }
        if(spawnObjects && totalToSpawn > 0 && Time.time-time > 7)
        {
            time = Time.time;
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2) + transform.position.x,
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2) + transform.position.y,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2) + transform.position.z
            );

            int randomIndex = Random.Range(0, zombies.Count);
            while (amountToSpawn[randomIndex] == 0)
            {
                randomIndex = Random.Range(0, zombies.Count);
            }
            GameObject obj = Instantiate(zombies[randomIndex], spawnPosition, Quaternion.identity);
            //obj.transform.localScale = new Vector3(3, 3, 3);
            //obj.AddComponent<Target>();
            obj.transform.SetParent(transform);
            amountToSpawn[randomIndex]--;
        }
        if (spawnObjects && totalToSpawn == 0 && transform.childCount == 0)
        {
            Debug.Log("area completed");
            spawnObjects = false;
            manager.inactiveAreas++;
        }
    }
}
