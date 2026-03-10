using UnityEngine;
using UnityEngine.AI;

public class BossBrain : MonoBehaviour
{
    public Transform player;

    public float detectRange = 20f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;
    public float pathUpdateRate = 0.2f;

    NavMeshAgent agent;
    public Animator anim;

    float lastAttackTime;
    float pathUpdateTimer;

    enum BossState { Idle, Chase, Attack }
    BossState state;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Tự động thiết lập khoảng cách phanh lại để tránh Boss ủi vào Player gây giật cục
        agent.stoppingDistance = attackRange - 0.5f;

        agent.updateRotation = false; // Tắt xoay tự động để tự xoay bằng code cho mượt

        state = BossState.Idle;
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        switch (state)
        {
            case BossState.Idle:
                anim.SetBool("isMoving", false);
                if (dist < detectRange)
                {
                    state = BossState.Chase;
                }
                break;

            case BossState.Chase:
                // SỬA LỖI GIẬT CỤC: Cứ vào tầm là chuyển sang Attack luôn để phanh lại (đứng chờ hồi chiêu)
                if (dist <= attackRange)
                {
                    state = BossState.Attack;
                    break;
                }

                agent.isStopped = false;
                anim.SetBool("isMoving", true);

                if (Time.time > pathUpdateTimer)
                {
                    agent.SetDestination(player.position);
                    pathUpdateTimer = Time.time + pathUpdateRate;
                }
                break;

            case BossState.Attack:
                agent.isStopped = true;
                anim.SetBool("isMoving", false);

                // Luôn nhìn chằm chằm vào Thạch Sanh
                FaceTarget(player.position);

                // Nếu hết hồi chiêu thì tung đòn
                if (Time.time > lastAttackTime + attackCooldown)
                {
                    ChooseAttack();
                    lastAttackTime = Time.time;
                }

                // Nếu Player lùi ra xa thì rượt tiếp
                if (dist > attackRange)
                {
                    state = BossState.Chase;
                }
                break;
        }

        RotateSmooth();

    }

    void RotateSmooth()
    {
        if (state == BossState.Chase && agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5 * Time.deltaTime);
        }
    }

    void ChooseAttack()
    {
        int attack = Random.Range(0, 2);

        if (attack == 0) anim.SetTrigger("Attack");
        else anim.SetTrigger("Skill");
    }
}