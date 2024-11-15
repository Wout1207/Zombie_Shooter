using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    //public static Action OnTargetHit;
    private GameObject player;
    private NavMeshAgent agent;
    public int hp;
    private bool playerInCollider = false;
    private float hitTimerDelay;
    // Start is called before the first frame update
    void Start()
    {
        hitTimerDelay = Time.time;
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((transform.position-player.transform.position).magnitude < 30)
        {
            agent.SetDestination(player.transform.position);
        }
        if (playerInCollider && Time.time-hitTimerDelay > 3)
        {
            Debug.Log(Time.time);
            player.GetComponent<Player>().TakeDamage(10);
            hitTimerDelay = Time.time;
        }
    }
    public void Hit()
    {
        hp -= 10;
        if(hp <= 0)
        {
            Destroy(this.gameObject);
        }
        //RandomizePosition();
        //OnTargetHit?.Invoke();
    }

    void RandomizePosition()
    {
        transform.position = TargetBounds.Instance.GetRandomPosition();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("in the collider");
        if (other.gameObject == player)
        {
            playerInCollider = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            playerInCollider = false;
        }
    }
}
