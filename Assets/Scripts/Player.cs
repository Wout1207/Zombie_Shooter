using System.Collections;  // Required for IEnumerator
using UnityEngine;
using UnityEngine.UI;  // For UI components like Text
using UnityEngine.Networking;  // For HTTP requests
using TMPro;

public class Player : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public float speed;
    public TMP_Text healthText;  // Reference to the UI Text for health

    private string esp32IP = "http://<ESP32_IP_ADDRESS>/update";  // Replace with your ESP32 IP address

    void Start()
    {
        currentHP = maxHP;
        UpdateHealthText();  // Update the UI with the initial health
        UpdateESP32();  // Send the initial health to ESP32
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);
        transform.Translate(direction * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        UpdateHealthText();  // Update the health on UI
        UpdateESP32();  // Send updated health to ESP32
    }

    void UpdateHealthText()
    {
        healthText.text = "Health: " + currentHP;  // Update the health text
    }

    // Send the current health to ESP32
    void UpdateESP32()
    {
        string url = esp32IP + "?health=" + currentHP + "&ammo=" + 50;  // Assuming ammo is 50 for now
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
            Debug.Log("ESP32 updated with health: " + currentHP);
        }
    }
}
