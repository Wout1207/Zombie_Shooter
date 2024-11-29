using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{

    public Transform imuObject; // Reference to the object that provides the IMU's rotation
    private Vector3 imuEulerAngles; // Stores IMU Euler angles

    public float sensitivity = 1.0f; // Control sensitivity for cursor movement

    private float cursorX, cursorY; // Cursor screen coordinates
    private Vector3 imuOffset;

    public Camera cam;
    public Transform target;
    private Vector3 screenPos;
    public float speedFactor = 15.0f;
    public GameObject player;
    public float rotationSpeed = 2f;
    public float rotationBoarder = 0.15f;

    void Start()
    {
        // Hide the system cursor
        Cursor.visible = false;

        SerialManager.Instance.OnDataReceivedIMU += ReadIMU;

        imuOffset = imuObject.localEulerAngles;


        target.transform.position = new Vector3(0, 2.43f, speedFactor);
    }

    void Update()
    {
        imuObject.parent.rotation = player.transform.rotation;
    }

    public void ReadIMU(string data)
    {
        if (imuObject != null)
        {
            imuEulerAngles = imuObject.localEulerAngles - imuOffset;
            screenPos = cam.WorldToScreenPoint(target.position);
            screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
            if (screenPos.x < rotationBoarder * Screen.width)
            {
                player.transform.Rotate(new Vector3(0,-rotationSpeed));
            }
            else if (screenPos.x > Screen.width - rotationBoarder * Screen.width)
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

        //GUI.Label(new Rect(screenPos.x - 20, (Screen.height - screenPos.y - 20), 40, 40), "+", style);
        GUI.Label(new Rect(screenPos.x -20, (Screen.height - screenPos.y -20), 40, 40), "+", style);

    }

    private void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= ReadIMU;
        }
    }

}
