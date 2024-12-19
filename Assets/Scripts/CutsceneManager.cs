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

    public void StartCutscene()
    {
        animator.gameObject.GetComponent<Target>().enabled = true;
        animator.gameObject.transform.GetChild(3).gameObject.SetActive(true);
        animator.gameObject.transform.GetChild(0).gameObject.SetActive(true);

        mainCamera.SetActive(false);
        cutsceneCamera.SetActive(true);

        canvas.SetActive(false);

        cutscene.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
        Time.timeScale = 0;
        cutscene.Play();
    }

    public void EndCutscene()
    {
        cutsceneCamera.SetActive(false);
        mainCamera.SetActive(true);

        canvas.SetActive(true);
        animator.SetBool("playAnimation", true);

        Time.timeScale = 1;

        cutSceneEndPlayer.PlayVoiceLine();
    }
}
