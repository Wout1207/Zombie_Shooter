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

    private bool isJammed = false; 
    [SerializeField] private int shakesRequiredToDejam = 3; 
    [SerializeField] private float shakeThreshold = 5f; 
    private int shakeCount = 0; 
    private Vector3 lastIMUReading = Vector3.zero;
    [SerializeField] public float jamRandVal = 0.1f;

    private float lastClickTime = 0f;  // Time of the last valid button press
    public float clickCooldown = 0.2f; // Time (in seconds) to wait between clicks
    private bool isReloading = false;
    public bool isFireAmmo = true;
    public GameObject fireEffect;

    public AudioClip shootingSound;
    public AudioClip reloadSound;
    public AudioClip emptyGunSound;
    public AudioSource audioSource;

    public ParticleSystem shotParticles;

    // Start is called before the first frame update
    void Start()
    {
        SerialManager.Instance.OnDataReceivedIMU += ReadIMU;
        SerialManager.Instance.OnDataReceivedTrigger += Shoot;
        SerialManager.Instance.OnDataReceivedRFID += readMag;

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r") && totalAmmoCount != 0 && !isReloading)
        {
            //if (isJammed) return;
            StartCoroutine(Reload());
            return;
        }

        if ((Input.GetKeyDown("b") || Input.GetMouseButtonDown(0)) && !isReloading)
        {
            SerialManager.Instance.SendDataToESP32("tr/0");
            Shoot(); // Simulate the shoot locally
        }
    }

    int prevMag = -1;

    public void readMag(string[] data) // The data is in the format "G1/1/10" where G1: gun 1, M1: magazine 1 and 10: the capacity of the mag
                                       // The data you receive here is in the format "mg/G1/1/10" the mg is a prefix to indicate that the data is for the magazine
    {
        int currentMag = System.Convert.ToInt32(data[2]);
        Debug.Log("Current magazine: " + currentMag);
        if (currentMag != prevMag)
        {
            prevMag = currentMag;
            maxAmmoCountInMag = System.Convert.ToInt32(data[3]);
            Debug.Log("Magazine capacity: " + maxAmmoCountInMag);
            StartCoroutine(Reload());
        }
    }


    public void ReadIMU(Quaternion data)
    {
        Vector3 currentIMUReading = data.eulerAngles;

        if (isJammed)
        {
            float imuDelta = Vector3.Distance(currentIMUReading, lastIMUReading);
            Debug.Log($"IMU delta: {imuDelta}, Threshold: {shakeThreshold}");

            if (imuDelta > shakeThreshold)
            {
                shakeCount++;
                Debug.Log($"Shake detected! Shake count: {shakeCount}/{shakesRequiredToDejam}");

                if (shakeCount >= shakesRequiredToDejam)
                {
                    ClearJam();
                }
            }
        }

        lastIMUReading = currentIMUReading;
    }

    public void TriggerJam()
    {
        if (isJammed) return;

        isJammed = true;
        shakeCount = 0;
        Debug.Log("Gun jammed! Shake to clear.");
        GameEvents.current.GunJammed();
    }

    private void ClearJam()
    {
        isJammed = false;
        shakeCount = 0;
        Debug.Log("Gun de-jammed!");
        GameEvents.current.GunDejammed(); 
    }


    public void Shoot()
    {
        //Debug.Log("Shoot() called");
/*        if (isJammed)
        {
            Debug.Log("Gun is jammed! Cannot shoot.");
            return;
        }*/

        //Debug.Log("Shoot() called");
        float currentTime = Time.time;
        
        //if (currentTime - lastClickTime >= clickCooldown && !isReloading) // Check if enough time has passed since the last click
        if (currentTime - lastClickTime >= clickCooldown) // Check if enough time has passed since the last click
        {
            lastClickTime = currentTime;

            //Debug.Log("Shooting...");
            ShootRay();
            
            if (currentAmmoCount == 0)
            {
                Debug.Log("Out of ammo!");
            }
        }
    }

    public void ShootRay()
    {
        //Debug.Log("ShootRay() called");
        Ray ray = cam.ScreenPointToRay(lastIMUReading);

        GameEvents.current.ShotFired();

        if (isJammed)
        {
            Debug.Log("Cannot fire; gun is jammed.");
            AddAmmo(1);
            return;
        }
        if (Random.value < jamRandVal && currentAmmoCount>= 0)
        {
            Debug.Log("rand val is below 10%");
            TriggerJam();
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            DoorController controller = hit.collider.gameObject.GetComponent<DoorController>();
            if (controller)
            {
                controller.hit();
                return;
            }

            Exit exit = hit.collider.gameObject.GetComponent<Exit>();
            if (exit)
            {
                if (exit.hit())
                {
                    Debug.Log("Congratulations. You have escaped");
                }
                else
                {
                    Debug.Log("Your score is not high enough");
                }
                return;
            }
            if (isReloading)
            {
                Debug.Log("Cannot shoot while reloading.");
                string[] strings = { "still reloading", "0.5" };
                GameEvents.current.OutofAmmo(strings);
                return;
            }
            if (currentAmmoCount <= 0)
            {
                audioSource.clip = emptyGunSound;
                audioSource.Play();
                string[] strings = { "out of ammo reload", "0.5" };
                GameEvents.current.OutofAmmo(strings);
                return;
            }
            AddAmmo(-1);
            Target target = hit.collider.gameObject.GetComponent<Target>();

            if (target != null && currentAmmoCount > 0)
            {

                Debug.Log("target hit");
                audioSource.clip = shootingSound;
                audioSource.Play();
                shotParticles.Play();
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
            //else if(currentAmmoCount < 0)
            //{
            //    AddAmmo(1);
            //    audioSource.clip = emptyGunSound;
            //    audioSource.Play();
            //}
            else
            {
                audioSource.clip = shootingSound;
                audioSource.Play();
                shotParticles.Play();
                //OnTargetMissed?.Invoke();
            }
        }
        else if (currentAmmoCount < 0)
        {
            AddAmmo(1);
        }
    }

    private IEnumerator Reload()
    {
        if (isJammed) yield break; 
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
        sendToGun("b"); // "rb" for reloading, "b" for bullet update/shot 
    }

    private void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= ReadIMU;
            SerialManager.Instance.OnDataReceivedTrigger -= Shoot;
            SerialManager.Instance.OnDataReceivedRFID -= readMag;
        }
    }
}
