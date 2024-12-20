using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour
{
    public bool exit = false;
    private float time;
    private bool timerStarted;

    private void Start()
    {
        time = Time.time;
        timerStarted = false;
    }
    private void Update()
    {
        if (exit)
        {
            if(timerStarted)
            {
                if (Time.time - time > 5)
                {
                    SceneManager.LoadScene("GameOverScene");
                }
            }
            else
            {
                timerStarted = true;
                time = Time.time;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        exit = true;
    }
}
