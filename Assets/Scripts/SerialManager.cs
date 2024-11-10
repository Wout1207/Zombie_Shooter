using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;
using System;

public class SerialManager : MonoBehaviour
{
    private SerialPort serialPort;

    public string portName = "COM12"; // change depending on your system!!!
    private int baudRate = 115200;
    //private Queue<string> receivedDataQueue = new Queue<string>(); // to store received data

    
    //public delegate void DataReceivedEventHandler(string data);

    private static SerialManager instance;

    public static SerialManager Instance //TODO move to GameEvents script
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

    //make the IMU data recieved available to other scripts to subscribe to
    public event Action<string> OnDataReceivedIMU;
    public void DatarecievedIMU(string data) //#TODO move to GameEvents script
    {
        if (OnDataReceivedIMU != null)
        {
            OnDataReceivedIMU?.Invoke(data);
        }
    }

    //make the Trigger data recieved available to other scripts to subscribe to
    public event Action<string> OnDataReceivedTrigger; //#TODO  move to GameEvents script
    public void DatarecievedTrigger(string data)
    {
        if (OnDataReceivedTrigger != null)
        {
            OnDataReceivedTrigger?.Invoke(data);
        }
    }

    //make the RFID data recieved available to other scripts to subscribe to
    public event Action<string> OnDataReceivedRFID; //#TODO  move to GameEvents script
    public void DatarecievedRFID(string data)
    {
        if (OnDataReceivedRFID != null)
        {
            OnDataReceivedRFID?.Invoke(data);
        }
    }

    void Awake()
    {
        instance = this;
        OpenSerialPort();
    }

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen && serialPort.BytesToRead > 0)
        {
            try
            {
                string data = serialPort.ReadLine();
                //Debug.Log("Data received: " + data);

                string[] values = data.Split('/'); // format : "r/0.1/0.2/0.3/0.4"
                if (values[0] == "r" && values.Length == 5)
                {
                    //Debug.Log("IMU data received");
                    SerialManager.Instance.DatarecievedIMU(data); // Trigger event for IMU data
                }else if (values[0] == "tr" && values.Length == 2) // format : "tr/1"
                {
                    //Debug.Log("Trigger data received");
                    SerialManager.Instance.DatarecievedTrigger(values[1]); // Trigger event for Trigger data
                }else if (values[0] == "mg" && values.Length == 4) // format : "mg/G1/M1/10"
                {
                    //Debug.Log("RFID data received");
                    SerialManager.Instance.DatarecievedRFID(data); // Trigger event for RFID data
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error reading from serial port in Update: " + ex.Message);
            }
        }
    }

    void OpenSerialPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();


            if (serialPort.IsOpen)
            {
                //Debug.Log("Serial port opened successfully.");
                Debug.Log("Serial port opened successfully on " + portName);
            }
            else
            {
                Debug.LogError("Failed to open serial port.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error opening serial port: " + ex.Message);
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
            catch (System.Exception ex)
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
        if (serialPort != null && serialPort.IsOpen)
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
}
