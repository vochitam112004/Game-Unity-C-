using UnityEngine;
using UnityEngine.UI; // BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ GỌI UI

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Giao diện UI")]
    public Image healthBarFill; // Kéo thả cục HealthBar_Fill vào đây

    void Start()
    {
        // Vừa vào game là đầy máu
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    // Hàm này sẽ bị con Boss gọi khi nó đấm trúng
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Không cho máu tụt xuống số âm
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();
        Debug.Log("OÁI! Thạch Sanh mất " + damage + " máu! Còn lại: " + currentHealth);

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void UpdateHealthUI()
    {
        // Công thức toán học: Máu hiện tại / Máu tối đa = Tỉ lệ từ 0 đến 1
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("THẠCH SANH ĐÃ TỬ TRẬN!!!");
        // Tạm thời tắt nhân vật đi khi chết
        this.gameObject.SetActive(false);
    }
}