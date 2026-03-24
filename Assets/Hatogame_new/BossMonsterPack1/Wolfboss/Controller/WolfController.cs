using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class WolfController : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 2f;
    public float chaseSpeedMultiplier = 1.5f;
    public float detectRange = 50f;
    public float attackRange = 2.5f;

    [Header("Cấu hình đi dạo (Wander)")]
    public float wanderRadius = 15f; // Khoảng cách tối đa mỗi lần đi dạo
    public float wanderTimer = 5f;   // Đứng chơi bao lâu thì đi tiếp
    private float timer;

    [Header("Cấu hình máu")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("Tham chiếu UI")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;
    public GameObject uiCanvas;

    private Animator anim;
    private Transform player;
    private NavMeshAgent agent;

    [Header("Cấu hình chiến đấu")]
    public Collider attackHitbox;
    public AudioClip dieSound; // m thanh khi quái chết

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        currentHealth = maxHealth;
        if (agent != null) agent.speed = moveSpeed;

        timer = wanderTimer; // Khởi tạo đồng hồ đi dạo
        UpdateHealthUI();
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 1. Logic di chuyển và chiến đấu
        if (distance <= detectRange)
        {
            if (distance <= attackRange)
            {
                StopAndAttack();
            }
            else
            {
                ChasePlayer();
            }
        }
        else
        {
            // Thay thế StopMoving() bằng Wander() khi không thấy người chơi
            Wander();
        }

        // 2. Logic xoay UI hướng về Camera (Billboarding)
        if (uiCanvas != null && Camera.main != null)
        {
            uiCanvas.transform.LookAt(uiCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
                                     Camera.main.transform.rotation * Vector3.up);
        }
    }

    // --- CÁC HÀM HÀNH ĐỘNG ---

    void Wander()
    {
        if (agent != null) agent.speed = moveSpeed; // Trả lại tốc độ đi bộ bình thường

        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(newPos);
            }
            timer = 0;
        }

        if (agent.velocity.magnitude > 0.1f)
        {
            anim.SetBool("IsMoving", true);
        }
        else
        {
            anim.SetBool("IsMoving", false);
        }
    }

    void ChasePlayer()
    {
        anim.SetBool("IsMoving", true);
        if (agent != null)
        {
            agent.speed = moveSpeed * chaseSpeedMultiplier; // Bật chế độ chạy nước rút
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // --- GỌI SCRIPT BẮT ĐẦU CHỬI NHAU KHI PHÁT HIỆN THẠCH SANH ---
        TalkToBoss talkScript = GetComponent<TalkToBoss>();
        if (talkScript != null)
        {
            talkScript.StartBossFight();
        }
        // -----------------------------------------------------------
    }

    void StopAndAttack()
    {
        anim.SetBool("IsMoving", false);
        if (agent != null) agent.isStopped = true;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        anim.SetTrigger("Attack");
    }

    void StopMoving()
    {
        anim.SetBool("IsMoving", false);
        if (agent != null) agent.isStopped = true;
    }

    // --- HÀM HỖ TRỢ TOÁN HỌC ---
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthUI();

        // --- GỌI SCRIPT CHỬI BỚI KHI BỊ CHÉM ---
        TalkToBoss talkScript = GetComponent<TalkToBoss>();
        if (talkScript != null)
        {
            talkScript.TakeHit();
        }
        // ----------------------------------------

        if (currentHealth <= 0) Die();
    }

    // --- HÀM GỌI TỪ ANIMATION EVENT ---
    public void EnableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        if (healthText != null)
        {
            healthText.text = currentHealth + "/" + maxHealth;
        }
    }

    void Die()
    {
        isDead = true;

        if (dieSound != null)
        {
            // PlayClipAtPoint giúp tạo sound độc lập, không bị cắt đứt khi object bị Destroy
            AudioSource.PlayClipAtPoint(dieSound, transform.position);
        }

        // --- GỌI SCRIPT TRĂN TRỐI KHI CHẾT ---
        TalkToBoss talkScript = GetComponent<TalkToBoss>();
        if (talkScript != null)
        {
            talkScript.Die();
        }
        // --------------------------------------

        anim.SetTrigger("Die");
        if (agent != null) agent.isStopped = true;
        if (uiCanvas != null) uiCanvas.SetActive(false);
        Destroy(gameObject, 2.5f);
    }
}