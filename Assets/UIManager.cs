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


    public TargetShooter TargetShooter;
    private int currentAmmo;
    private int totalAmmo;
    private int prevTotalAmmoCount;
    public Player player;
    public SpawnManager spawnManager;

    // Start is called before the first frame update
    void Start()
    {
        GameEvents.current.onShotFired += UpdateAmmoCount;
        GameEvents.current.onGunJammed += ShowJamWarning;
        GameEvents.current.onGunDejammed += HideJamWarning;

        UpdateAmmoCount();
        textJamWarning.enabled = false; 
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
            textJamWarning.alpha = Mathf.PingPong(Time.time, 1.0f); 
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
}
