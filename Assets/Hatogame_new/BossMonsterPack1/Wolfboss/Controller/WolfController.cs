using TMPro;
using UnityEngine;
using UnityEngine.AI; // Cần thiết để dùng NavMesh
using UnityEngine.UI; // Cần thiết để điều khiển UI thanh máu

public class WolfController : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 2f;
    public float detectRange = 50f;
    public float attackRange = 2.5f; // Chỉnh lại cho sát hơn chút

    [Header("Cấu hình máu")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("Tham chiếu UI")]
    public Image healthBarFill;    // Kéo HealthBar_Fill vào đây
    public TextMeshProUGUI healthText;       // Kéo HealthText vào đây
    public GameObject uiCanvas;    // Kéo cái Canvas trên đầu sói vào đây

    private Animator anim;
    private Transform player;
    private NavMeshAgent agent;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Tìm Thạch Sanh qua Tag
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        // Khởi tạo máu
        currentHealth = maxHealth;
        if (agent != null) agent.speed = moveSpeed;

        UpdateHealthUI();
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // 1. Logic di chuyển và tấn công
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
            StopMoving();
        }

        // 2. Logic xoay UI hướng về Camera (Billboarding)
        if (uiCanvas != null && Camera.main != null)
        {
            uiCanvas.transform.LookAt(uiCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
                                     Camera.main.transform.rotation * Vector3.up);
        }
    }

    void ChasePlayer()
    {
        anim.SetBool("IsMoving", true);
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void StopAndAttack()
    {
        anim.SetBool("IsMoving", false);
        if (agent != null) agent.isStopped = true;

        // Xoay mặt về phía player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Không xoay lên xuống
        transform.rotation = Quaternion.LookRotation(direction);

        anim.SetTrigger("Attack");
    }

    void StopMoving()
    {
        anim.SetBool("IsMoving", false);
        if (agent != null) agent.isStopped = true;
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0) Die();
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
        anim.SetTrigger("Die");
        if (agent != null) agent.isStopped = true;
        if (uiCanvas != null) uiCanvas.SetActive(false); // Ẩn thanh máu khi chết
        Destroy(gameObject, 2.5f);
    }
}