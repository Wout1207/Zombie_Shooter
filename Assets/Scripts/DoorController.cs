using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private float hp = 30;
    public List<GameObject> text;
    public GameObject zombieArea;
    
    public void Hit(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            if (zombieArea)
            {
                zombieArea.SetActive(true);
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            foreach(GameObject obj in text)
            {
                obj.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            foreach (GameObject obj in text)
            {
                obj.SetActive(false);
            }
        }
    }
}
