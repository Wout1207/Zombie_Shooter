using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public float damage;
    public Rigidbody rb;
    public string thrownBy;
    // Start is called before the first frame update

    private void Update()
    {
        if(rb.velocity.magnitude<0.01f || transform.position.y < -20)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        if (player)
        {
            player.TakeDamage(damage);
            Destroy(this.gameObject);
        }
        Target target = other.gameObject.GetComponent<Target>();
        if(target)
        {
            if (!other.gameObject.name.Equals(thrownBy))
            {
                target.Hit(damage);
                Destroy(this.gameObject);
            }
        }
    }
}
