using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetThrowing : Target
{
    public GameObject coinPrefab;
    // Update is called once per frame
    protected new void Update()
    {
        base.Update();
        if (agent.isStopped && agent.hasPath)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
        }
    }
    public new void OnAttackAnimationEnd()
    {
        Debug.Log("Animation event triggered: Animation ended.");
        if (castRayToPlayer())
        {
            throwItem();
        }
        audioSource.clip = hitPlayer;
        audioSource.Play();
    }

    private bool castRayToPlayer()
    {
        Vector3 directionToPlayer = player.transform.position - transform.position + transform.forward;
        Ray ray = new Ray(transform.position, directionToPlayer);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Player playerHit = hit.collider.gameObject.GetComponent<Player>();
            if (playerHit != null)
            {
                return true;
            }
        }
        return false;
    }


    private void throwItem()
    {
        GameObject coin = Instantiate(coinPrefab, transform);
        coin.transform.position += new Vector3(transform.forward.x, 2, transform.forward.z);
        coin.transform.SetParent(transform.parent);
        Coin coinScript = coin.GetComponent<Coin>();
        coinScript.damage = damage;
        coinScript.thrownBy = gameObject.name;
        coinScript.rb.velocity = (-transform.position + player.transform.position).normalized*50;
        hitTimerDelay = Time.time;
    }
}
