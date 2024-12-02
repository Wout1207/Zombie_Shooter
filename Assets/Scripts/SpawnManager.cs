using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private List<GameObject> spawnLocations = new List<GameObject>();
    public int round = 0;
    public int inactiveAreas;
    private float time;
    private bool waitNewRound;
    // Start is called before the first frame update
    void Start()
    {
        time = Time.time;
        int childAmount = transform.childCount;
        for (int i = 0; i < childAmount; i++)
        {
            spawnLocations.Add(transform.GetChild(i).gameObject);
            inactiveAreas++;
        }
        waitNewRound = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (waitNewRound && Time.time - time <= 5 && inactiveAreas == transform.childCount)
        {

        }
        else if (waitNewRound && Time.time - time > 5 && inactiveAreas == transform.childCount)
        {
            waitNewRound = false;
            startRound();
        }
        else if(inactiveAreas == transform.childCount)
        {
            waitNewRound = true;
            time = Time.time;
        }
    }

    public void startRound()
    {
        round++;
        List<int> spawnerIndex = new List<int>();
        if (round % 3 == 0)
        {
            for (int i = 0; i < round/3; i++)
            {
                spawnerIndex.Add(Random.Range(0, spawnLocations.Count));
            }
        }
        for (int i = 0; i<spawnLocations.Count; i++)
        {
            ObjectSpawner spawner = spawnLocations[i].GetComponent<ObjectSpawner>();
            if (spawner)
            {
                spawner.amountToSpawn[0] = round * 5 / transform.childCount;
                foreach(int index in spawnerIndex)
                {
                    if (i==index)
                    {
                        spawner.amountToSpawn[1]++;
                    }
                }
                spawner.amountToSpawn[2] = round / 3;
                if (round >= 5)
                {
                    spawner.amountToSpawn[3] = round - 4;
                }
                spawner.spawnObjects = true;
                inactiveAreas --;
            }
        }
    }

}
