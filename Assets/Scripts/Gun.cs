using System.Collections;  // Required for IEnumerator
using UnityEngine;
using UnityEngine.Networking;  // For HTTP requests
using UnityEngine.UI;  // For UI components like Text
using TMPro;

public class Gun : MonoBehaviour
{
    public int maxBullets = 30;
    public int currentBullets;
    public WeaponType weaponType;
    public TMP_Text ammoText;  // Reference to the UI Text for ammo

    public enum WeaponType
    {
        Shotgun,
        P90
    }

    private string esp32IP = "http://<ESP32_IP_ADDRESS>/update";  // Replace with your ESP32 IP address

    void Start()
    {
        currentBullets = maxBullets;  // Initialize current bullets
        UpdateAmmoText();  // Update the UI with the initial ammo
        UpdateESP32();  // Send the initial ammo to ESP32
    }

    void Update()
    {
        // Shoot when the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))  // Left mouse button for shooting
        {
            Shoot();
        }

        // Reload when the 'R' key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    public void Shoot()
    {
        if (currentBullets > 0)
        {
            currentBullets--;  // Decrease bullet count when shooting
            UpdateAmmoText();  // Update the ammo UI text
            UpdateESP32();  // Send updated ammo count to ESP32
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }

    public void Reload()
    {
        currentBullets = maxBullets;  // Reload the weapon to max bullets
        UpdateAmmoText();  // Update the ammo UI text
        UpdateESP32();  // Send updated ammo count to ESP32
    }

    void UpdateAmmoText()
    {
        ammoText.text = "Ammo: " + currentBullets;  // Update the ammo text on UI
    }

    // Send the current ammo to ESP32
    void UpdateESP32()
    {
        string url = esp32IP + "?health=" + 100 + "&ammo=" + currentBullets;  // Assuming health is fixed at 100 for now
        StartCoroutine(SendData(url));
    }

    // Coroutine to send HTTP GET request to ESP32
    IEnumerator SendData(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("ESP32 updated with ammo: " + currentBullets);
        }
    }
}
