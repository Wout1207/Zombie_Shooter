using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour
{
    //public static Action OnTargetHit;
    protected private GameObject player;
    protected private NavMeshAgent agent;
    protected private Animator animator;
    public float hp;
    public float damage;
    protected private bool playerInCollider = false;
    protected private float hitTimerDelay;
    protected private float distanceToPlayer;
    static private float distanceToPlayerThreshold = 30;
    static private float distanceToAttackThreshold = 2.5f;
    protected private bool firstWithinRange = true;
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

        animator = GetComponent<Animator>();;
    }

    // Update is called once per frame
    protected void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position); // wout.c : think better way to calculate distance?

        //if ((transform.position-player.transform.position).magnitude < 30) // wout.c : changed value to variable "distanceToPlayerThreshold"

        if (distanceToPlayer < distanceToPlayerThreshold)
        {
            if (firstWithinRange)
            {
                agent.isStopped = true;
                animator.SetBool("zombie_isWalking", false);
                firstWithinRange = false;
                animator.SetTrigger("zombie_scream");
            }
            else if ((distanceToPlayer <= distanceToAttackThreshold || playerInCollider) && (Time.time - hitTimerDelay > 3))
            {
                agent.isStopped = true;
                animator.SetBool("zombie_isWalking", false);
                animator.SetTrigger("zombie_attack");
                player.GetComponent<Player>().TakeDamage(damage);
                hitTimerDelay = Time.time;
            }
            else
            {
                //agent.SetDestination(player.transform.position);
                agent.isStopped = false;
                agent.destination = player.transform.position; // wout.c : changed to destination instead of SetDestination (later is for error handling)
                animator.SetBool("zombie_isWalking", true);
            }
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("zombie_isWalking", false);
        }
        //agent.isStopped = (((transform.position - player.transform.position).magnitude) <= 2.5f) || agent.pathStatus == NavMeshPathStatus.PathPartial || !agent.hasPath;
        //if (playerInCollider && Time.time - hitTimerDelay > 3)
        //{
        //    player.GetComponent<Player>().TakeDamage(damage);
        //    hitTimerDelay = Time.time;
        //}
    }
    public void Hit(float damage)
    {
        animator.SetTrigger("zombie_hit");
        hp -= damage;
        if (hp <= 0)
        {
            animator.SetTrigger("zombie_death");
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
