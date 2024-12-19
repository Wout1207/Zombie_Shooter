using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    //private CalibrationManager calibrationManager;
    [SerializeField]
    private SerialManager serialManager;
    public void StartGame()
    {
        //CalibrateIMU();
        SceneManager.LoadScene("CutScene");
        
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }



    //public void CalibrateIMU()
    //{
    //    if (SerialManager.instance != null)
    //    {
    //        // Get the most recent rotation from the queue
    //        Quaternion currentRotation = SerialManager.instance.GetLastRotation();

    //        // Set the new calibration point
    //        SerialManager.instance.SetFirstPos(currentRotation);

    //        Debug.Log("IMU calibrated. New firstPos set to: " + currentRotation);
    //    }
    //    else
    //    {
    //        Debug.LogError("Error in StartManager!");
    //    }
    //}
}
