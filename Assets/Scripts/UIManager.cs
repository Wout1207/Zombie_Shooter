using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text textCurrentAmmoCount;
    public TMP_Text textTotalAmmoCount;
    public TMP_Text textHealth;
    public TMP_Text textGameOver;
    public TMP_Text textRound;
    public TMP_Text textJamWarning;
    public TMP_Text textOutOfAmmo;


    public TargetShooter TargetShooter;
    private int currentAmmo;
    private int totalAmmo;
    private int prevTotalAmmoCount;
    public Player player;
    public SpawnManager spawnManager;

    public float displayDurationOutOfAmmo = 1.5f;
    public float blinkingSpeedFactor = 2f;

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

        if (player.currentHP <= 0)
        {
            textGameOver.enabled = true;
        }

        if (textJamWarning.enabled)
        {
            //textJamWarning.alpha = Mathf.PingPong(Time.time, 0.5f); 
            float alpha = Mathf.Clamp01((Mathf.Sin(Time.time * Mathf.PI * blinkingSpeedFactor) + 1f) / 1.5f);
            textJamWarning.alpha = alpha;
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
        displayDurationOutOfAmmo = float.Parse(values[1]);
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
