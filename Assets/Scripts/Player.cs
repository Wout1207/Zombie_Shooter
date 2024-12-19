using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private float deadTime;

    public RandomAudioPlayer startGamePlayer;
    public RandomAudioPlayer grenadePlayer;
    public RandomAudioPlayer damagePlayer;

    void Start()
    {
        currentHP = maxHP;
        currentGrenadeAmount = maxGrenadeAmount;
        //SendHealthData();
        //SerialManager.Instance.OnDataReceivedMovement += readMovement;
        if (SerialManager.instance != null)
        {
            SerialManager.instance.OnDataReceivedMovement += readMovement;
            SerialManager.instance.OnDataReceivedGrenade += readGrenadeData;
        }
        Score.score = 0;
        startGamePlayer.PlayVoiceLine();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            throwGrenade();
            grenadePlayer.PlayVoiceLine();
        }
        if (grenadeToBeThrown)
        {
            throwGrenade();
            grenadePlayer.PlayVoiceLine();
            grenadeToBeThrown = false;
        }
        if (!alive && Time.time - deadTime > 5)
        {
            SceneManager.LoadScene("GameOverScene");
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
            transform.Translate(movementDirection.normalized * speed * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        SendHealthData();
        Debug.Log("Current HP: " + currentHP);
        if (currentHP <= 0 && alive)
        {
            PlayerDied();
        }
        damagePlayer.PlayVoiceLine();
    }

    public void PlayerDied()
    {
        alive = false;
        gameOverText.SetActive(true);
        GameEvents.current.playerDead();
        gameOverText.SetActive(true);
        deadTime = Time.time;
    }

    public void SendHealthData()
    {
        string message = $"H:{currentHP}\n";
        SerialManager.instance.SendDataToESP32(message);
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
                        movementDirection = state == 1 ? movementDirection + Vector3.forward : movementDirection - Vector3.forward;
                        break;

                    case 2: // Pin 2 is right
                        movementDirection = state == 1 ? movementDirection + Vector3.right : movementDirection - Vector3.right;
                        break;

                    case 3: // Pin 3 is backward
                        movementDirection = state == 1 ? movementDirection + Vector3.back : movementDirection - Vector3.back;
                        break;

                    case 4: // Pin 4 is left
                        movementDirection = state == 1 ? movementDirection + Vector3.left : movementDirection - Vector3.left;
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
            grenade.GetComponent<Rigidbody>().velocity = transform.forward * 10;
            grenade.transform.SetParent(transform.parent);
            grenade.transform.localScale = new Vector3(4, 4, 4);
        }
    }

    private void OnDestroy()
    {
        if (SerialManager.instance != null)
        {
            //SerialManager.Instance.OnDataReceivedMovement -= readMovement;
        }
    }
}
