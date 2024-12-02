using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{
    public List<AudioClip> audioClips = new List<AudioClip>();
    public AudioSource audioSource;
    private int lastSongIndex = -1;
    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            int index = Random.Range(0, audioClips.Count - 1);
            while (index == lastSongIndex)
            {
                index = Random.Range(0, audioClips.Count - 1);
            }
            audioSource.clip = audioClips[index];
            lastSongIndex = index;
            audioSource.Play();
        }
    }
}
