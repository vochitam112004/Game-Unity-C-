using UnityEngine;

public class AxeDamage : MonoBehaviour
{
    public GameObject bloodPrefab; // Kéo Prefab máu vào đây
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            WolfController wolf = other.GetComponent<WolfController>();
            if (wolf != null)
            {
                // 1. Gây sát thương
                wolf.TakeDamage(1);

                // 2. Tạo hiệu ứng máu tại điểm chạm
                if (bloodPrefab != null)
                {
                    Instantiate(bloodPrefab, other.transform.position + Vector3.up, Quaternion.identity);
                }

                // 3. Rung màn hình
                CameraShake shaker = Camera.main.GetComponent<CameraShake>();
                if (shaker != null)
                {
                    StartCoroutine(shaker.Shake(shakeDuration, shakeMagnitude));
                }

                Debug.Log("Chém trúng! Rung màn hình và tóe máu!");
            }
        }
    }
}