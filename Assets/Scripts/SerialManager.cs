using System.IO.Ports;
using System.Threading;
using UnityEngine;
using System;

public class SerialManager : MonoBehaviour
{
    private SerialPort serialPort;
    private Thread serialThread;
    private bool isRunning = true;

    public string portName = "COM17"; // change depending on your system!!!
    private int baudRate = 115200;

    private static SerialManager instance;

    // Buffers for thread-safe communication
    private string imuData;
    private string triggerData;
    private string rfidData;

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
    public event Action<string> OnDataReceivedIMU;
    public event Action<string> OnDataReceivedTrigger;
    public event Action<string> OnDataReceivedRFID;

    void Awake()
    {
        instance = this;
        OpenSerialPortUART();
        // Start the serial reading thread
        serialThread = new Thread(ReadSerialPort);
        serialThread.IsBackground = true; // Allow the thread to exit with the app
        serialThread.Start();
    }

    void Update()
    {
        // Check thread-safe buffers for new data and trigger corresponding events
        if (imuData != null)
        {
            OnDataReceivedIMU?.Invoke(imuData);
            imuData = null; // Clear the buffer
        }

        if (triggerData != null)
        {
            OnDataReceivedTrigger?.Invoke(triggerData);
            triggerData = null; // Clear the buffer
        }

        if (rfidData != null)
        {
            OnDataReceivedRFID?.Invoke(rfidData);
            rfidData = null; // Clear the buffer
        }
    }

    //use this function if you use the UART connection
    void OpenSerialPortUART()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();

            if (serialPort.IsOpen)
            {
                Debug.Log("Serial port opened successfully on " + portName);
            }
            else
            {
                Debug.LogError("Failed to open serial port.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error opening serial port: " + ex.Message);
        }
    }

    //use this function if you use a USB CDC connection
    /*void OpenSerialPortUSB()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.DtrEnable = true; // Ensure DTR is enabled for USB CDC
            serialPort.RtsEnable = true; // Ensure RTS is enabled for USB CDC
            serialPort.Open();

            // Add delay for initialization
            System.Threading.Thread.Sleep(1000);

            if (serialPort.IsOpen)
            {
                Debug.Log("USBSerial port opened successfully on " + portName);
            }
            else
            {
                Debug.LogError("Failed to open USBSerial port.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error opening USBSerial port: " + ex.Message);
        }
    }*/


    void ReadSerialPort()
    {
        while (isRunning)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    string data = serialPort.ReadLine(); // Blocking call
                    ParseAndStoreData(data); // Process and store data in thread-safe buffers
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

    private void ParseAndStoreData(string data)
    {
        string[] values = data.Split('/'); // Assuming format: "r/0.1/0.2/0.3/0.4"
        //Debug.Log("Data received: " + data);
        lock (this) // Ensure thread safety
        {
            if (values[0] == "r" && values.Length == 5)
            {
                imuData = data; // Store IMU data
            }
            else if (values[0] == "tr" && values.Length == 2) // Format: "tr/1"
            {
                triggerData = values[1]; // Store Trigger data
            }
            else if (values[0] == "mg" && values.Length == 4) // Format: "mg/G1/M1/10"
            {
                rfidData = data; // Store RFID data
            }
        }
    }

    public void SendDataToESP32(string message)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.WriteLine(message);
                Debug.Log("Sent to ESP32: " + message);
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

    void OnApplicationQuit()
    {
        isRunning = false; // Stop the thread

        if (serialThread != null && serialThread.IsAlive)
        {
            serialThread.Join(); // Wait for the thread to exit
        }

        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                serialPort.Close();
                Debug.Log("Serial port closed.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error closing serial port: " + ex.Message);
            }
        }
    }
}
