using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float maxHP = 100;
    public float currentHP;
    public float speed;
    public GameObject gameOverText;
    public GameObject grenadePrefab;
    public int currentGrenadeAmount;
    public int maxGrenadeAmount;

    private Vector3 movementDirection = Vector3.zero;
    private bool grenadeToBeThrown = false;

    private bool alive = true;

    public WalkingSoundScript walkingSoundScript;

    void Start()
    {
        currentHP = maxHP;
        currentGrenadeAmount = maxGrenadeAmount;
        //SendHealthData();
        //SerialManager.Instance.OnDataReceivedMovement += readMovement;
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedMovement += readMovement;
            SerialManager.Instance.OnDataReceivedGrenade += readGrenadeData;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            throwGrenade();
        }
        if (grenadeToBeThrown)
        {
            throwGrenade();
            grenadeToBeThrown = false;
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10); 
        }
        float horizontalMovement = Input.GetAxis("Horizontal");
        float verticalMovement = Input.GetAxis("Vertical");
        if(alive)
        {
            if(horizontalMovement != 0 || verticalMovement != 0 || movementDirection != Vector3.zero)
            {
                walkingSoundScript.isWalking = true;
            }
            else
            {
                walkingSoundScript.isWalking = false;
            }
            transform.Translate(new Vector3(horizontalMovement * Time.deltaTime * speed, 0, verticalMovement * Time.deltaTime * speed));
            transform.Translate(movementDirection * speed * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        SendHealthData();
        Debug.Log("Current HP: " + currentHP);
        if (currentHP <= 0)
        {
            PlayerDied();
        }
    }

    public void PlayerDied()
    {
        alive = false;
        gameOverText.SetActive(true);
        GameEvents.current.playerDead();
    }

    public void SendHealthData()
    {
        string message = $"H:{currentHP}\n";
        SerialManager.Instance.SendDataToESP32(message);
    }

    public void readMovement(string[] data) // The data is in the format "m/pin/state"
    {
        string[] values = data;

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

    public void readGrenadeData()
    {
        grenadeToBeThrown = true;
        //string[] values = data.Split("/");
        //if (values[0] == "g")
        //{
        //    grenadeToBeThrown = true;
        //}
    }

    public void throwGrenade()
    {
        if (currentGrenadeAmount>0) 
        {
            currentGrenadeAmount -= 1;
            GameObject grenade = Instantiate(grenadePrefab,transform);
            grenade.transform.position += new Vector3(0,2,transform.forward.z * 2);
            grenade.GetComponent<Rigidbody>().velocity = transform.forward * 20;
            grenade.transform.SetParent(transform.parent);
            grenade.transform.localScale = new Vector3(4, 4, 4);
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
