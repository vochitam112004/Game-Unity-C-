using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BossBrain : MonoBehaviour
{
    public Transform player;
    public PlayerHealth playerHealth;

    [Header("Chỉ số Di chuyển & Tầm nhìn")]
    public float detectRange = 20f;
    public float pathUpdateRate = 0.2f;

    [Header("Chỉ số Chiến đấu Phase 1 (Cận chiến)")]
    public float meleeAttackRange = 3f;
    public float attackDuration = 1.2f;
    public float attackCooldown = 2f;

    [Header("Chỉ số Phase 2 (Tầm xa)")]
    public float maxHealth = 1000f;
    public float currentHealth;
    public float phase2HealthThreshold = 300f;
    public float phase2HealAmount = 500f;
    public float rangedAttackRange = 15f;
    public float transformDuration = 3f;

    [Header("Giao diện UI Boss")]
    public Image bossHealthBarFill;

    [Header("Ảo thuật Biến Hình (Đổi Model)")]
    public GameObject phase1Model;
    public GameObject phase2Model;
    public Animator phase2Animator;
    public GameObject smokeVFX;

    [Header("Hiệu ứng & Sát thương (MỚI)")]
    public GameObject bloodVFX;
    public Transform hitPoint;
    public float attackDamage = 50f; // BIẾN SÁT THƯƠNG ĐÂY RỒI

    public bool isPhase2 = false;
    private float currentAttackRange;

    NavMeshAgent agent;
    public Animator anim;

    float lastAttackTime;
    float pathUpdateTimer;

    enum BossState { Idle, Chase, Attack, Transforming }
    BossState state;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        currentAttackRange = meleeAttackRange;
        agent.stoppingDistance = currentAttackRange - 0.5f;
        agent.updateRotation = false;
        state = BossState.Idle;
        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (state == BossState.Transforming || currentHealth <= 0) return;

        float dist = Vector3.Distance(player.position, transform.position);

        switch (state)
        {
            case BossState.Idle:
                anim.SetBool("isMoving", false);
                if (dist < detectRange) state = BossState.Chase;
                break;

            case BossState.Chase:
                if (dist <= currentAttackRange)
                {
                    state = BossState.Attack;
                    break;
                }

                agent.isStopped = false;
                anim.SetBool("isMoving", true);

                anim.ResetTrigger("Attack");
                anim.ResetTrigger("Skill");
                anim.ResetTrigger("Kick");
                anim.ResetTrigger("RangedAttack");

                if (Time.time > pathUpdateTimer)
                {
                    agent.SetDestination(player.position);
                    pathUpdateTimer = Time.time + pathUpdateRate;
                }
                break;

            case BossState.Attack:
                agent.isStopped = true;
                anim.SetBool("isMoving", false);

                FaceTarget(player.position);

                if (Time.time < lastAttackTime + attackDuration) break;

                if (Time.time < lastAttackTime + (attackDuration * 0.5f))
                {
                    transform.Translate(Vector3.forward * 2f * Time.deltaTime);
                }

                if (dist > currentAttackRange + 0.5f)
                {
                    state = BossState.Chase;
                }
                else if (Time.time > lastAttackTime + attackCooldown)
                {
                    ChooseAttack();
                    lastAttackTime = Time.time;
                }
                break;
        }

        RotateSmooth();
    }

    // --- ĐÃ FIX CẤU TRÚC LỆNH IF/ELSE ---
    public void TriggerHitPlayer()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        bool isCloseEnough = dist <= meleeAttackRange + 0.5f;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        bool isInFront = angle < 60f;

        if (isCloseEnough && isInFront)
        {
            if (bloodVFX != null && hitPoint != null)
            {
                GameObject blood = Instantiate(bloodVFX, hitPoint.position, Quaternion.LookRotation(transform.position - hitPoint.position));
                Destroy(blood, 2f);
            }

            if (playerHealth != null)
            {
                // Đã đổi thành attackDamage (tên biến đúng khai báo ở trên)
                playerHealth.TakeDamage(Mathf.RoundToInt(attackDamage));
            }

            Debug.Log("<color=red>BỤP! THẠCH SANH BỊ CHÉM TRÚNG TÓE MÁU!</color>");
        }
        else // Kéo chữ else ra ngoài cho đúng cấu trúc C#
        {
            Debug.Log("<color=yellow>NÉ ĐÒN THÀNH CÔNG! (Thạch Sanh đã chạy xa hoặc lách ra sau lưng)</color>");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0 || state == BossState.Transforming) return;

        currentHealth -= damageAmount;
        Debug.Log("Boss bị chém! Máu còn: " + currentHealth);

        if (bossHealthBarFill != null)
        {
            bossHealthBarFill.fillAmount = currentHealth / maxHealth;
        }

        bool isActivelyAttacking = (state == BossState.Attack) && (Time.time < lastAttackTime + attackDuration);

        if (!isActivelyAttacking)
        {
            anim.SetTrigger("Hit");
        }
        else
        {
            Debug.Log("<color=orange>Boss đang vung tay! Super Armor kích hoạt!</color>");
        }

        if (currentHealth <= phase2HealthThreshold && !isPhase2)
        {
            TriggerPhase2();
        }
        else if (currentHealth <= 0)
        {
            Die();
        }
    }

    void TriggerPhase2()
    {
        Debug.Log("BOSS NỔI ĐIÊN! VÀO PHASE 2!");
        isPhase2 = true;
        state = BossState.Transforming;

        agent.isStopped = true;
        anim.SetBool("isMoving", false);

        anim.ResetTrigger("Attack");
        anim.ResetTrigger("Skill");
        anim.ResetTrigger("Kick");
        anim.SetTrigger("Transform");

        currentHealth += phase2HealAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        currentAttackRange = rangedAttackRange;
        agent.stoppingDistance = currentAttackRange - 0.5f;

        Invoke("EndTransformation", transformDuration);
    }

    void EndTransformation()
    {
        if (currentHealth > 0) state = BossState.Chase;
    }

    void Die()
    {
        Debug.Log("BOSS ĐÃ CHẾT!");

        if (agent != null) agent.enabled = false;

        Collider bossCollider = GetComponent<Collider>();
        if (bossCollider != null) bossCollider.enabled = false;

        anim.SetTrigger("Die");
        this.enabled = false;
    }

    void ChooseAttack()
    {
        if (!isPhase2)
        {
            int attack = Random.Range(0, 3);
            if (attack == 0) anim.SetTrigger("Attack");
            else if (attack == 1) anim.SetTrigger("Skill");
            else anim.SetTrigger("Kick");
        }
        else
        {
            anim.SetTrigger("RangedAttack");
        }
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
}