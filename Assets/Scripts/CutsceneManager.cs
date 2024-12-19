using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public PlayableDirector cutscene;
    public GameObject mainCamera;
    public GameObject cutsceneCamera;
    public GameObject canvas;
    public Animator animator;
    public RandomAudioPlayer cutSceneEndPlayer;

    private bool isCutscenePlaying = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isCutscenePlaying)
        {
            StartCutscene();
        }
    }

    public void StartCutscene()
    {
        isCutscenePlaying = true;

        mainCamera.SetActive(false);
        cutsceneCamera.SetActive(true);

        canvas.SetActive(false);

        cutscene.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
        Time.timeScale = 0;
        cutscene.Play();
    }

    public void EndCutscene()
    {
        isCutscenePlaying = false;

        cutsceneCamera.SetActive(false);
        mainCamera.SetActive(true);

        canvas.SetActive(true);
        animator.SetBool("playAnimation", true);

        Time.timeScale = 1;
        cutSceneEndPlayer.PlayVoiceLine();
    }
}
