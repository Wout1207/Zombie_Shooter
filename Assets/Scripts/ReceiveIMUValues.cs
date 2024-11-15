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

    private string dataBuffer; // Buffer to hold the latest data
    private bool isProcessing = false; // Flag to check if the data is being processed

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
        QueueIMUData(data);
        if (!isProcessing && !string.IsNullOrEmpty(dataBuffer))
        {
            ProcessIMUData(dataBuffer);
            dataBuffer = null; // Clear the buffer after processing
            //Debug.Log("IMU read");
        }
    }

    public void QueueIMUData(string data)
    {
        if (isProcessing)
        {
            //Debug.Log("Data is still being processed!");
            return; // Skip new data if the current data is still being processed
        }
        //Debug.Log("Data queued");
        dataBuffer = data; // Store the latest data in the buffer
    }
    private void ProcessIMUData(string data)
    //public void ReadIMU(string data)
    {
        //Debug.Log("Processing IMU data: " + data);
        isProcessing = true; // Set the flag to indicate processing
        string[] values = data.Split('/');
        if (values.Length == 5 && values[0] == "r") // Rotation of the first one 
        {
            float w = float.Parse(values[1]);
            float x = float.Parse(values[2]);
            float y = float.Parse(values[3]);
            float z = float.Parse(values[4]);

            if (firstTime)
            {
                firstTime = false;
                firstPos = new Quaternion(y, -z, x, w);
                //Quaternion A = new Quaternion(y, -z, x, w);
                isProcessing = false; // Reset the flag after processing is complete
                return;
            }
            else
            {   
                //A * B * iB = C
                //Quaternion A = new Quaternion(-y, -z, x, w);
                //Quaternion A = new Quaternion(x, -z, -y, w); // The one from Uduino tests
                Quaternion A = new Quaternion(y, -z, x, w);
                Quaternion B = firstPos;
                Quaternion iB = Quaternion.Inverse(firstPos);
                Quaternion C = A * iB;

                //this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, new Quaternion(w, y, x, z), Time.deltaTime * speedFactor);
                //this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, new Quaternion(-y, -z, x, w), Time.deltaTime * speedFactor);
                this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, C, Time.deltaTime * speedFactor);
            }
        }
        else if (values.Length != 5)
        {
           Debug.LogWarning(data);
        }
        this.transform.parent.transform.eulerAngles = rotationOffset;

        isProcessing = false; // Reset the flag after processing is complete
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
