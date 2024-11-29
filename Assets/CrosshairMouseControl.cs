using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairMouseControl : MonoBehaviour
{
    public float sensitivity = 1.5f; // Control sensitivity for mouse movement

    private Vector3 cursorPosition; // Crosshair position on the screen

    public Camera cam; // Camera for calculating screen-space positions
    public Transform target; // Target object that the crosshair follows

    public float rotationSpeed = 2f; // Speed of player rotation
    public float rotationBorder = 0.3f; // Screen border percentage for rotation triggers

    public GameObject player; // Reference to the player object

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

    private void OnGUI()
    {
        // Draw crosshair at the cursor position
        GUIStyle style = new GUIStyle
        {
            fontSize = 50,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        GUI.Label(new Rect(cursorPosition.x - 20, (Screen.height - cursorPosition.y - 20), 40, 40), "+", style);
    }
}

