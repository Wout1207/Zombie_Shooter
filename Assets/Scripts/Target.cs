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
    static protected private float distanceToPlayerThreshold = 30;
    public float distanceToAttackThreshold;
    protected private bool firstWithinRange = true;
    protected private bool playerDeadInvoked = false;

    public AudioSource audioSource;
    public AudioClip hitPlayer;
    public List<AudioClip> groans;

    public int score;

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
        if(!audioSource.isPlaying && Vector3.Distance(transform.position, player.transform.position) < distanceToPlayerThreshold)
        {
            audioSource.clip = groans[Random.Range(0, groans.Count - 1)];
            audioSource.Play();
        }
    }

    private void targetAnimations()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position); // wout.c : think better way to calculate distance?

        //if ((transform.position-player.transform.position).magnitude < 30) // wout.c : changed value to variable "distanceToPlayerThreshold"
        if (player.GetComponent<Player>().currentHP <= 0)
        {
            animator.SetTrigger("player_died");
        }

        else if (distanceToPlayer < distanceToPlayerThreshold)
        {
            if ((transform.position - player.transform.position).magnitude < 30)
            {
                agent.SetDestination(player.transform.position);
            }
            agent.isStopped = distanceToPlayer <= distanceToAttackThreshold || agent.pathStatus == NavMeshPathStatus.PathPartial || !agent.hasPath && player.GetComponent<Player>().currentHP > 0;
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
            else if (distanceToPlayer < distanceToAttackThreshold)
            {
                animator.SetBool("zombie_isWalking", false);
            }
            else if (!agent.isStopped)
            {
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
        hp -= damage;
        if (hp <= 0)
        {
            animator.SetTrigger("zombie_death");
            animator.SetBool("zombie_isDead", true);
            //Destroy(this.gameObject);
        }
        else
        {
            animator.SetTrigger("zombie_hit");
        }
        //RandomizePosition();
        //OnTargetHit?.Invoke();
    }

    public void fireHit(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            animator.SetBool("zombie_isDead", true);
            animator.SetTrigger("zombie_death");
            
            //Destroy(this.gameObject);
        }
    }

    public void destroyTarget()
    {
        Destroy(gameObject);
        Score.score += score;
    }
    public void OnAttackAnimationEnd()
    {
        audioSource.clip = hitPlayer;
        audioSource.Play();
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
        if (agent != null)
        {
            agent.isStopped = true;
        }
        if (!animator)
        {
            return;
        }
        animator.SetBool("zombie_isWalking", false);
        animator.SetBool("player_died", true);
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
