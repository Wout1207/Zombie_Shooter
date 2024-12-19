using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioPlayer : MonoBehaviour
{
    public AudioSource audioSource;           // AudioSource to play the voice line
    public AudioClip[] voiceLines;            // Array of random audio clips

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Plays a random voice line
    public void PlayVoiceLine()
    {
        if (audioSource != null && voiceLines.Length > 0)
        {
            int randomIndex = Random.Range(0, voiceLines.Length); // Select random clip
            audioSource.clip = voiceLines[randomIndex];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No audio clips assigned to the array!");
        }
    }
}
