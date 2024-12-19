using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//public GameObject damageText;

public class FireResistantZombie : Target
{
    // Override the Hit method to make it resistant to normal bullets
    public override void Hit(float damage)
    {
        base.Hit(damage);
        /*Debug.Log("FireResistantZombie Hit method called.");
        Debug.Log("Zombie 5 is resistant to normal bullets!");*/
    }

    public override void fireHit(float damage)
    {
        GameObject text = Instantiate(damageText, transform);
        text.transform.LookAt(player.transform);
        text.transform.position += new Vector3(0, 4, 0);
        text.transform.Rotate(0, 180, 0);
        text.GetComponent<TMP_Text>().text = "IMMUNE";
        //Debug.Log("Zombie 5 takes fire damage!");
    }
}

