using UnityEngine;

public class Gun : MonoBehaviour
{
    public int maxBullets = 30;
    public int currentBullets;

    public enum WeaponType { Shotgun, P90 }
    public WeaponType weaponType;

    void Start()
    {
        currentBullets = maxBullets;
        SendAmmoData();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Fire();
        }

        if (Input.GetKeyDown(KeyCode.R)) 
        {
            Reload();
        }
    }

    public void Reload()
    {
        currentBullets = maxBullets;
        Debug.Log("Gun reloaded.");
        SendAmmoData();
    }

    public void Fire()
    {
        if (currentBullets > 0)
        {
            currentBullets--;
            Debug.Log("Fired! Bullets left: " + currentBullets);
            SendAmmoData();
        }
        else
        {
            Debug.Log("Out of ammo! Reload.");
        }
    }

    public void SendAmmoData()
    {
        string message = $"A:{currentBullets}\n"; 
        SerialManager.Instance.SendDataToESP32(message);
    }
}
