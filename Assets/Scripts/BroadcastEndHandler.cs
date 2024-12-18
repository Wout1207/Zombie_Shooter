using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class BroadcastEndHandler : MonoBehaviour
{
    public VideoPlayer videoPlayer;           // Video Player component to play the video
    public AudioSource audioSource;           // AudioSource to play the voice line
    public AudioClip[] voiceLines;            // Array of random audio clips
    public Camera mainCamera;                 // Explicit reference to the main camera
    public float rotationAngle = 40f;         // Amount of rotation in degrees (relative rotation)
    public float rotationDuration = 15f;      // Duration for camera rotation (slower speed)
    public float delayBeforeTurn = 70f;       // Delay before camera starts turning (in seconds)
    private bool hasPlayed = false;           // Ensures the end logic triggers only once

    // Start is called before the first frame update
    void Start()
    {
        // Start the delay timer as soon as the video starts playing
        videoPlayer.Play();  // Ensure the video starts playing
        StartCoroutine(DelayedTurnAndVoice());
    }

    void Update()
    {
        // Check if the audio has finished playing
        if (!audioSource.isPlaying && hasPlayed)
        {
            hasPlayed = false; // Reset flag
            OnAudioEnd();
        }

        // Check if audio is playing to set the flag
        if (audioSource.isPlaying && !hasPlayed)
        {
            hasPlayed = true;
        }
    }

    // Coroutine to delay the camera turn and play voice line
    IEnumerator DelayedTurnAndVoice()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delayBeforeTurn);

        // Start rotating the main camera, then play the voice line
        StartCoroutine(RotateCameraAndPlayVoice());
    }

    // Coroutine to rotate the camera smoothly, then play the voice line
    IEnumerator RotateCameraAndPlayVoice()
    {
        float time = 0;
        Quaternion startRotation = mainCamera.transform.rotation;

        // Calculate the target rotation relative to the current rotation (rotate around Y-axis)
        Quaternion endRotation = Quaternion.Euler(mainCamera.transform.eulerAngles + new Vector3(0, rotationAngle, 0));

        // Smoothly rotate the camera using an ease-in-out curve (quadratic ease-out)
        while (time < rotationDuration)
        {
            float t = time / rotationDuration;
            t = t * t * (3f - 2f * t); // Quadratic ease-in-out

            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            time += Time.deltaTime;
            yield return null;
        }

        // Final rotation after the rotation is done
        mainCamera.transform.rotation = endRotation;

        // Play the voice line AFTER rotation
        PlayVoiceLine();
    }

    // Plays a random voice line
    void PlayVoiceLine()
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

    // This method will be called when the audio ends
    void OnAudioEnd()
    {
        Debug.Log("Audio has finished playing!");
        // Add your custom logic here
        SceneManager.LoadScene("SampleScene");
    }
}
