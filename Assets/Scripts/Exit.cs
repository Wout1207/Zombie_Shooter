using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public bool exit = false;
    private float time;

    private void Start()
    {
        time = Time.time;
    }
    private void Update()
    {
        if (exit)
        {

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        exit = true;
    }
}
