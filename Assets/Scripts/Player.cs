using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public float speed;
    public GameObject gameOverText;

    void Start()
    {
        currentHP = maxHP;
        //SendHealthData();
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10); 
        }
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(horizontalMovement * Time.deltaTime * speed, 0, verticalMovement * Time.deltaTime * speed));
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        SendHealthData();
        Debug.Log("Current HP: " + currentHP);
        if (currentHP <= 0)
        {
            gameOverText.SetActive(true);
        }
    }

    public void SendHealthData()
    {
        string message = $"H:{currentHP}\n";
        SerialManager.Instance.SendDataToESP32(message);
    }
}
