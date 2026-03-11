using UnityEngine;

public class WolfController : MonoBehaviour
{
    private Animator anim;
    private Transform player;
    public float moveSpeed = 2f;
    public float detectRange = 50f;   // Tầm nhìn thấy Thạch Sanh
    public float attackRange = 1.5f; // Tầm để dừng lại đánh
    public int health = 3;           // Sói chết sau 3 hit
    private bool isDead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        // Tìm Thạch Sanh qua Tag "Player"
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectRange)
        {
            if (distance <= attackRange)
            {
                // Đứng lại tấn công
                anim.SetBool("IsMoving", false);
                AttackPlayer();
            }
            else
            {
                // Đuổi theo Thạch Sanh
                anim.SetBool("IsMoving", true);
                MoveTowardsPlayer();
            }
        }
        else
        {
            anim.SetBool("IsMoving", false);
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(target);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    void AttackPlayer()
    {
        // Xoay mặt về phía người chơi khi đánh
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        anim.SetTrigger("Attack");
    }

    // Hàm này được gọi khi Thạch Sanh đánh trúng Sói
    public void TakeDamage()
    {
        if (isDead) return;
        health--;
        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");
        Destroy(gameObject, 2.5f); // Chờ diễn xong animation die rồi biến mất
    }
}