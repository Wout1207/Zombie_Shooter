using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Globalization;

public class SerialManager : MonoBehaviour
{
    public float speedFactor = 15.0f; // Speed factor for rotation interpolation

    private SerialPort serialPort;
    private Thread serialThread;
    private bool isRunning = true;
    private ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>(); // Thread-safe queue for serial data
    private ConcurrentQueue<Quaternion> rotationQueue = new ConcurrentQueue<Quaternion>(); // Thread-safe queue for rotations

    [Header("Serial Port Settings")]
    public string portName = "COM16"; // Change depending on your system
    public int baudRate = 1000000;

    public bool firstTime = true;
    private Quaternion firstPos;

    private static SerialManager instance;

    private string currentSceneName;

    public static SerialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SerialManager>();
            }
            return instance;
        }
    }

    // Events to broadcast received data
    public event Action<Quaternion> OnDataReceivedIMU;
    public event Action OnDataReceivedTrigger;
    public event Action<string[]> OnDataReceivedRFID;
    public event Action<string[]> OnDataReceivedMovement;
    public event Action OnDataReceivedGrenade;

    // Dictionary to map data type prefixes to handlers
    private Dictionary<string, Action<string[]>> dataHandlers;
    private static readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            OpenSerialPort();
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        // Initialize the data handlers
        dataHandlers = new Dictionary<string, Action<string[]>>
        {
            { "r", HandleIMUData },
            { "tr", HandleTriggerData },
            { "mg", HandleRFIDData },
            { "m", HandleMovementData },
            { "g", HandleGrenadeData },
            { "i", HandlePointerReset }
        };

        Debug.Log("SerialManager Awake");

        serialThread = new Thread(ReadSerialPort);
        serialThread.IsBackground = true; // Allow the thread to exit with the app
        serialThread.Start();
    }

    void Update()
    {
        // Check if there is a new rotation in the queue
        if (rotationQueue.TryDequeue(out Quaternion rotation))
        {
            //Debug.Log("Rotation: " + rotation);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speedFactor);
            OnDataReceivedIMU?.Invoke(rotation);
        }

        if (mainThreadActions.TryDequeue(out var action))
        {
            action.Invoke();
        }

        //Debug.Log($"Rotation queue count: {rotationQueue.Count}");
        int tresholdToClear = 5;
        if (rotationQueue.Count > tresholdToClear)
        {
            Debug.Log($"rotationQueue count: {tresholdToClear} --> Clearing rotation queue");
            rotationQueue.Clear();
        }
    }

    public static void EnqueueToMainThread(Action action)
    {
        if (action != null)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    private void OpenSerialPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();

            Debug.Log($"Serial port opened successfully on {portName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error opening serial port: " + ex.Message);
        }
    }

    void ReadSerialPort()
    {
        while (isRunning)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    string data = serialPort.ReadLine(); // Blocking call
                    ParseAndStoreData(data);
                }
                catch (TimeoutException)
                {
                    // Ignore timeout errors, keep reading
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error reading serial port: " + ex.Message);
                }
            }
        }
    }

    public void ParseAndStoreData(string data)
    {
        if (string.IsNullOrWhiteSpace(data)) return;

        string[] values = data.Split('/');
        if (values.Length == 0) return;

        string dataType = values[0];
        Debug.Log("Current data type: " + dataType);
        if (dataHandlers.TryGetValue(dataType, out Action<string[]> handler))
        {
            try
            {
                handler.Invoke(values); // Call the appropriate handler
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing data of type '{dataType}': {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Unknown data type received: {dataType}");
        }
    }

    private void HandleIMUData(string[] values)
    {
        //if (values[0] == "r" && values.Length == 5)
        if (values.Length == 5)
        {
            if (float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float w) &&
                float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))

                /*if (float.TryParse(values[1], out float w) &&
                float.TryParse(values[2], out float x) &&
                float.TryParse(values[3], out float y) &&
                float.TryParse(values[4], out float z))*/

            //if (float.TryParse(values[1], out float w) &&
            //    float.TryParse(values[2], out float x) &&
            //    float.TryParse(values[3], out float y) &&

            {

                //Quaternion rotation = new Quaternion(z, x, -y, w); //ESP32 bread board
                Quaternion rotation = new Quaternion(y, x, -z, w); //ESP32 actual gun front
                //Quaternion rotation = new Quaternion(x, -z, y, w); //ESP32 actual gun back
                if (firstTime)
                {
                    firstTime = false;
                    firstPos = rotation;
                }

                // Calculate absolute rotation relative to the initial offset
                rotation = Quaternion.Inverse(firstPos) * rotation;

                rotationQueue.Enqueue(rotation); // Enqueue rotation for Update
            }
        }
    }

    private void HandleTriggerData(String[] values)
    {
        if (values[1] == "0") //is inverted 
        {
            SerialManager.EnqueueToMainThread(() =>
            {
                currentSceneName = SceneManager.GetActiveScene().name;
                if (currentSceneName == "MenuScene" || currentSceneName == "GameOverScene")
                {
                    // Find the StartManager and call StartGame
                    StartManager startManager = FindObjectOfType<StartManager>();
                    if (startManager != null)
                    {
                        startManager.StartGame();
                    }
                }

                OnDataReceivedTrigger?.Invoke();
            });
        }
    }

    private void HandleRFIDData(string[] values)
    {
        if (values.Length == 4)
        {
            SerialManager.EnqueueToMainThread(() => {
                OnDataReceivedRFID?.Invoke(values);
            });
        }
    }

    private void HandleMovementData(string[] values)
    {
        if (values.Length == 3)
        {
            SerialManager.EnqueueToMainThread(() => {
                OnDataReceivedMovement?.Invoke(values);
            });
        }
    }

    private void HandleGrenadeData(string[] values)
    {
        if (values.Length == 1)
        {
            SerialManager.EnqueueToMainThread(() => {
                OnDataReceivedGrenade?.Invoke();
            });
        }
    }

    private void HandlePointerReset(string[] values)
    {
        if (values.Length == 1)
        {
            SerialManager.EnqueueToMainThread(() =>
            {
                firstTime = true;
            });
        }
    }

    public void SendDataToESP32(string message)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.WriteLine(message);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error writing to serial port: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Serial port is closed or not initialized.");
        }
    }

    private void CloseSerialPort()
    {
        if (serialPort?.IsOpen == true)
        {
            try
            {
                serialPort.Close();
                Debug.Log("Serial port closed.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error closing serial port: " + ex.Message);
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false; // Stop the thread

        if (serialThread != null && serialThread.IsAlive)
        {
            serialThread.Join(); // Wait for the thread to exit
        }

        CloseSerialPort();
    }

    public void SetFirstPos(Quaternion newFirstPos)
    {
        firstPos = newFirstPos;
        firstTime = false; // Prevent reinitializing
    }

    public Quaternion GetLastRotation()
    {
        if (rotationQueue.Count > 0)
        {
            Quaternion lastRotation = Quaternion.identity;

            // Iterate through the queue to get the last element
            foreach (Quaternion rotation in rotationQueue)
            {
                lastRotation = rotation;
            }

            return lastRotation;
        }
        else
        {
            Debug.LogWarning("No rotation data available in the queue.");
            return Quaternion.identity; // Return a default value if the queue is empty
        }
    }
}
