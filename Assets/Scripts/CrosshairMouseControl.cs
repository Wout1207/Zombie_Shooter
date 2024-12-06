using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class CrosshairMouseControl : MonoBehaviour
{
    public float sensitivity = 1.5f; // Control sensitivity for mouse movement

    private Vector3 cursorPosition; // Crosshair position on the screen

    public Camera cam; // Camera for calculating screen-space positions
    public Transform target; // Target object that the crosshair follows

    public float rotationSpeed = 2f; // Speed of player rotation
    public float rotationBorder = 0.3f; // Screen border percentage for rotation triggers

    public GameObject player; // Reference to the player object
    private bool firstTime = true;
    private Quaternion firstPos;
    public Quaternion absoluteRotation; // Set this quaternion dynamically

    private void Start()
    {
        // Hide the system cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined; // Keep cursor inside the game window

        // Initialize cursor position to the center of the screen
        cursorPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    private void Update()
    {
        // Get mouse input and update cursor position
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * sensitivity;

        cursorPosition.x = Mathf.Clamp(cursorPosition.x + mouseX, 0, Screen.width);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y - mouseY, 0, Screen.height);

        // Move the target object in world space to match the cursor position
        Ray ray = cam.ScreenPointToRay(cursorPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            target.position = hit.point;
        }

        // Handle player rotation based on cursor position
        if (cursorPosition.x < rotationBorder * Screen.width)
        {
            player.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        }
        else if (cursorPosition.x > Screen.width - rotationBorder * Screen.width)
        {
            player.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    public void convertIMUData(string data)
    {
        string[] values = data.Split('/');
        if (values.Length == 5 && values[0] == "r")
        {
            if (!float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float w) ||
                !float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
                !float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) ||
                !float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            {
                Debug.LogWarning($"Invalid quaternion values in data: {data}");
                return;
            }

            //Quaternion currentIMURotation = new Quaternion(y, x, -z, w); //version in gun
            //Quaternion currentIMURotation = new Quaternion(x, -z, -y, w); //Version breadboard Wout.C
            Quaternion currentIMURotation = new Quaternion(x, -z, -y, w); //Version breadboard Wout.C Experimental
            Debug.Log($"Current IMU Rotation: {currentIMURotation}");

            if (firstTime)
            {
                firstTime = false;
                firstPos = currentIMURotation;
            }

            // Calculate absolute rotation relative to the initial offset
            //absoluteRotation = Quaternion.Inverse(firstPos) * currentIMURotation;
            absoluteRotation = currentIMURotation;
            //Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            //Quaternion finalRotation = absoluteRotation * offsetRotation;

            //this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, absoluteRotation, Time.deltaTime * speedFactor);
        }
        else if (values.Length != 5)
        {
            Debug.LogWarning($"Unexpected data format: {data}");
        }
    }
}

