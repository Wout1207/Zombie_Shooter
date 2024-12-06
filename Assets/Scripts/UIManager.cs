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
        UpdateAmmoCount();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAmmo >= 0)
        {
            textCurrentAmmoCount.text = "Ammo mag: " + TargetShooter.currentAmmoCount.ToString();
        }
        
        //if (totalAmmo != prevTotalAmmoCount)
        //{
            //prevTotalAmmoCount = totalAmmo;
        textTotalAmmoCount.text = "Ammo: " + TargetShooter.totalAmmoCount.ToString();
        //}
        textHealth.text = "Health: " + player.currentHP.ToString();
        textRound.text = "Round: " + spawnManager.round.ToString();
        if (player.currentHP <= 0)
        {
            textGameOver.enabled = true;
        }
    }

    void UpdateAmmoCount()
    {
        //currentAmmo = TargetShooter.currentAmmoCount;
        //totalAmmo = TargetShooter.tottalAmmoCount;

    }
}
