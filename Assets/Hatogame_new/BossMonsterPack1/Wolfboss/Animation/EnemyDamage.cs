using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damageAmount = 20; // Sát thương mỗi lần cắn

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem vùng sát thương có chạm vào Thạch Sanh (Player) không
        if (other.CompareTag("Player"))
        {
            // Lấy script PlayerHealth trên người Thạch Sanh và gọi hàm trừ máu
            PlayerHealth playerHp = other.GetComponent<PlayerHealth>();
            if (playerHp != null)
            {
                playerHp.TakeDamage(damageAmount);
                Debug.Log("Sói đã cắn trúng Thạch Sanh! Gây " + damageAmount + " sát thương.");
            }
        }
    }
}