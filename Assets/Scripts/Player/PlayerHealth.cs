using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI References")]
    public Slider healthSlider;

    [Header("Liên Kết Script")]
    // ĐỔI THÀNH PUBLIC ĐỂ KÉO THẢ TRÊN INSPECTOR
    public Player playerScript;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        // Tự động tìm script Player (Phòng hờ nếu Khoa quên kéo thả)
        if (playerScript == null)
        {
            playerScript = GetComponent<Player>();
        }
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (playerScript != null)
        {
            playerScript.enabled = true;
        }

        Debug.Log("💖 Thạch Sanh đã được hồi đầy máu!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // --- KIỂM TRA ĐỠ ĐÒN ---
        if (playerScript != null)
        {
            if (playerScript.isBlocking)
            {
                Debug.Log("🛡️ ĐỠ ĐÒN THÀNH CÔNG! Miễn nhiễm sát thương.");
                return; // Thoát hàm ngay, KHÔNG trừ máu
            }
        }
        else
        {
            Debug.Log("<color=red>LỖI: PlayerHealth không tìm thấy script Player!</color>");
        }

        // Nếu không đỡ, trừ máu như bình thường
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("Thạch Sanh bị cắn mất " + damage + " máu! Còn: " + currentHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Thạch Sanh đã gục ngã...");

        if (playerScript != null)
        {
            // 1. Gọi Animator để phát Animation gục ngã (chữ "Die" phải khớp trong Unity)
            if (playerScript.playerAnim != null)
            {
                // Reset các lệnh khác để tránh kẹt animation
                playerScript.playerAnim.ResetTrigger("ATK1");
                playerScript.playerAnim.ResetTrigger("combo1");
                playerScript.playerAnim.SetTrigger("Die");
            }

            // 2. Khóa code điều khiển để Thạch Sanh không đi lại hay chém được nữa
            playerScript.enabled = false;
        }
    }
}