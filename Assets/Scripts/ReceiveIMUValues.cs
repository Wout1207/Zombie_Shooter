using UnityEngine;
using System.Globalization;
//using Uduino;

public class ReceiveIMUValues : MonoBehaviour
{

    Vector3 position;
    Vector3 rotation;
    public Vector3 rotationOffset;
    public float speedFactor = 15.0f;

    private bool firstTime = true;
    private Quaternion firstPos;

    void Start()
    {
        //UduinoManager.Instance.OnDataReceived += ReadIMU;
        //  Note that here, we don't use the delegate but the Events, assigned in the Inpsector Panel
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU += ReadIMU    ;
        }
    }

    void Update() { }

    public void ReadIMU(string data)
    {
        string[] values = data.Split('/');
        if (values.Length == 5 && values[0] == "r") // Valid IMU data
        {
            if (!float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float w) ||
                !float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
                !float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) ||
                !float.TryParse(values[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            {
                Debug.LogWarning($"Invalid quaternion values in data: {data}");
                return;
            }

            Quaternion currentIMURotation = new Quaternion(y, z, x, w); // Adjust axes as needed

            if (firstTime)
            {
                firstTime = false;
                firstPos = currentIMURotation; // Save the initial orientation
            }

            // Calculate absolute rotation relative to the initial offset
            Quaternion absoluteRotation = Quaternion.Inverse(firstPos) * currentIMURotation;

            // Smoothly apply the rotation
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, absoluteRotation, Time.deltaTime * speedFactor);
        }
        else if (values.Length != 5)
        {
            Debug.LogWarning($"Unexpected data format: {data}");
        }

        // Apply rotation offset to parent transform
        if (this.transform.parent != null)
        {
            this.transform.parent.transform.eulerAngles = rotationOffset;
        }
    }

    void OnDestroy()
    {
        //UduinoManager.Instance.OnDataReceived -= ReadIMU;
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU -= ReadIMU;
        }
    }
}
