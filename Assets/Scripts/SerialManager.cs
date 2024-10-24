using System.IO.Ports;
using UnityEngine;

public class SerialManager : MonoBehaviour
{
    private SerialPort serialPort;
    public string portName = "COM9"; 
    public int baudRate = 9600;

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

    void Awake()
    {
        instance = this;
        OpenSerialPort();
    }

    void OpenSerialPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();

            if (serialPort.IsOpen)
            {
                Debug.Log("Serial port opened successfully.");
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
