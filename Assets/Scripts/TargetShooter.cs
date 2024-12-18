using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class TargetShooter : MonoBehaviour
{
    //public static Action OnTargetMissed;

    [SerializeField] Camera cam;
    [SerializeField] public int currentAmmoCount = 0;
    [SerializeField] public int maxAmmoCountInMag = 10;
    [SerializeField] public int totalAmmoCount = 100;
    [SerializeField] public float reloadTime = 3f;

    public bool isJammed = false; 
    [SerializeField] private int shakesRequiredToDejam = 3; 
    [SerializeField] private float shakeThreshold = 5f; 
    private int shakeCount = 0; 
    private Vector3 lastIMUReading = Vector3.zero;
    [SerializeField] public float jamRandVal = 0.02f;
    public GameObject reticle;

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
    public GameObject hitParticles;
    public GameObject NormalHitParticles;
    public GameObject ZombieHitParticles;
    int prevMag = -1;

    public RandomAudioPlayer outOfAmmoPlayer;
    public RandomAudioPlayer reloadingPlayer;
    public RandomAudioPlayer victoryPlayer;
    public RandomAudioPlayer immuneZombie;

    public WeaponAnim weaponAnim;


    // Start is called before the first frame update
    void Start()
    {
        SerialManager.instance.OnDataReceivedIMU += ReadIMU;
        SerialManager.instance.OnDataReceivedTrigger += Shoot;
        SerialManager.instance.OnDataReceivedRFID += readMag;

        audioSource = GetComponent<AudioSource>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        SerialManager.instance.SendDataToESP32("rb/0/0");
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
            SerialManager.instance.SendDataToESP32("tr/0");
            Shoot(); // Simulate the shoot locally
        }
    }

    
    public void readMag(string[] data) // The data is in the format "G1/1/10" where G1: gun 1, M1: magazine 1 and 10: the capacity of the mag
                                       // The data you receive here is in the format "mg/G1/1/10" the mg is a prefix to indicate that the data is for the magazine
    {
        int currentMag = System.Convert.ToInt32(data[2]);
        if (currentMag != prevMag)
        {
            prevMag = currentMag;
            maxAmmoCountInMag = System.Convert.ToInt32(data[3]);
            if (currentMag == 3) {
                isFireAmmo = true;
            }
            else
            {
                isFireAmmo = false;
            }
            StartCoroutine(Reload());
        }
    }


    public void ReadIMU(Quaternion data)
    {
        Vector3 currentIMUReading = data.eulerAngles;

        if (isJammed)
        {
            float imuDelta = Vector3.Distance(currentIMUReading, lastIMUReading);
            //Debug.Log($"IMU delta: {imuDelta}, Threshold: {shakeThreshold}");

            if (imuDelta > shakeThreshold)
            {
                shakeCount++;
                //Debug.Log($"Shake detected! Shake count: {shakeCount}/{shakesRequiredToDejam}");

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
        if(!SceneManager.GetActiveScene().name.Equals("SampleScene"))
        {
            return;
        }
        if (isJammed) return;

        isJammed = true;
        shakeCount = 0;
        //Debug.Log("Gun jammed! Shake to clear.");
        GameEvents.current.GunJammed();
    }

    private void ClearJam()
    {
        isJammed = false;
        shakeCount = 0;
        //Debug.Log("Gun de-jammed!");
        GameEvents.current.GunDejammed(); 
    }


    public void Shoot()
    {
        if (SceneManager.GetActiveScene().name == "SampleScene")
        {
            float currentTime = Time.time;


            if (currentTime - lastClickTime >= clickCooldown) // Check if enough time has passed since the last click
            {
                lastClickTime = currentTime;

                //Debug.Log("Shooting...");
                ShootRay();

                if (currentAmmoCount == 0)
                {
                    //Debug.Log("Out of ammo!");
                    outOfAmmoPlayer.PlayVoiceLine();
                }
            }
        }
        else
        {
            ShootRay();
        }

    }

    public void ShootRay()
    {
        Ray ray = cam.ScreenPointToRay(reticle.transform.position);
        
        if (SceneManager.GetActiveScene().name != "SampleScene")
        {
            GameObject clickedObject = GetUIElementUnderReticle();
            if (clickedObject != null)
            {
                Button button = clickedObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
            }
            return;
        }

        if (GameEvents.current)
        {
            GameEvents.current.ShotFired();
        }

        if (isJammed)
        {
            //Debug.Log("Cannot fire; gun is jammed.");
            return;
        }
        

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Exit exit = hit.collider.gameObject.GetComponent<Exit>();
            if (exit)
            {
                if (exit.hit())
                {
                    Debug.Log("Congratulations. You have escaped");
                    victoryPlayer.PlayVoiceLine();
                }
                else
                {
                    Debug.Log("Your score is not high enough");
                }
                return;
            }
            if (isReloading)
            {
                //Debug.Log("Cannot shoot while reloading.");
                string[] strings = { "Still reloading!", "0.5" };
                GameEvents.current.OutofAmmo(strings);
                return;
            }
            if (currentAmmoCount <= 0)
            {
                audioSource.clip = emptyGunSound;
                audioSource.Play();
                string[] strings = { "Out of ammo! Reload!", "0.5" };
                GameEvents.current.OutofAmmo(strings);
                return;
            }
            AddAmmo(-1);
            GameObject particles;
            particles = Instantiate(NormalHitParticles);
            
            DoorController controller = hit.collider.gameObject.GetComponent<DoorController>();
            Debug.Log((controller));
            if (controller && currentAmmoCount > 0)
            {
                audioSource.clip = shootingSound;
                audioSource.Play();
                shotParticles.Play();
                controller.Hit(10);
                weaponAnim.TriggerRecoil();
                return;
            }

            RandomGenJam();

            Target target = hit.collider.gameObject.GetComponent<Target>();

            if (target != null && currentAmmoCount > 0)
            {
                particles = Instantiate(ZombieHitParticles);
                audioSource.clip = shootingSound;
                audioSource.Play();
                shotParticles.Play();
                weaponAnim.TriggerRecoil();
                if (target is BulletImuneZombie bulletZombie)
                {
                    if (isFireAmmo)
                    {
                        bulletZombie.fireHit(10);
                    }
                    else
                    {
                        bulletZombie.Hit(10);
                        immuneZombie.PlayVoiceLine();
                    }
                }
                else if (target is FireResistantZombie fireZombie)
                {
                    if (isFireAmmo)
                    {
                        fireZombie.fireHit(10);
                        immuneZombie.PlayVoiceLine();
                    }
                    else
                    {
                        fireZombie.Hit(10);
                    }
                }
                else
                {
                    target.Hit(10);
                }
                if (isFireAmmo)
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
            else
            {
                audioSource.clip = shootingSound;
                audioSource.Play();
                shotParticles.Play();
                weaponAnim.TriggerRecoil();
            }

            particles.transform.position = hit.point;
            particles.transform.rotation = Quaternion.LookRotation(hit.normal);
            Destroy(particles, 5);
        }
        else if (currentAmmoCount < 0)
        {
            AddAmmo(1);
        }
        else if (currentAmmoCount > 0)
        {
            AddAmmo(-1);
            audioSource.clip = shootingSound;
            audioSource.Play();
            shotParticles.Play();
            weaponAnim.TriggerRecoil();
        }
    }

    private void RandomGenJam()
    {
        if (Random.value < jamRandVal && currentAmmoCount >= 0)
        {
            TriggerJam();
            return;
        }
    }

    private GameObject GetUIElementUnderReticle()
    {
        // Convert the reticle position into a screen position
        Vector2 screenPoint = reticle.transform.position;

        // Create a PointerEventData object
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = screenPoint // The position of the reticle on the screen
        };

        // Perform a GraphicRaycaster Raycast
        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        if (raycastResults.Count > 0)
        {
            //Debug.Log("UI element hit: " + raycastResults[0].gameObject.name);
            return raycastResults[0].gameObject; // Return the first hit UI element
        }

        return null;
    }

    private IEnumerator Reload()
    {
        if (isJammed) yield break; 
        isReloading = true;
        //Debug.Log("Reloading...");
        reloadingPlayer.PlayVoiceLine();
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
            SerialManager.instance.SendDataToESP32(message);
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
        SerialManager.instance.SendDataToESP32("rb/0/0");

        if (SerialManager.instance != null)
        {
            SerialManager.instance.OnDataReceivedIMU -= ReadIMU;
            SerialManager.instance.OnDataReceivedTrigger -= Shoot;
            SerialManager.instance.OnDataReceivedRFID -= readMag;
        }
    }
}
