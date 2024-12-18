using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireResistantZombie : Target
{
    // Override the Hit method to make it resistant to normal bullets
    public override void Hit(float damage)
    {
        Debug.Log("Zombie 5 is resistant to normal bullets!");
    }

    public override void fireHit(float damage)
    {
        base.fireHit(damage);
        Debug.Log("Zombie 5 takes fire damage!");
    }
}

