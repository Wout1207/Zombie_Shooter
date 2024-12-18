using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CutSceneHandler : MonoBehaviour
{
    public VideoPlayer videoPlayer;           // Video Player component to play the video
    public AudioSource audioSource;           // AudioSource to play the voice line
    public AudioClip[] voiceLines;            // Array of random audio clips
    public Camera mainCamera;                 // Explicit reference to the main camera
    public float rotationAngle = 40f;         // Amount of rotation in degrees (relative rotation)
    public float tiltAmount = 10f;            // Amount to tilt the camera down (in degrees)
    public float rotationDuration = 15f;      // Duration for camera rotation (slower speed)
    public float delayBeforeTurn = 70f;       // Delay before camera starts turning (in seconds)
    public float delayAfterVoice = 1f;        // Delay after voice line ended before loading scene (in seconds)
    private bool hasPlayed = false;           // Ensures the end logic triggers only once
    public float fadeDuration = 2f;           // Duration of the fade
    public float startFOV = 10f;              // Initial small FOV for black screen effect
    public float normalFOV = 60f;             // Normal FOV
    public float zoomFOV = 40f;               // Zoom FOV for the end

    // Start is called before the first frame update
    void Start()
    {
        mainCamera.fieldOfView = startFOV;
        StartCoroutine(FadeIn());
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

    private IEnumerator FadeIn()
    {
        float timeElapsed = 0f;

        // Gradually increase the FOV from startFOV to endFOV
        while (timeElapsed < fadeDuration)
        {
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, normalFOV, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure FOV is exactly at the end value
        mainCamera.fieldOfView = normalFOV;
    }

    // Coroutine to delay the camera turn and play voice line
    IEnumerator DelayedTurnAndVoice()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delayBeforeTurn);

        // Start rotating the main camera, then play the voice line
        StartCoroutine(RotateCameraAndPlayVoice());
    }

    // Coroutine to rotate the camera smoothly, tilt it, and play the voice line
    IEnumerator RotateCameraAndPlayVoice()
    {
        // Play the voice line BEFORE rotation and zoom
        PlayVoiceLine();

        float time = 0;
        Quaternion startRotation = mainCamera.transform.rotation;

        // Calculate the target rotation relative to the current rotation (rotate around Y-axis)
        Quaternion endRotation = Quaternion.Euler(mainCamera.transform.eulerAngles + new Vector3(tiltAmount, rotationAngle, 0));

        // Smoothly rotate the camera, tilt it, and zoom
        while (time < rotationDuration)
        {
            float t = time / rotationDuration;
            t = t * t * (3f - 2f * t); // Quadratic ease-in-out

            // Rotate the camera smoothly
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            // Zoom the camera smoothly
            mainCamera.fieldOfView = Mathf.Lerp(normalFOV, zoomFOV, t);

            time += Time.deltaTime;
            yield return null;
        }

        // Final rotation and FOV after the transition is done
        mainCamera.transform.rotation = endRotation;
        mainCamera.fieldOfView = zoomFOV;
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

        // Start a coroutine to delay the scene load by 1 second
        StartCoroutine(DelayedSceneLoad());
    }

    IEnumerator DelayedSceneLoad()
    {
        // Wait for 1 second before loading the scene
        yield return new WaitForSeconds(delayAfterVoice);

        // Load the scene after the delay
        SceneManager.LoadScene("SampleScene");
    }
}
