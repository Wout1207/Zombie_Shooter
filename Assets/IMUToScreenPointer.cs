using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For displaying the point on the UI (optional)
using System.Globalization;

public class IMUToScreenPointer : MonoBehaviour
{
    // Stores the most recent IMU data
    private string latestIMUData;
    private readonly object dataLock = new object(); // Ensures thread safety
    private Quaternion latestRotation;
    // Input Euler angles (from the IMU device)
    public Vector3 eulerAngles; // Example: (yaw, pitch, roll)

    // Screen visualization point
    public RectTransform pointerUIElement; // Assign a UI element (e.g., an Image) to this in the inspector

    // Screen boundaries for mapping
    private float screenWidth;
    private float screenHeight;
    private bool firstTime = true;
    private Quaternion firstPos;
    public Quaternion absoluteRotation; // Set this quaternion dynamically
    public float speedFactor = 15.0f;
    public float distanceToCamera = 10.0f;
    
    public float rotationSpeed = 3f;
    public float rotationBoarder = 0.1f;

    public GameObject player;

    void Start()
    {
        // Get screen dimensions
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        if (SerialManager.Instance != null)
        {
            //SerialManager.Instance.OnDataReceivedIMU += convertIMUData;
            SerialManager.Instance.OnDataReceivedIMU += ReceiveIMUData;
        }

        // Set eulerAngles to the initial IMU values to point to the center
        if (firstTime && SerialManager.Instance != null)
        {
            // Simulate first IMU data if needed
            eulerAngles = new Vector3(0, 0, 0);  // Center position (adjust if required)
        }
    }

