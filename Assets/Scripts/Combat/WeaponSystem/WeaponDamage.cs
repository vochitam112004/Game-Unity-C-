using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    public int damageAmount = 20; // Lượng sát thương mỗi nhát chém

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem rìu có chạm vào object mang tag "Enemy" (Quái vật) không
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("💥 Thạch Sanh đã chém trúng: " + other.gameObject.name + " | Gây " + damageAmount + " sát thương!");

            // Ở bài sau, mình sẽ viết code gọi hàm trừ máu của con quái tại đây!
        }
    }
}