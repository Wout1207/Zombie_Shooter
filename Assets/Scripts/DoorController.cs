using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public enum OpeningDirection
    {
        posX,
        posY,
        posZ,
        negX,
        negY,
        negZ,
        rotPosX,
        rotPosY,
        rotPosZ,
        rotNegX,
        rotNegY,
        rotNegZ
    }
    public OpeningDirection openingDirection;
    public int id;
    public bool open;
    public List<GameObject> openDoorText;
    public List<GameObject> closeDoorText;
    // Start is called before the first frame update
    void Start()
    {
        open = false;
        GameEvents.current.onOpenDoor += onOpenDoor;
        GameEvents.current.onCloseDoor += onCloseDoor;
        GameEvents.current.onToggleOpenDoorText += onToggleOpenDoorText;
        GameEvents.current.onToggleCloseDoorText += onToggleCloseDoorText;
    }

    private void onOpenDoor(int id)
    {
        if (this.id == id)
        {
            switch (openingDirection)
            {
                case OpeningDirection.posX:
                    if (!open)
                    {
                        transform.Translate(3, 0, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.posY:
                    if (!open)
                    {
                        transform.Translate(0, 3, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.posZ:
                    if (!open)
                    {
                        transform.Translate(0, 0, 3);
                        open = true;
                    }
                    break;
                case OpeningDirection.negX:
                    if (!open)
                    {
                        transform.Translate(-3, 0, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.negY:
                    if (!open)
                    {
                        transform.Translate(0, -3, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.negZ:
                    if (!open)
                    {
                        transform.Translate(0, 0, -3);
                        open = true;
                    }
                    break;
                case OpeningDirection.rotPosX:
                    if (!open)
                    {
                        transform.Rotate(90, 0, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.rotPosY:
                    if (!open)
                    {
                        transform.Rotate(0, 90, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.rotPosZ:
                    if (!open)
                    {
                        transform.Rotate(0, 0, 90);
                        open = true;
                    }
                    break;
                case OpeningDirection.rotNegX:
                    if (!open)
                    {
                        transform.Rotate(-90, 0, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.rotNegY:
                    if (!open)
                    {
                        transform.Rotate(0, -90, 0);
                        open = true;
                    }
                    break;
                case OpeningDirection.rotNegZ:
                    if (!open)
                    {
                        transform.Rotate(0, 0, -90);
                        open = true;
                    }
                    break;
                default:
                    Debug.LogError("Door has not selected an opening direction");
                    break;
            }
        }
    }

    private void onCloseDoor(int id)
    {
        if (this.id == id)
        {
            switch (openingDirection)
            {
                case OpeningDirection.posX:
                    if (open)
                    {
                        transform.Translate(-3, 0, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.posY:
                    if (open)
                    {
                        transform.Translate(0, -3, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.posZ:
                    if (open)
                    {
                        transform.Translate(0, 0, -3);
                        open = false;
                    }
                    break;
                case OpeningDirection.negX:
                    if (open)
                    {
                        transform.Translate(3, 0, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.negY:
                    if (open)
                    {
                        transform.Translate(0, 3, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.negZ:
                    if (open)
                    {
                        transform.Translate(0, 0, 3);
                        open = false;
                    }
                    break;
                case OpeningDirection.rotPosX:
                    if (open)
                    {
                        transform.Rotate(-90, 0, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.rotPosY:
                    if (open)
                    {
                        transform.Rotate(0, -90, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.rotPosZ:
                    if (open)
                    {
                        transform.Rotate(0, 0, -90);
                        open = false;
                    }
                    break;
                case OpeningDirection.rotNegX:
                    if (open)
                    {
                        transform.Rotate(90, 0, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.rotNegY:
                    if (open)
                    {
                        transform.Rotate(0, 90, 0);
                        open = false;
                    }
                    break;
                case OpeningDirection.rotNegZ:
                    if (open)
                    {
                        transform.Rotate(0, 0, 90);
                        open = false;
                    }
                    break;
                default:
                    Debug.LogError("Door has not selected an opening direction");
                    break;
            }
        }
    }

    private void onToggleOpenDoorText(int id)
    {
        if (this.id == id)
        {
            foreach (GameObject obj in openDoorText)
            {
                if (obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                }
                else
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    private void onToggleCloseDoorText(int id)
    {
        if (this.id == id)
        {
            foreach (GameObject obj in closeDoorText)
            {
                if (obj.activeInHierarchy)
                {
                    obj.SetActive(false);
                }
                else
                {
                    obj.SetActive(true);
                }
            }
        }
    }

    public bool hit()
    {
        for (int i = 0; i<openDoorText.Count; i++)
        {
            if (openDoorText[i].activeInHierarchy || closeDoorText[i].activeInHierarchy)
            {
                if (open)
                {
                    GameEvents.current.closeDoor(id);
                }
                else
                {
                    GameEvents.current.openDoor(id);
                }
                return true;
            }
        }
        
        return false;
    }

    private void OnDestroy()
    {
        GameEvents.current.onOpenDoor -= onOpenDoor;
        GameEvents.current.onCloseDoor -= onCloseDoor;
        GameEvents.current.onToggleOpenDoorText -= onToggleOpenDoorText;
        GameEvents.current.onToggleCloseDoorText -= onToggleCloseDoorText;
    }
}
