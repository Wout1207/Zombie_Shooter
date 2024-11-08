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

    public event System.Action<int> onOpenDoor;
    public void openDoor(int id)
    {
        if (onOpenDoor != null)
        {
            onOpenDoor(id);
        }
    }

    public event System.Action<int> onCloseDoor;
    public void closeDoor(int id)
    {
        if (onCloseDoor != null)
        {
            onCloseDoor(id);
        }
    }
}
