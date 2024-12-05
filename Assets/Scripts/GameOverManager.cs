using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Method to restart the game
    public void RestartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
