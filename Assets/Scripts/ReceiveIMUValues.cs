using System.Globalization;
using UnityEngine;
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
            SerialManager.Instance.OnDataReceivedIMU += ReadIMU;
        }
    }

    void Update() { }



    public void ReadIMU(string data)
    {
        Debug.Log(data);
        string[] values = data.Split('/');
        if (values.Length == 5 && values[0] == "r") // Rotation of the first one 
        {
            float w = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
            float x = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);
            float y = float.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat);
            float z = float.Parse(values[4], CultureInfo.InvariantCulture.NumberFormat);
            if (firstTime)
            {
                firstTime = false;
                //firstPos = new Quaternion(-y, -z, x, w);
                //firstPos = new Quaternion(x, -z, -y, w);
                firstPos = new Quaternion(y, x, z, w);
                return;
            }
            else
            {
                //A * B * iB = C
                //Quaternion A = new Quaternion(-y, -z, x, w);
                //Quaternion A = new Quaternion(x, -z, -y, w); // The one from Uduino tests
                //Quaternion A = new Quaternion(-x, -z, -y, w);
                Quaternion A = new Quaternion(y, x, z, w);
                Quaternion B = firstPos;
                Quaternion iB = Quaternion.Inverse(B);
                Quaternion C = A * iB;

                //this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, new Quaternion(w, y, x, z), Time.deltaTime * speedFactor);
                //this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, new Quaternion(-y, -z, x, w), Time.deltaTime * speedFactor);


                this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, C, Time.deltaTime * speedFactor);

            }
        }
        else if (values.Length != 5)
        {
           Debug.LogWarning(data);
        }
        this.transform.parent.transform.eulerAngles = rotationOffset;
        
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
