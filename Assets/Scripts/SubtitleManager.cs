using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class SubtitleManager : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;
    public List<Subtitle> subtitles;
    public VideoPlayer videoPlayer;  // Replace AudioSource with VideoPlayer

    private void Start()
    {
        // Start the subtitle coroutine once the video starts playing
        StartCoroutine(ShowSubtitles());
    }

    private IEnumerator ShowSubtitles()
    {
        foreach (var subtitle in subtitles)
        {
            // Wait until the video reaches the start time of the subtitle
            yield return new WaitUntil(() => videoPlayer.time >= subtitle.startTime);

            // Display the subtitle text
            subtitleText.text = subtitle.text;

            // Wait until the video reaches the end time of the subtitle
            yield return new WaitUntil(() => videoPlayer.time >= subtitle.endTime);

            // Clear the subtitle text
            subtitleText.text = "";
        }
    }
}

[System.Serializable]
public class Subtitle
{
    public float startTime;  // When the subtitle should appear (in seconds)
    public float endTime;    // When the subtitle should disappear (in seconds)
    public string text;      // The subtitle text
}
