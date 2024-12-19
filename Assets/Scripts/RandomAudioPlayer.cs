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

    // Plays all voice lines (non-blocking using Coroutine)
    public void PlayAllVoiceLines()
    {
        if (audioSource != null && voiceLines.Length > 0)
        {
            StartCoroutine(PlayAllVoiceLinesCoroutine());
        }
    }

    private IEnumerator PlayAllVoiceLinesCoroutine()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        for (int i = 0; i < voiceLines.Length; i++)
        {
            audioSource.clip = voiceLines[i];
            audioSource.Play();
            // Wait until the clip finishes playing
            yield return new WaitWhile(() => audioSource.isPlaying);
        }
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
