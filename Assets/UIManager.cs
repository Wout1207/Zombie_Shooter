using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text textCurrentAmmoCount;
    public TMP_Text textTotalAmmoCount;

    public TargetShooter TargetShooter;
    private int currentAmmo;
    private int totalAmmo;
    private int prevTotalAmmoCount;

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
            textCurrentAmmoCount.text = "currentAmmo: " + TargetShooter.currentAmmoCount.ToString();
        }
        
        //if (totalAmmo != prevTotalAmmoCount)
        //{
            //prevTotalAmmoCount = totalAmmo;
            textTotalAmmoCount.text = "totalAmmo: " + TargetShooter.tottalAmmoCount.ToString();
        //}
    }

    void UpdateAmmoCount()
    {
        //currentAmmo = TargetShooter.currentAmmoCount;
        //totalAmmo = TargetShooter.tottalAmmoCount;

    }
}
