using UnityEngine;
using System.Globalization;

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
        if (SerialManager.Instance != null)
        {
            SerialManager.Instance.OnDataReceivedIMU += ReadIMU;
        }
    }

    void Update() { }

    public void ReadIMU(string data)
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

            if (firstTime)
            {
                firstTime = false;
                firstPos = currentIMURotation;
            }

            // Calculate absolute rotation relative to the initial offset
            Quaternion absoluteRotation = Quaternion.Inverse(firstPos) * currentIMURotation;
            //Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            //Quaternion finalRotation = absoluteRotation * offsetRotation;

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
            SerialManager.Instance.OnDataReceivedIMU -= ReadIMU;
        }
    }
}