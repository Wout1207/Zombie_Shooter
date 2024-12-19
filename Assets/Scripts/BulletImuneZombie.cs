using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BulletImuneZombie : Target
{
    public override void Hit(float damage)
    {
        GameObject text = Instantiate(damageText, transform);
        text.transform.LookAt(player.transform);
        text.transform.position += new Vector3(0, 4, 0);
        text.transform.Rotate(0, 180, 0);
        text.GetComponent<TMP_Text>().text = "IMMUNE";
        /*Debug.Log("FireResistantZombie Hit method called.");
        Debug.Log("Zombie 5 is resistant to normal bullets!");*/
    }

    public override void fireHit(float damage)
    {
        base.fireHit(damage);
        //Debug.Log("Zombie 5 takes fire damage!");
    }
}
