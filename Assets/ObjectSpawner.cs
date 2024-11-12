using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f); // Define the area size
    public Vector3 spawnOffset = new Vector3(-50, 0, 150);
    public int amountToSpawn = 10;

    void Start()
    {
        StartCoroutine(SpawnObjectCoroutine());
    }

    IEnumerator SpawnObjectCoroutine()
    {
        while (amountToSpawn>0)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2) + spawnOffset.x,
                Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2) + spawnOffset.y,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2) + spawnOffset.z
            );
            Debug.Log(spawnPosition);
            GameObject obj = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            obj.transform.localScale = new Vector3(3, 3, 3);
            obj.AddComponent<Target>();
            amountToSpawn--;
            yield return new WaitForSeconds(10f);
        }
    }
}
