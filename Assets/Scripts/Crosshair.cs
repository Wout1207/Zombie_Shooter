using System.Collections;
using System.Collections.Generic;
using Uduino;
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

    void Start()
    {
        // Hide the system cursor
        Cursor.visible = false;
        UduinoManager.Instance.OnDataReceived += ReadIMU;
        target.transform.position = new Vector3(0, 0, speedFactor);
    }

    void Update()
    {

    }

    public void ReadIMU(string data, UduinoDevice device)
    {
        if (imuObject != null)
        {
            // Get the IMU Euler angles
            //imuEulerAngles = imuObject.eulerAngles;

            screenPos = cam.WorldToScreenPoint(target.position);
            //Debug.Log("target is X: " + screenPos.x + " pixels from the left");
            //Debug.Log("target is Y: " + screenPos.y + " pixels from the bottom");

            //// Map IMU yaw (Y) to horizontal movement and pitch (X) to vertical movement
            //cursorX = Mathf.Tan(imuEulerAngles.y) * sensitivity + (Screen.width / 2); // Move cursor horizontally
            //cursorY = Mathf.Tan(imuEulerAngles.x) * sensitivity + (Screen.height / 2); // Move cursor vertically

            //// Clamp the cursor position to the screen bounds
            //cursorX = Mathf.Clamp(cursorX, 0, Screen.width);
            //cursorY = Mathf.Clamp(cursorY, 0, Screen.height);
            //Debug.Log("Cursor position: " + cursorX + ", " + cursorY);
        }
        else
        {
            Debug.LogWarning("IMU_Object not assigned!");
        }
    }

    void OnGUI()
    {
        //// Create a custom GUIStyle for the label
        //GUIStyle style = new GUIStyle();

        //// Increase the font size to make the "+" symbol larger
        //style.fontSize = 50;  // Adjust the size to your liking

        //// Optionally, make the font bold for a fatter appearance
        //style.fontStyle = FontStyle.Bold;

        //// Set the alignment of the text to the center
        //style.alignment = TextAnchor.MiddleCenter;

        // Create a custom GUIStyle for the label
        GUIStyle style = new GUIStyle
        {
            fontSize = 50,  // Adjust the size to your liking
            fontStyle = FontStyle.Bold,  // Make the font bold for better visibility
            alignment = TextAnchor.MiddleCenter
        };

        // Define the position of the "+" symbol on the screen
        //float xMin = (Screen.width / 2) - 20;  // Adjust to position the "+" symbol
        //float yMin = (Screen.height / 2) - 20;

        // Draw the "+" symbol with the custom style
        //GUI.Label(new Rect(xMin, yMin, 40, 40), "+", style);
        //GUI.Label(new Rect(cursorX - 20, cursorY - 20, 40, 40), "+", style);
        GUI.Label(new Rect(screenPos.x - 20, (Screen.height - screenPos.y - 20), 40, 40), "+", style);

    }

}
