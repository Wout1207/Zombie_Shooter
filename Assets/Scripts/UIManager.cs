using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class UIManager : MonoBehaviour
{
    public TMP_Text textCurrentAmmoCount;
    public TMP_Text textTotalAmmoCount;
    public TMP_Text textHealth;
    public TMP_Text textGameOver;
    public TMP_Text textRound;
    public TMP_Text textJamWarning;
    public TMP_Text textOutOfAmmo;
    public TMP_Text textScore;
    public TMP_Text textAddToScore;


    public TargetShooter TargetShooter;
    private int currentAmmo;
    private int totalAmmo;
    private int prevTotalAmmoCount;
    public Player player;
    public SpawnManager spawnManager;
    public Exit exit;

    public float displayDurationOutOfAmmo = 1.5f;
    public float blinkingSpeedFactor = 2f;

    private float addToScoreDelay;

    private int scoreToAdd;

    // Start is called before the first frame update
    void Start()
    {
        GameEvents.current.onShotFired += UpdateAmmoCount;
        GameEvents.current.onGunJammed += ShowJamWarning;
        GameEvents.current.onGunDejammed += HideJamWarning;
        GameEvents.current.onOutofAmmo += ShowOutOfAmmo;

        UpdateAmmoCount();
        textJamWarning.enabled = false; 
        textOutOfAmmo.enabled = false;
        textScore.text = "Score: " + Score.score.ToString();
        textAddToScore.gameObject.SetActive(false);

    }

    void Update()
    {
        if (currentAmmo >= 0)
        {
            textCurrentAmmoCount.text = "Ammo mag: " + TargetShooter.currentAmmoCount.ToString();
        }

        textTotalAmmoCount.text = "Ammo: " + TargetShooter.totalAmmoCount.ToString();
        textHealth.text = "Health: " + player.currentHP.ToString();
        textRound.text = "Round: " + spawnManager.round.ToString();
        //textScore.text = "Score: " + Score.score.ToString();

        if (player.currentHP <= 0)
        {
            textGameOver.gameObject.SetActive(true);
            textGameOver.text = "YOU DIED";
        }

        if (exit.exit)
        {
            textGameOver.gameObject.SetActive(true);
            textGameOver.text = "YOU HAVE ESCAPED";
        }

        if (textJamWarning.enabled)
        {
            //textJamWarning.alpha = Mathf.PingPong(Time.time, 0.5f); 
            float alpha = Mathf.Clamp01((Mathf.Sin(Time.time * Mathf.PI * blinkingSpeedFactor) + 1f) / 1.5f);
            textJamWarning.alpha = alpha;
        }
        if (Time.time - addToScoreDelay > 3)
        {
            if (textAddToScore.gameObject.activeSelf)
            {
                Score.score += scoreToAdd;
            }
            scoreToAdd = 0;
            textScore.text = "Score: " + Score.score.ToString();
            
            textAddToScore.gameObject.SetActive(false);
        }
    }
    public void updateScore(int newScoreToAdd)
    {
        if (textAddToScore.gameObject.activeSelf)
        {
            scoreToAdd = newScoreToAdd + scoreToAdd;
            textAddToScore.text = "+ " + scoreToAdd.ToString();
            addToScoreDelay = Time.time;
        }
        else
        {
            scoreToAdd = newScoreToAdd;
            textAddToScore.gameObject.SetActive(true);
            textAddToScore.text = "+ " + scoreToAdd.ToString();
            addToScoreDelay = Time.time;
        }
    }

    void UpdateAmmoCount()
    {
        //currentAmmo = TargetShooter.currentAmmoCount;
        //totalAmmo = TargetShooter.tottalAmmoCount;

    }

    void ShowJamWarning()
    {
        textJamWarning.text = "Gun Jammed! Shake to clear.";
        textJamWarning.enabled = true;
    }

    void HideJamWarning()
    {
        textJamWarning.enabled = false;
    }

    void ShowOutOfAmmo(string[] values)
    {
        textOutOfAmmo.text = values[0];
        //displayDurationOutOfAmmo = float.Parse(values[1]);
        float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float displayDurationOutOfAmmo);
        //textOutOfAmmo.enabled = true;
        StartCoroutine(ShowOutOfAmmoCoroutine());
    }
    private IEnumerator ShowOutOfAmmoCoroutine()
    {
        textOutOfAmmo.enabled = true; // Show the message
        yield return new WaitForSeconds(displayDurationOutOfAmmo); // Wait for the specified duration
        textOutOfAmmo.enabled = false; // Hide the message
    }
}
