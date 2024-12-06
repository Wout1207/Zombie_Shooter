using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationManager : MonoBehaviour
{
    [SerializeField]
    private SerialManager serialManager; // Reference to SerialManager

    public void CalibrateIMU()
    {
        if (serialManager != null)
        {
            // Get the most recent rotation from the queue
            Quaternion currentRotation = serialManager.GetLastRotation();

            // Set the new calibration point (adjust firstPos)
            serialManager.SetFirstPos(currentRotation);

            Debug.Log("IMU calibrated. New firstPos set to: " + currentRotation);
        }
        else
        {
            Debug.LogError("SerialManager reference is not set in CalibrationManager!");
        }
    }
}
