using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class Crosshair : MonoBehaviour
{
    // Stores the most recent IMU data
    private string[] latestIMUData;
    private readonly object dataLock = new object(); // Ensures thread safety
    private Quaternion latestRotation;

    // Screen visualization point
    public RectTransform pointerUIElement; // Assign a UI element (e.g., an Image) to this in the inspector

    // Screen boundaries for mapping
    private float screenWidth;
    private float screenHeight;
    //private bool firstTime = true;
    private Quaternion firstPos;
    public Quaternion absoluteRotation; // Set this quaternion dynamically

    public float rotationSpeed = 3f;
    public float rotationBoarder = 0.1f;

    private GameObject player;
    public float speedFactor = 15.0f;

    private Coroutine pointerUpdateCoroutine;

    [SerializeField] private Image leftArrow;
    [SerializeField] private Image rightArrow;


    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        // Get screen dimensions
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU += ReceiveIMUData;
        }

        pointerUpdateCoroutine = StartCoroutine(UpdatePointerRoutine());
        leftArrow.enabled = false;
        rightArrow.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the position of the UI element
        //if (pointerUIElement != null)
        //{
        //    Vector2 screenPoint = MapQuaternionToScreen(absoluteRotation);

        //    Vector2 screenPos = new Vector2();
        //    screenPos.x = Mathf.Clamp(screenPoint.x, 0, Screen.width);
        //    screenPos.y = Mathf.Clamp(screenPoint.y, 0, Screen.height);

        //    if (screenPos.x < rotationBoarder * Screen.width)
        //    {
        //        player.transform.Rotate(new Vector3(0, -rotationSpeed));
        //    }
        //    else if (screenPos.x > Screen.width - rotationBoarder * Screen.width)
        //    {
        //        player.transform.Rotate(new Vector3(0, rotationSpeed));
        //    }

        //    Vector2 currentPos = pointerUIElement.anchoredPosition;
        //    Vector2 smoothedPos = Vector2.Lerp(currentPos, screenPos, Time.deltaTime * speedFactor);

        //    //pointerUIElement.anchoredPosition = screenPoint;
        //    pointerUIElement.anchoredPosition = smoothedPos;
        //}

        // Debug: Draw a point where the IMU points on the screen
        //Debug.DrawLine(Camera.main.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 10)), Color.red);
        //Debug.Log($"Screen point: {screenPoint}");
    }


    IEnumerator UpdatePointerRoutine()
    {
        while (true)
        {
            UpdatePointerPosition();
            yield return null; // Run every frame
        }
    }

    void UpdatePointerPosition()
    {
        if (pointerUIElement != null)
        {
            Vector2 screenPoint = MapQuaternionToScreen(absoluteRotation);

            Vector2 screenPos = new Vector2();
            screenPos.x = Mathf.Clamp(screenPoint.x, 0, Screen.width);
            screenPos.y = Mathf.Clamp(screenPoint.y, 0, Screen.height);

            if (screenPos.x < rotationBoarder * Screen.width)
            {
                player.transform.Rotate(new Vector3(0, -rotationSpeed));
                leftArrow.enabled = true;
            }
            else if (screenPos.x > Screen.width - rotationBoarder * Screen.width)
            {
                player.transform.Rotate(new Vector3(0, rotationSpeed));
                rightArrow.enabled = true;
            }

            Vector2 currentPos = pointerUIElement.anchoredPosition;
            Vector2 smoothedPos = Vector2.Lerp(currentPos, screenPos, Time.deltaTime * speedFactor);

            //pointerUIElement.anchoredPosition = screenPoint;
            pointerUIElement.anchoredPosition = smoothedPos;
        }
    }


    Vector2 MapQuaternionToScreen(Quaternion rotation)
    {
        ////Vector3 direction = rotation * Vector3.forward;
        //Vector3 screenPoint = Camera.main.WorldToScreenPoint(direction);

        Vector3 localDirection = Camera.main.transform.TransformDirection(rotation * Vector3.forward); // to solve the ofset created by the camera

        // Map to screen space
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(Camera.main.transform.position + localDirection);

        // Map the screen point to the UI space (if the UI element is inside the canvas)
        Vector2 uiPoint = new Vector2(screenPoint.x, screenPoint.y);

        // Clamp to screen boundaries
        uiPoint.x = Mathf.Clamp(uiPoint.x, 0, screenWidth);
        uiPoint.y = Mathf.Clamp(uiPoint.y, 0, screenHeight);

        return uiPoint;
    }

    void ReceiveIMUData(Quaternion rotation)
    {
        absoluteRotation = rotation;
    }

    void OnDestroy()
    {
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= ReceiveIMUData;
        }
    }
}