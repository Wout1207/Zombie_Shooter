using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    //public static Action OnTargetHit;
    protected private GameObject player;
    protected private NavMeshAgent agent;
    public float hp;
    public float damage;
    protected private bool playerInCollider = false;
    protected private float hitTimerDelay;
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
    protected void Update()
    {
        if ((transform.position-player.transform.position).magnitude < 30)
        {
            agent.SetDestination(player.transform.position);
        }
        agent.isStopped = (((transform.position - player.transform.position).magnitude) <= 2.5f) || agent.pathStatus == NavMeshPathStatus.PathPartial || !agent.hasPath;
        if (playerInCollider && Time.time-hitTimerDelay > 3)
        {
            player.GetComponent<Player>().TakeDamage(damage);
            hitTimerDelay = Time.time;
        }
    }
    public void Hit(float damage)
    {
        hp -= damage;
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
