using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCollider : MonoBehaviour
{
    public DoorController DoorController;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (DoorController)
        {
            GameEvents.current.openDoor(DoorController.id);
        }
    }
}