    void Update()
    {
        ProcessLatestIMUData();
        // Map quaternion rotation to screen position
        //Vector2 screenPoint = MapEulerToScreen(eulerAngles);
        //Vector2 screenPoint = MapQuaternionToScreen(absoluteRotation);
        //Vector2 screenPoint = ProjectPointPerpendicularToScreen(absoluteRotation);
        //Vector2 screenPoint = ProjectToViewPlane(absoluteRotation);

        //absoluteRotation = Quaternion.Slerp(absoluteRotation, latestRotation, Time.deltaTime * 10f);
        Vector2 screenPoint = MapQuaternionToScreen(absoluteRotation);

        Vector2 screenPos = new Vector2();
        screenPos.x = Mathf.Clamp(screenPoint.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPoint.y, 0, Screen.height);

        // Update the position of the UI element
        if (pointerUIElement != null)
        {
            if (screenPos.x < rotationBoarder * Screen.width)
            {
                player.transform.Rotate(new Vector3(0, -rotationSpeed));
            }
            else if (screenPos.x > Screen.width - rotationBoarder * Screen.width)
            {
                player.transform.Rotate(new Vector3(0, rotationSpeed));
            }

            Vector2 currentPos = pointerUIElement.anchoredPosition;
            Vector2 smoothedPos = Vector2.Lerp(currentPos, screenPos, Time.deltaTime * 10f);

            pointerUIElement.anchoredPosition = screenPoint;
            screenPoint.y = screenPoint.y + 1.63f;
        }

        // Debug: Draw a point where the IMU points on the screen
        Debug.DrawLine(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 10)), Color.red);
    }

    void ProcessLatestIMUData()
    {
        string data;

        // Retrieve the most recent data in a thread-safe way
        lock (dataLock)
        {
            data = latestIMUData;
        }

        if (string.IsNullOrEmpty(data))
            return;

        convertIMUData(data);
    }
    Vector2 ProjectPointPerpendicularToScreen(Quaternion rotation)
    {
        Vector3 direction = rotation * Vector3.forward;

        // Calculate the world position based on the direction vector
        Vector3 worldPosition = transform.position + direction;

        // Create a ray from the camera through the calculated point
        //Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(worldPosition));
        Ray ray = new Ray(Camera.main.transform.position, direction);

        // Calculate the intersection point with the camera's near plane (which is the image plane)
        //float distance = -Camera.main.transform.position.z / ray.direction.z; // Z-component tells how far along the ray the intersection is
        float distance = Mathf.Abs(Camera.main.transform.position.z) - ray.direction.z;  // Assuming camera is looking down -Z axis
        Debug.Log($"Distance: {distance}");

        // Calculate the intersection point (perpendicular projection)
        Vector3 intersectionPoint = ray.GetPoint(distance);

        // Project this point onto screen space
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(intersectionPoint);

        // Map the screen point to UI space
        Vector2 uiPoint = new Vector2(screenPoint.x, screenPoint.y);

        return uiPoint;
    }

    Vector2 ProjectToViewPlane(Quaternion rotation)
    {
        float zDistance = distanceToCamera;
        float yaw = rotation.eulerAngles.y;
        float pitch = rotation.eulerAngles.x;

        // Convert yaw and pitch to radians
        float yawRad = Mathf.Deg2Rad * yaw; // Horizontal angle around Y-axis
        float pitchRad = Mathf.Deg2Rad * pitch; // Vertical angle around X-axis

        // Calculate the direction vector based on yaw and pitch
        float xDir = Mathf.Sin(pitchRad) * Mathf.Cos(yawRad);  // Calculate x component
        float yDir = Mathf.Sin(pitchRad) * Mathf.Sin(yawRad);  // Calculate y component
        float zDir = Mathf.Cos(pitchRad); // Calculate z component (forward direction)

        // The direction vector is now in 3D space
        Vector3 direction = new Vector3(xDir, yDir, zDir);

        // Scale this direction vector by the z-distance to get the 3D position in space
        Vector3 worldPosition = Camera.main.transform.position + direction * zDistance;

        // Project the world position to screen space
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        // Convert to 2D screen coordinates (ignoring the Z value)
        Vector2 uiPoint = new Vector2(screenPoint.x, screenPoint.y);

        return uiPoint;
    }


    Vector2 MapEulerToScreen(Vector3 euler)
    {
        float screenX = (euler.y / 360f) * screenWidth;
        float screenY = (euler.x / 360f) * screenHeight;

        // Clamp to screen boundaries
        //screenX = Mathf.Clamp(screenX, 0, screenWidth);
        //screenY = Mathf.Clamp(screenY, 0, screenHeight);

        return new Vector2(screenX, screenY);
    }
    Vector2 MapQuaternionToScreen(Quaternion rotation)
    {
        //Vector3 direction = rotation * Vector3.forward;
        Vector3 direction = rotation * new Vector3(0,0,distanceToCamera);

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(direction);

        // Map the screen point to the UI space (if the UI element is inside the canvas)
        Vector2 uiPoint = new Vector2(screenPoint.x, screenPoint.y);

        // Clamp to screen boundaries
        //uiPoint.x = Mathf.Clamp(uiPoint.x, 0, screenWidth);
        //uiPoint.y = Mathf.Clamp(uiPoint.y, 0, screenHeight);

        return uiPoint;
    }

    public void ReceiveIMUData(string data)
    {
        lock (dataLock)
        {
            latestIMUData = data;
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

            Quaternion currentIMURotation = new Quaternion(y, x, -z, w); //version in gun
            //Quaternion currentIMURotation = new Quaternion(x, -z, -y, w); //Version breadboard Wout.C
            //Quaternion currentIMURotation = new Quaternion(x, -z, y, w); //Version breadboard Wout.C Experimental
            Debug.Log($"Current IMU Rotation: {currentIMURotation}");

            if (firstTime)
            {
                firstTime = false;
                firstPos = currentIMURotation;
            }

            // Calculate absolute rotation relative to the initial offset
            absoluteRotation = Quaternion.Inverse(firstPos) * currentIMURotation;
            //Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            //Quaternion finalRotation = absoluteRotation * offsetRotation;

            eulerAngles = absoluteRotation.eulerAngles;
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, absoluteRotation, Time.deltaTime * speedFactor);
        }
        else if (values.Length != 5)
        {
            Debug.LogWarning($"Unexpected data format: {data}");
        }
    }

    void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= convertIMUData;
        }
    }
}

