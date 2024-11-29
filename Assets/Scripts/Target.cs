using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    protected private bool playerDeadInvoked = false;
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

        animator = GetComponent<Animator>();

        GameEvents.current.onPlayerDead += playerDied;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (!playerDeadInvoked)
        {
            targetAnimations();
        }
    }

    private void targetAnimations()
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
            //else if ((distanceToPlayer <= distanceToAttackThreshold || playerInCollider) && (Time.time - hitTimerDelay > 3))
            else if ((playerInCollider) && (Time.time - hitTimerDelay > 3))
            {
                agent.isStopped = true;
                animator.SetBool("zombie_isWalking", false);
                animator.SetTrigger("zombie_attack");
                //player.GetComponent<Player>().TakeDamage(damage); // moved to OnAttackAnimationEnd
                setPosAndDest();
                hitTimerDelay = Time.time;
            }
            else
            {
                //agent.SetDestination(player.transform.position);
                agent.isStopped = false;
                //agent.destination = player.transform.position; // wout.c : changed to destination instead of SetDestination (later is for error handling)
                agent.destination = player.transform.position;
                setPosAndDest(false, true);
                animator.SetBool("zombie_isWalking", true);
            }
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("zombie_isWalking", false);
            setPosAndDest(true, true);
        }
        //agent.isStopped = (((transform.position - player.transform.position).magnitude) <= 2.5f) || agent.pathStatus == NavMeshPathStatus.PathPartial || !agent.hasPath;
    }
    public void setPosAndDest(bool pos = true, bool rot = true)
    {
        animator.SetFloat("speed", agent.velocity.magnitude);

    }
    public void Hit(float damage)
    {
        animator.SetTrigger("zombie_hit");
        hp -= damage;
        if (hp <= 0)
        {
            animator.SetTrigger("zombie_death");
            //Destroy(this.gameObject);
        }
        //RandomizePosition();
        //OnTargetHit?.Invoke();
    }
    public void OnAttackAnimationEnd()
    {

        Debug.Log("Animation event triggered: Animation ended.");
        player.GetComponent<Player>().TakeDamage(damage);
        // Add your logic here, such as transitioning to the next state
    }

    private void TargetDestroy()
    {
        Destroy(this.gameObject);
    }

    private void playerDied()
    {
        playerDeadInvoked = true;
        print("Player died");
        agent.isStopped = true;
        animator.SetBool("zombie_isWalking", false);
        animator.SetTrigger("player_died");
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
