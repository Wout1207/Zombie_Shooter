using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FireResistantZombie : Target
{
    // Override the Hit method to make it resistant to normal bullets
    public override void Hit(float damage)
    {
        ShowImmunityText("IMMUNE");
        /*Debug.Log("FireResistantZombie Hit method called.");
        Debug.Log("Zombie 5 is resistant to normal bullets!");*/
    }

    public override void fireHit(float damage)
    {
        base.fireHit(damage);
        //Debug.Log("Zombie 5 takes fire damage!");
    }

    private void ShowImmunityText(string message)
    {
        GameObject text = Instantiate(damageText, transform);
        text.transform.LookAt(player.transform);
        text.transform.position += new Vector3(0, 2, 0); 
        text.transform.Rotate(0, 180, 0);

        TMP_Text tmp = text.GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = message;
            tmp.color = Color.yellow;
        }
    }
}

