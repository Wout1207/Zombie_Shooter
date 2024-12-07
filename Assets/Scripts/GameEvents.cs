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
            onToggleCloseDoorText(id);
            onToggleOpenDoorText(id);
            onOpenDoor(id);
        }
    }

    public event System.Action<int> onCloseDoor;
    public void closeDoor(int id)
    {
        if (onCloseDoor != null)
        {
            onToggleCloseDoorText(id);
            onToggleOpenDoorText(id);
            onCloseDoor(id);
        }
    }

    public event System.Action<int> onToggleOpenDoorText;
    public void toggleOpenDoorText(int id)
    {
        if (onToggleOpenDoorText != null)
        {
            onToggleOpenDoorText(id);
        }
    }

    public event System.Action<int> onToggleCloseDoorText;
    public void toggleCloseDoorText(int id)
    {
        if (onToggleCloseDoorText != null)
        {
            onToggleCloseDoorText(id);
        }
    }

    public event System.Action onPlayerDead;
    public void playerDead()
    {
        if (onPlayerDead != null)
        {
            onPlayerDead();
        }
    }

    public event System.Action onGunJammed;
    public void GunJammed()
    {
        if (onGunJammed != null)
        {
            onGunJammed();
        }
    }

    public event System.Action onGunDejammed;
    public void GunDejammed()
    {
        if (onGunDejammed != null)
        {
            onGunDejammed();
        }
    }

    public event System.Action<string[]> onOutofAmmo;
    public void OutofAmmo(string[] values)
    {
        if (onOutofAmmo != null)
        {
            onOutofAmmo(values);
        }
    }
}
