using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public float speed;
    public GameObject gameOverText;

    private Vector3 movementDirection = Vector3.zero;

    void Start()
    {
        currentHP = maxHP;
        //SendHealthData();
        //SerialManager.Instance.OnDataReceivedMovement += readMovement;
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10); 
        }
        //float horizontalMovement = Input.GetAxis("Horizontal");
        //float verticalMovement = Input.GetAxis("Vertical");
        //transform.Translate(new Vector3(horizontalMovement * Time.deltaTime * speed, 0, verticalMovement * Time.deltaTime * speed));
        transform.Translate(movementDirection * speed * Time.deltaTime);
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

    public void readMovement(string data) // The data is in the format "m/pin/state"
    {
        string[] values = data.Split('/');

        Debug.Log("Reading movement data: " + data);

        if (values.Length == 3)
        {
            int pin, state;

            if (int.TryParse(values[1], out pin) && int.TryParse(values[2], out state))
            {
                // Modify movementDirection based on the pin and state
                switch (pin)
                {
                    case 1: // Pin 1 is forward
                        movementDirection = state == 1 ? Vector3.forward : Vector3.zero;
                        break;

                    case 2: // Pin 2 is right
                        movementDirection = state == 1 ? Vector3.right : Vector3.zero;
                        break;

                    case 3: // Pin 3 is backward
                        movementDirection = state == 1 ? Vector3.back : Vector3.zero;
                        break;

                    case 4: // Pin 4 is left
                        movementDirection = state == 1 ? Vector3.left : Vector3.zero;
                        break;

                    default:
                        Debug.LogWarning("Unhandled pin: " + pin);
                        break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            //SerialManager.Instance.OnDataReceivedMovement -= readMovement;
        }
    }
}
