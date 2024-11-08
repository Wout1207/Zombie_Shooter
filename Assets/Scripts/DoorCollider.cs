using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCollider : MonoBehaviour
{
    public DoorController DoorController;

    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (DoorController.open)
        {
            GameEvents.current.toggleCloseDoorText(DoorController.id);
        }
        else
        {
            GameEvents.current.toggleOpenDoorText(DoorController.id);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (DoorController.open)
        {
            GameEvents.current.toggleCloseDoorText(DoorController.id);
        }
        else
        {
            GameEvents.current.toggleOpenDoorText(DoorController.id);
        }
    }
}
