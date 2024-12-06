using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TargetShooter : MonoBehaviour
{
    //public static Action OnTargetMissed;

    [SerializeField] Camera cam;
    [SerializeField] public int currentAmmoCount = 0;
    [SerializeField] public int maxAmmoCountInMag = 10;
    [SerializeField] public int totalAmmoCount = 100;
    [SerializeField] public float reloadTime = 3f;

    public Transform imuObject; // Reference to the object that provides the IMU's rotation
    private Vector3 imuEulerAngles; // Stores IMU Euler angles

    private float lastClickTime = 0f;  // Time of the last valid button press
    public float clickCooldown = 0.2f; // Time (in seconds) to wait between clicks
    private bool isReloading = false;
    public bool isFireAmmo = true;
    public GameObject fireEffect;

    public AudioClip shootingSound;
    public AudioClip reloadSound;
    public AudioClip emptyGunSound;
    public AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        SerialManager.Instance.OnDataReceivedIMU += ReadIMU;
        SerialManager.Instance.OnDataReceivedTrigger += Shoot;
        SerialManager.Instance.OnDataReceivedRFID += readMag;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r") && totalAmmoCount != 0 && !isReloading)
        {
            StartCoroutine(Reload());
            return;
        }

        if ((Input.GetKeyDown("b") || Input.GetMouseButtonDown(0)) && !isReloading)
        {
            SerialManager.Instance.SendDataToESP32("tr/0");
            Shoot(); // Simulate the shoot locally
        }
    }



    public void readMag(string[] data) // The data is in the format "G1/M1/10" where G1: gun 1, M1: magazine 1 and 10: the capacity of the mag
    {
        Debug.Log("Reading mag data: " + data);

        if (data.Length == 4)
        {
            maxAmmoCountInMag = System.Convert.ToInt32(data[3]);
            Debug.Log("Magazine capacity: " + maxAmmoCountInMag);
        }
        StartCoroutine(Reload());
    }


    public void ReadIMU(Quaternion data)
    {
        if (imuObject != null)
        {
            // Get the IMU Euler angles
            imuEulerAngles = imuObject.eulerAngles;
        }
        else
        {
            Debug.LogWarning("IMU_Object not assigned!");
        }
    }

    public void Shoot()
    {
        float currentTime = Time.time;
        
        if (currentTime - lastClickTime >= clickCooldown && !isReloading) // Check if enough time has passed since the last click
        {
            lastClickTime = currentTime;

            Debug.Log("Shooting...");
                ShootRay();
            if (currentAmmoCount == 0)
            {
                Debug.Log("Out of ammo!");
            }
        }
    }

    public void ShootRay()
    {
        //Debug.Log("I am in ShootRay()");
        Vector3 screenPos = cam.WorldToScreenPoint(imuObject.GetChild(0).position);
        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
        screenPos = new Vector3(screenPos.x, (Screen.height - screenPos.y));
        Ray ray = cam.ScreenPointToRay(screenPos);
        
        GameEvents.current.ShotFired();
        AddAmmo(-1);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            DoorController controller = hit.collider.gameObject.GetComponent<DoorController>();
            if (controller)
            {
                AddAmmo(1);
                controller.hit();
                return;
            }
            Target target = hit.collider.gameObject.GetComponent<Target>();

            if (target != null && currentAmmoCount > 0) // waarom alleen ammo verliezen als je raakt?
            {
                Debug.Log("target hit");
                audioSource.clip = shootingSound;
                audioSource.Play();
                //AddAmmo(-1);
                target.Hit(10);
                if(isFireAmmo)
                {
                    FireEffect fire = target.transform.GetComponentInChildren<FireEffect>();
                    if (fire)
                    {
                        fire.duration += fireEffect.GetComponent<FireEffect>().duration;
                    }
                    else
                    {
                        Instantiate(fireEffect, target.transform);
                    }
                }
            }
            else if(currentAmmoCount < 0)
            {
                AddAmmo(1);
                audioSource.clip = emptyGunSound;
                audioSource.Play();
            }
            else
            {
                audioSource.clip = shootingSound;
                audioSource.Play();
                //OnTargetMissed?.Invoke();
            }
        }
        else if (currentAmmoCount < 0)
        {
            AddAmmo(1);
        }
        else
        {
            //OnTargetMissed?.Invoke();
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        audioSource.clip = reloadSound;
        audioSource.Play();
        yield return new WaitForSeconds(reloadTime); // Wait for reload time
        if (totalAmmoCount >= maxAmmoCountInMag)
        {
            totalAmmoCount -= maxAmmoCountInMag - currentAmmoCount;
            currentAmmoCount = maxAmmoCountInMag;
        }
        else
        {
            currentAmmoCount = totalAmmoCount;
            totalAmmoCount = 0;
        }
        sendToGun("rb"); // "rb" for reloading, "b" for bullet update/shot 
        Debug.Log("Reload complete! Ammo refilled.");
        isReloading = false;
    }

    private void sendToGun(string type)
    {
        //fromat "b/15/10"
        if (type == "rb" || type == "b")
        {
            string message = type + "/" + maxAmmoCountInMag + "/" + currentAmmoCount;
            SerialManager.Instance.SendDataToESP32(message);
        }
        else
        {
            Debug.Log("Invalid message type");
        }

    }

    public void AddAmmo(int amount)
    {
        currentAmmoCount += amount;
        Debug.Log("Ammo: " + currentAmmoCount);

        sendToGun("b"); // "rb" for reloading, "b" for bullet update/shot 
    }

    private void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= ReadIMU;
        }

        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedTrigger -= Shoot;
        }
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedRFID -= readMag;
        }
    }
}
