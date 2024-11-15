using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{

    public Transform imuObject; // Reference to the object that provides the IMU's rotation
    private Vector3 imuEulerAngles; // Stores IMU Euler angles

    public float sensitivity = 1.0f; // Control sensitivity for cursor movement

    private float cursorX, cursorY; // Cursor screen coordinates

    public Camera cam;
    public Transform target;
    private Vector3 screenPos;
    public float speedFactor = 15.0f;
    public GameObject player;
    public float rotationSpeed;

    void Start()
    {
        // Hide the system cursor
        Cursor.visible = false;

        SerialManager.Instance.OnDataReceivedIMU += ReadIMU;

        target.transform.position = new Vector3(0, 0, speedFactor);
    }

    void Update()
    {

    }

    public void ReadIMU(string data)
    {
        if (imuObject != null)
        {
            screenPos = cam.WorldToScreenPoint(target.position);
            screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
            if (screenPos.x < 0.1*Screen.width)
            {
                player.transform.Rotate(new Vector3(0,-rotationSpeed));
            }
            else if (screenPos.x > Screen.width - 0.1 * Screen.width)
            {
                player.transform.Rotate(new Vector3(0, rotationSpeed));
            }
        }
        else
        {
            Debug.LogWarning("IMU_Object not assigned!");
        }
    }

    void OnGUI()
    {
        // Create a custom GUIStyle for the label
        GUIStyle style = new GUIStyle
        {
            fontSize = 50,  // Adjust the size to your liking
            fontStyle = FontStyle.Bold,  // Make the font bold for better visibility
            alignment = TextAnchor.MiddleCenter
        };

        GUI.Label(new Rect(screenPos.x - 20, (Screen.height - screenPos.y - 20), 40, 40), "+", style);

    }

    private void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= ReadIMU;
        }
    }

}
