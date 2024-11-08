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
    private bool open = false;
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.current.onOpenDoor += onOpenDoor;
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

    private void OnDestroy()
    {
        GameEvents.current.onOpenDoor -= onOpenDoor;
    }
}
