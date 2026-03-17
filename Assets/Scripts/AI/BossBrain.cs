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

    [Header("Hiệu ứng & Sát thương")]
    public GameObject bloodVFX;
    public Transform hitPoint;
    public float attackDamage = 50f;

    public GameObject healthBarCanvas; // Kéo cái BossHealthCanvas vào đây

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
                if (dist < detectRange) 
                {
                    state = BossState.Chase;
                    // --- GỌI SCRIPT BẮT ĐẦU CHỬI NHAU KHI PHÁT HIỆN THẠCH SANH ---
                    TalkToBoss talkScript = GetComponent<TalkToBoss>();
                    if (talkScript != null)
                    {
                        talkScript.StartBossFight();
                    }
                    // -----------------------------------------------------------
                }
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

    public void TriggerHitPlayer()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        bool isCloseEnough = dist <= currentAttackRange + 0.5f;

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
                playerHealth.TakeDamage(Mathf.RoundToInt(attackDamage));
            }

            Debug.Log("<color=red>BỤP! THẠCH SANH BỊ CHÉM TRÚNG TÓE MÁU!</color>");
        }
        else
        {
            Debug.Log("<color=yellow>NÉ ĐÒN THÀNH CÔNG! (Thạch Sanh đã chạy xa hoặc lách ra sau lưng)</color>");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (currentHealth <= 0 || state == BossState.Transforming) return;

        currentHealth -= damageAmount;
        Debug.Log("Boss bị chém! Máu còn: " + currentHealth);

        // --- GỌI SCRIPT CHỬI BỚI KHI BỊ CHÉM ---
        TalkToBoss talkScript = GetComponent<TalkToBoss>();
        if (talkScript != null)
        {
            talkScript.TakeHit();
        }
        // ----------------------------------------

        // --- CẬP NHẬT MỚI: TÓE MÁU KHI BOSS BỊ CHÉM TRÚNG ---
        if (bloodVFX != null)
        {
            // Tự động tính toán vị trí giữa bụng/ngực Boss (cao lên 1.5m) để xịt máu
            Vector3 bloodSpawnPos = transform.position + Vector3.up * 1.5f;
            GameObject bossBlood = Instantiate(bloodVFX, bloodSpawnPos, Quaternion.identity);
            Destroy(bossBlood, 2f); // Xóa vết máu sau 2 giây cho đỡ lag
        }
        // ----------------------------------------------------

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
        // 1. Tắt con Boss cũ
        if (phase1Model != null) phase1Model.SetActive(false);

        // 2. Bật con Boss mới (Mãng Xà/Sói...)
        if (phase2Model != null) phase2Model.SetActive(true);

        // 3. Đổi não (Chuyển quyền điều khiển hoạt ảnh sang Animator mới)
        if (phase2Animator != null) anim = phase2Animator;

        // 4. Tạo khói che mắt (Nếu có)
        if (smokeVFX != null)
        {
            GameObject smoke = Instantiate(smokeVFX, transform.position, Quaternion.identity);
            Destroy(smoke, 3f); // Xóa khói sau 3 giây
        }

        // 5. Kết thúc biến hình, tiếp tục rượt đuổi
        if (currentHealth > 0) state = BossState.Chase;

        Debug.Log("<color=magenta>BIẾN HÌNH HOÀN TẤT!</color>");
    }

    void Die()
    {
        Debug.Log("BOSS ĐÃ CHẾT!");

        // --- GỌI SCRIPT TRĂN TRỐI KHI CHẾT ---
        TalkToBoss talkScript = GetComponent<TalkToBoss>();
        if (talkScript != null)
        {
            talkScript.Die();
        }
        // --------------------------------------

        // --- THÊM DÒNG NÀY ĐỂ TẮT THANH MÁU ---
        if (healthBarCanvas != null) healthBarCanvas.SetActive(false);
        // --------------------------------------

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
            // 3 đòn của Dạng 2 (Đã được nâng cấp)
            int attackPhase2 = Random.Range(0, 3);
            if (attackPhase2 == 0) anim.SetTrigger("RangedAttack"); // Đòn 1
            else if (attackPhase2 == 1) anim.SetTrigger("RangedAttack2"); // Đòn 2
            else anim.SetTrigger("RangedAttack3"); // Đòn 3
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