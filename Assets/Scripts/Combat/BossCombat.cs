using UnityEngine;
using UnityEngine.AI;

public class BossCombat : MonoBehaviour
{
    public Transform player;
    public float attackRange = 3f;
    public float attackCooldown = 2f;

    private NavMeshAgent agent;
    private Animator anim;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            agent.SetDestination(player.position);
            anim.SetBool("Run", true);
        }
        else
        {
            agent.ResetPath();
            anim.SetBool("Run", false);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                anim.SetTrigger("Attack");
                lastAttackTime = Time.time;
            }
        }
    }
}