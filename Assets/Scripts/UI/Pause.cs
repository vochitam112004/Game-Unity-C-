using UnityEngine;

namespace ThachSanh.UI
{
    public class Pause : MonoBehaviour
    {
        [Header("Kéo Panel Pause vào đây")]
        public GameObject pausePanel;

        [Header("Kéo bảng Settings vào đây để nút Setting mở được")]
        public GameObject settingsPanel;

        private bool isPaused = false;

        void Start()
        {
            // Mới vào game thì Menu Pause phải mờ đi
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            // Đăng ký sự kiện tự động gỡ Pause mỗi khi load cảnh mới
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private float pauseUnlockTime = 0f;

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene var1, UnityEngine.SceneManagement.LoadSceneMode var2)
        {
            // Mỗi lần qua cảnh mới, phải dọn dẹp sạch sẽ trạng thái Pause cũ
            isPaused = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1f;

            // Đặt thời gian an toàn: Không cho bấm Pause trong 0.2s đầu màn chơi để tránh dính phím từ Menu
            pauseUnlockTime = Time.unscaledTime + 0.2f;
        }

        void Update()
        {
            // Nếu mới load scene xong chưa được 0.2s thì khóa nút Esc lại
            if (Time.unscaledTime < pauseUnlockTime) return;

            // Bấm Esc bật/tắt (Fix cả lỗi Input System mới)
            bool isEscPressed = false;
            
            // Lấy input từ hệ thống Cũ
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isEscPressed = true;
            }
            
            // Lấy input từ hệ thống Mới (nếu có)
#if ENABLE_INPUT_SYSTEM
            if (!isEscPressed && UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    isEscPressed = true;
                }
            }
#endif

            // Nếu ấn Esc, bắt đầu kiểm tra điều kiện
            if (isEscPressed)
            {
                if (UI.Instance == null)
                {
                    Debug.LogError("Pause.cs: NHỨT ĐẦU QUÁ! Bạn chưa tạo bất kỳ GameObject nào để gắn file 'UI.cs' vào! (Hoặc nó bị hỏng). Tạo 1 cục trống mang tên 'UI Manager', ném UI.cs vào đó đi!");
                }
                else if (!UI.Instance.CanPause)
                {
                    Debug.LogWarning("Pause.cs: Nút Esc bị MÀN HÌNH CHÍNH THEO DÕI VÀ KHOÁ LẠI. Bạn đang test ở màn hình Menu (Build Index = 0) đúng không? Pause chỉ chạy khi vào màn chơi chính thôi!");
                }
                else
                {
                    // Đủ điều kiện, tiến hành Bật/Tắt Pause
                    if (isPaused)
                    {
                        ResumeGame();
                    }
                    else
                    {
                        PauseGame();
                    }
                }
            }
        }

        // --- CÁC HÀM CÔNG KHAI ĐỂ GẮN VÀO NÚT BẤM ---

        public void PauseGame()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }

        // Hàm này được gọi khi ấn Esc để tiếp tục game
        public void ResumeGame()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }

        // Gắn vào nút Settings
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                // Gọi cha nội Settings ra, ném cho nó cái tên của thằng đang mở (Ví dụ: "Pause")
                Settings settingsScript = settingsPanel.GetComponent<Settings>();
                if (settingsScript != null)
                {
                    // Truyền XÁC THỊT TRỰC TIẾP sang cho Settings (Tuyệt đối không trượt đi đâu được)
                    settingsScript.OpenSettingsAuto(pausePanel != null ? pausePanel : gameObject);
                }
                else
                {
                    Debug.LogError("Pause.cs: Bảng Settings không có gắn Script Settings.cs!");
                }
                
                // Tự giấu Menu Pause đi
                if (pausePanel != null) pausePanel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Chưa kéo bảng Settings vào Script Pause.cs!");
            }
        }

        // Gắn vào nút Main Menu
        public void MainMenuButton()
        {
            Time.timeScale = 1f; // Phải nhả pause ra trước khi đổi màn
        }
    }
}
