using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetShooter : MonoBehaviour
{
    //public static Action OnTargetMissed;

    [SerializeField] Camera cam;
    [SerializeField] public int ammoCount = 10;

    public Transform imuObject; // Reference to the object that provides the IMU's rotation
    private Vector3 imuEulerAngles; // Stores IMU Euler angles

    // Start is called before the first frame update
    void Start()
    {
        SerialManager.Instance.OnDataReceived += ReadIMU;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space") && ammoCount > 0)
        {
            //Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            //Ray ray = imuEulerAngles;
            Ray ray = new Ray(imuObject.position, imuObject.forward);

            AddAmmo(-1);
            GameEvents.current.ShotFired();



            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Target target = hit.collider.gameObject.GetComponent<Target>();

                if (target != null)
                {
                    target.Hit();
                }
                else
                {
                    //OnTargetMissed?.Invoke();
                }
            }
            else
            {
                //OnTargetMissed?.Invoke();
            }
        }
    }

    //public void OnDataReceived(string data)
    //{
    //    Debug.Log("Received from ESP32 in targetShoother: " + data);
    //}

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

    public void AddAmmo(int amount)
    {
        ammoCount += amount;
        Debug.Log("Ammo: " + ammoCount);
    }

    private void OnDestroy()
    {
        SerialManager.Instance.OnDataReceived -= ReadIMU;
    }
}
