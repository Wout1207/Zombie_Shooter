using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetShooter : MonoBehaviour
{
    //public static Action OnTargetMissed;

    [SerializeField] Camera cam;
    [SerializeField] public int currentAmmoCount = 0;
    [SerializeField] public int maxAmmoCountInMag = 10;
    [SerializeField] public int totalAmmoCount = 100;
    [SerializeField] public float reloadTime = 2f;

    public Transform imuObject; // Reference to the object that provides the IMU's rotation
    private Vector3 imuEulerAngles; // Stores IMU Euler angles

    private float lastClickTime = 0f;  // Time of the last valid button press
    public float clickCooldown = 0.2f; // Time (in seconds) to wait between clicks
    private bool isReloading = false;

    // Start is called before the first frame update
    void Start()
    {
        SerialManager.Instance.OnDataReceivedIMU += ReadIMU;
        SerialManager.Instance.OnDataReceivedTrigger += Shoot;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r") && totalAmmoCount != 0 && !isReloading)
        {
            StartCoroutine(Reload());
            return;
        }

        // Check for shooting input
        if (Input.GetKeyDown("space") && !isReloading)
        {
            ShootRay();
        }
    }

    public void ReadIMU(string data)
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

    public void Shoot(string data)
    {
        float currentTime = Time.time;
        
        if (currentTime - lastClickTime >= clickCooldown && !isReloading) // Check if enough time has passed since the last click
        {
            lastClickTime = currentTime;

            int shot = System.Convert.ToInt32(data);
            if (currentAmmoCount > 0 && shot == 0)
            {
                ShootRay();
            }
            else if (currentAmmoCount == 0)
            {
                Debug.Log("Out of ammo!");
            }
        }
    }

    public void ShootRay()
    {
        Ray ray = new Ray(imuObject.position, imuObject.forward);
        
        GameEvents.current.ShotFired();

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Target target = hit.collider.gameObject.GetComponent<Target>();

            if (target != null && currentAmmoCount > 0)
            {
                AddAmmo(-1);
                target.Hit();
            }
            else
            {
                DoorController controller = hit.collider.gameObject.GetComponent<DoorController>();
                if (controller)
                {
                    controller.hit();
                }
                else
                {
                    //OnTargetMissed?.Invoke();
                }
            }
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
        Debug.Log("Reload complete! Ammo refilled.");
        isReloading = false;
    }

    public void AddAmmo(int amount)
    {
        currentAmmoCount += amount;
        Debug.Log("Ammo: " + currentAmmoCount);
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
    }
}
