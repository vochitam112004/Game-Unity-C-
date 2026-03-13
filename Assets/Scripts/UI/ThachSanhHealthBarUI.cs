using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThachSanhHealthBarUI : MonoBehaviour
{
    [Header("Tìm nhân vật")]
    public PlayerHealth playerHealth;

    [Header("Chữ máu (tùy chọn)")]
    public bool showHealthText = true;
    public string format = "{0} / {1}";

    [Header("Tham chiếu UI")]
    [SerializeField] private Image _fillImage;
    [SerializeField] public TextMeshProUGUI _healthText;

    void Start()
    {
        // Tự tìm Player nếu chưa kéo vào
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        UpdateHealthUI();
    }

    void Update()
    {
        if (playerHealth == null) return;

        UpdateHealthUI();

        // Giúp thanh máu luôn hướng về phía Camera
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }

    public void UpdateHealthUI()
    {
        if (playerHealth == null) return;

        // Cập nhật thanh dài/ngắn
        if (_fillImage != null)
        {
            _fillImage.fillAmount = playerHealth.currentHealth / playerHealth.maxHealth;
        }

        // Cập nhật con số 100/100
        if (showHealthText && _healthText != null)
        {
            _healthText.text = string.Format(format,
                Mathf.CeilToInt(playerHealth.currentHealth),
                Mathf.CeilToInt(playerHealth.maxHealth));
        }
    }
}