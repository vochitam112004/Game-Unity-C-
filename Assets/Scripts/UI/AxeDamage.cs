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

    [Header("Cấu hình Âm thanh")]
    public AudioClip hitSound; // Kéo tiếng chém trúng thịt vào đây
    private AudioSource audioSource;

    private Collider triggerCollider;
    private System.Collections.Generic.List<Collider> alreadyHit = new System.Collections.Generic.List<Collider>();

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // Nếu Collider bị tắt (không tấn công), reset danh sách đã trúng
        if (triggerCollider == null || !triggerCollider.enabled)
        {
            alreadyHit.Clear();
            return;
        }

        Collider[] hits;
        
        if (triggerCollider is BoxCollider box)
        {
            Vector3 center = transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size, transform.lossyScale) * 0.5f;
            hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        }
        else if (triggerCollider is SphereCollider sphere)
        {
            Vector3 center = transform.TransformPoint(sphere.center);
            float radius = sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
            hits = Physics.OverlapSphere(center, radius);
        }
        else
        {
            // Fallback nếu dùng dạng khác
            hits = Physics.OverlapBox(triggerCollider.bounds.center, triggerCollider.bounds.extents, Quaternion.identity);
        }

        foreach (var other in hits)
        {
            if (other.CompareTag("Enemy") && !alreadyHit.Contains(other))
            {
                alreadyHit.Add(other); // Đánh dấu đã chém trúng
                ProcessHit(other);
            }
        }
    }

    private void ProcessHit(Collider other)
    {
        bool isHitSuccess = false;

        WolfController wolf = other.GetComponent<WolfController>();
        if (wolf != null)
        {
            wolf.TakeDamage(wolfDamage);
            isHitSuccess = true;
        }

        BossBrain boss = other.GetComponent<BossBrain>();
        if (boss != null)
        {
            boss.TakeDamage(bossDamage);
            isHitSuccess = true;
        }

        if (isHitSuccess)
        {
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

            if (bloodPrefab != null)
            {
                Instantiate(bloodPrefab, other.transform.position + Vector3.up * 1.2f, Quaternion.identity);
            }

            CameraShake shaker = Camera.main.GetComponent<CameraShake>();
            if (shaker != null)
            {
                StartCoroutine(shaker.Shake(shakeDuration, shakeMagnitude));
            }

            Debug.Log("BỤP! Chém trúng quái qua OverlapBox!");
        }
    }
}