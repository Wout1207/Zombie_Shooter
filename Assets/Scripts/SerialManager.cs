using System.IO.Ports;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;
using System;

public class SerialManager : MonoBehaviour
{
    private SerialPort serialPort;

    public string portName = "COM10"; // change depending on your system!!!
    private int baudRate = 115200;
    //private Queue<string> receivedDataQueue = new Queue<string>(); // to store received data

    
    //public delegate void DataReceivedEventHandler(string data);

    private static SerialManager instance;

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

    //make the data recieved available to other scripts to subscribe to
    public event Action<string> OnDataReceived;
    public void Datarecieved(string data)
    {
        if (OnDataReceived != null)
        {
            OnDataReceived?.Invoke(data);
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

                string[] values = data.Split('/');
                if (values.Length == 5 && values[0] == "r")
                {
                    Debug.Log("IMU data received and trigger called");
                    SerialManager.Instance.Datarecieved(data); // Trigger event for IMU data
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
