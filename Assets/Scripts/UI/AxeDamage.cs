using UnityEngine;

public class AxeDamage : MonoBehaviour
{
    [Header("Cấu hình Hiệu ứng")]
    public GameObject bloodPrefab; // Kéo Prefab máu vào đây
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.2f;

    [Header("Cấu hình Sát thương")]
    public int wolfDamage = 1;
    public float bossDamage = 50f; // Chằn Tinh máu tận 1000, mỗi nhát chém 50 máu cho vừa tầm

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ xử lý khi chạm vào vật thể có tag "Enemy"
        if (other.CompareTag("Enemy"))
        {
            bool isHitSuccess = false; // Cờ đánh dấu xem có chém trúng không

            // 1. Kiểm tra xem có trúng Sói không
            WolfController wolf = other.GetComponent<WolfController>();
            if (wolf != null)
            {
                wolf.TakeDamage(wolfDamage);
                isHitSuccess = true;
            }

            // 2. Kiểm tra xem có trúng Chằn Tinh (BossBrain) không
            BossBrain boss = other.GetComponent<BossBrain>();
            if (boss != null)
            {
                boss.TakeDamage(bossDamage);
                isHitSuccess = true;
            }

            // 3. XỬ LÝ HIỆU ỨNG CHUNG (Tóe máu & Rung màn hình)
            // Nếu chém trúng bất kỳ con quái nào, khối lệnh này sẽ chạy
            if (isHitSuccess)
            {
                // Tạo hiệu ứng máu văng ra (Cộng thêm Vector3.up * 1.2f để xịt máu từ tầm ngực quái)
                if (bloodPrefab != null)
                {
                    Instantiate(bloodPrefab, other.transform.position + Vector3.up * 1.2f, Quaternion.identity);
                }

                // Kích hoạt rung màn hình
                CameraShake shaker = Camera.main.GetComponent<CameraShake>();
                if (shaker != null)
                {
                    StartCoroutine(shaker.Shake(shakeDuration, shakeMagnitude));
                }

                Debug.Log("BỤP! Chém trúng quái, rung màn hình và tóe máu!");
            }
        }
    }
}