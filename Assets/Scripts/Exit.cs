using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public bool exit = false;
    public bool hit()
    {
        if(Score.score >= 100)
        {
            exit = true;
            return true;
        }
        return false;
    }
}
