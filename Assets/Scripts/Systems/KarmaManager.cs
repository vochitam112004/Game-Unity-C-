using UnityEngine;

namespace ThachSanh.Systems
{
    public class KarmaManager : MonoBehaviour
    {
        // Singleton pattern để các script khác dễ dàng truy cập
        public static KarmaManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int currentKarma = 0;
        [SerializeField] private int maxKarma = 100;
        [SerializeField] private int minKarma = -100;

        private void Awake()
        {
            // Đảm bảo chỉ có một KarmaManager duy nhất trong game
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Giữ hệ thống này không bị xóa khi chuyển cảnh

                Debug.Log("Hệ thống Karma đã khởi động thành công!");
                ChangeKarma(100);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Hàm để thay đổi điểm Nghiệp (Cộng hoặc Trừ)
        public void ChangeKarma(int amount)
        {
            currentKarma += amount;
            currentKarma = Mathf.Clamp(currentKarma, minKarma, maxKarma);

            Debug.Log($"Điểm Nghiệp hiện tại: {currentKarma}");

            // Bạn có thể thêm logic kiểm tra ở đây
            if (currentKarma <= -50)
            {
                Debug.LogWarning("Cảnh báo: Nghiệp quá nặng, Lý Thông có thể xuất hiện!");
            }
        }

        // Hàm để lấy điểm Nghiệp hiện tại (Dùng cho Save/Load)
        public int GetCurrentKarma()
        {
            return currentKarma;
        }

        // Hàm thiết lập điểm (Dùng khi Load game)
        public void SetKarma(int value)
        {
            currentKarma = value;
        }

        void Update()
        {
#if UNITY_EDITOR
            // Chỉ trong Editor: Nhấn phím K để trừ 60 điểm và test cảnh báo
            if (Input.GetKeyDown(KeyCode.K))
            {
                ChangeKarma(-60);
            }
#endif
        }
    }
}
