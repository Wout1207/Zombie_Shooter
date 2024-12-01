using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingSoundScript : MonoBehaviour
{
    public List<GameObject> floorList = new List<GameObject>();
    public bool isWalking;
    public AudioSource audioSource;
    public AudioClip grass;
    public AudioClip sand;
    public AudioClip concrete;
    public AudioClip dirt;

    private void OnTriggerEnter(Collider other)
    {
        string name = other.name;
        if (name.Contains("Ground_Tile") || name.Contains("Concrete_Block") || name.Contains("Mountain") || name.Contains("Road") || name.Contains("Dirt_Rows"))
        {
            floorList.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (floorList.Contains(other.gameObject))
        {
            floorList.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        if (isWalking)
        {
            PlayFootstepSound();
        }
        else
        {
            audioSource.Stop();
        }
    }

    private void PlayFootstepSound()
    {
        if (floorList.Count > 0)
        {
            // Determine the floor type
            GameObject currentFloor = floorList[0]; // Take the last floor entered
            string name = currentFloor.name;

            // Map floor types to sounds
            if (name.Contains("Grass"))
            {
                audioSource.clip = grass;
            }
            else if (name.Contains("Sand"))
            {
                audioSource.clip = sand;
            }
            else if (name.Contains("Concrete"))
            {
                audioSource.clip = concrete;
            }
            else if (name.Contains("Dirt"))
            {
                audioSource.clip = dirt;
            }

            // Play sound if it's not already playing
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
