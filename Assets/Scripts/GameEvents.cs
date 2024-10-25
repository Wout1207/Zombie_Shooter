using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }

    public event System.Action onShotFired;
    public void ShotFired()
    {
        if (onShotFired != null)
        {
            onShotFired();
        }
    }
}
