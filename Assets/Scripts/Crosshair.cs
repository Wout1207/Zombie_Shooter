using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

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
    public float rotationSpeed = 4f;
    public float rotationBoarder = 0.4f;

    public Quaternion absoluteRotation; // Set this quaternion dynamically
    public Vector3 screenLocation;
    private Quaternion firstPos;
    private bool firstTime = true;


    private void Start()
    {
        // Hide the system cursor
        Cursor.visible = false;

        SerialManager.Instance.OnDataReceivedIMU += ReadIMU;
        //SerialManager.Instance.OnDataReceivedIMU += translateImuDataTo2D;

        imuOffset = imuObject.localEulerAngles;


        target.transform.position = new Vector3(0, 2.43f, speedFactor);
    }

    void Update()
    {
        imuObject.parent.rotation = player.transform.rotation;
    }

    /* private void Convert3DTo2D()
     {

         // Convert quaternion to a forward vector
         Vector3 forward = absoluteRotation * Vector3.forward;
         Debug.Log($"Absolute rotation: {absoluteRotation}");
         Debug.Log($"Forward: {forward}");

         Vector3 screenPos = cam.WorldToScreenPoint(forward);
         Debug.Log("target is " + screenPos.x + " pixels from the left");
         Debug.Log("target is " + screenPos.y + " pixels from the bottom");

         // Assume the object's origin is the rotation pivot
         Vector3 worldPosition = firstPos * Vector3.forward;

         // Calculate the world space point in front of the object
         Vector3 targetWorldPosition = worldPosition + forward;

         // Project the target point onto the screen
         Vector3 screenPoint = cam.WorldToScreenPoint(targetWorldPosition);

         // Flip the Y-coordinate for OnGUI
         //screenPoint.y = Screen.height + screenPoint.y;

         // Store as a 2D screen location
         //screenLocation = new Vector2(screenPoint.x, screenPoint.y);

         // make sure the crosshair stays within the screen
         screenPos.x = Mathf.Clamp(screenPoint.x, 0, Screen.width);
         screenPos.y = screenPoint.y;
         //screenPos.y = Mathf.Clamp(screenPoint.y, -Screen.height, 0);

         Debug.Log($"Screen Location unclamped: {screenPoint}");
         Debug.Log($"Screen Pos clamped: {screenPos}");
     }*/


    public void ReadIMU(string data)
    {
        if (imuObject != null)
        {
            //imuEulerAngles = imuObject.localEulerAngles - imuOffset;
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

    /*public void convertIMUData(string data)
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

            Quaternion currentIMURotation = new Quaternion(y, x, -z, w); //version in gun
            //Quaternion currentIMURotation = new Quaternion(x, -z, -y, w); //Version breadboard Wout.C
            //Quaternion currentIMURotation = new Quaternion(x, -z, -y, w); //Version breadboard Wout.C Experimental
            Debug.Log($"Current IMU Rotation: {currentIMURotation}");

            if (firstTime)
            {
                firstTime = false;
                firstPos = currentIMURotation;
            }

            // Calculate absolute rotation relative to the initial offset
            absoluteRotation = Quaternion.Inverse(firstPos) * currentIMURotation;
            //absoluteRotation = currentIMURotation;
            //Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            //Quaternion finalRotation = absoluteRotation * offsetRotation;

            //this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, absoluteRotation, Time.deltaTime * speedFactor);
        }
        else if (values.Length != 5)
        {
            Debug.LogWarning($"Unexpected data format: {data}");
        }
    }*/

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
        GUI.Label(new Rect(screenPos.x - 20, (Screen.height - screenPos.y - 20), 40, 40), "+", style);

    }

    private void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= ReadIMU;
        }
    }

    //void Start()
    //{
    //    // Hide the system cursor
    //    Cursor.visible = false;

    //    //SerialManager.Instance.OnDataReceivedIMU += ReadIMU;
    //    SerialManager.Instance.OnDataReceivedIMU += translateImuDataTo2D;

    //    imuOffset = imuObject.localEulerAngles;


    //    target.transform.position = new Vector3(0, 2.43f, speedFactor);
    //}

    //void Update()
    //{
    //    imuObject.parent.rotation = player.transform.rotation;
    //}

    //public void ReadIMU(string data)
    //{
    //    if (imuObject != null)
    //    {
    //        imuEulerAngles = imuObject.localEulerAngles - imuOffset;
    //        screenPos = cam.WorldToScreenPoint(target.position);
    //        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
    //        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
    //        if (screenPos.x < rotationBoarder * Screen.width)
    //        {
    //            player.transform.Rotate(new Vector3(0,-rotationSpeed));
    //        }
    //        else if (screenPos.x > Screen.width - rotationBoarder * Screen.width)
    //        {
    //            player.transform.Rotate(new Vector3(0, rotationSpeed));
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning("IMU_Object not assigned!");
    //    }
    //}
}
