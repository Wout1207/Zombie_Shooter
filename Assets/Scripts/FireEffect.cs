using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireEffect : MonoBehaviour
{
    private Target parent;
    public float damage;
    public float timeBetweenDamage;
    public float duration;
    private float currentTime;
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject.GetComponent<Target>();
        currentTime = Time.time;
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (parent)
        {
            if (Time.time-currentTime >= timeBetweenDamage)
            {
                currentTime = Time.time;
                if (parent.hp > 0)
                {
                    parent.fireHit(damage);
                }
            }
            if (Time.time-startTime >= duration)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
