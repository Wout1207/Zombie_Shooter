using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;

    void Start()
    {
        currentHP = maxHP;
        SendHealthData();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10); 
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        SendHealthData();
        Debug.Log("Current HP: " + currentHP);
    }

    public void SendHealthData()
    {
        string message = $"H:{currentHP}\n";
        SerialManager.Instance.SendDataToESP32(message);
    }
}
