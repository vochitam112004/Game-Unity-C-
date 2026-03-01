using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThachSanh.UI
{
    public class UI : MonoBehaviour
    {
        public static UI Instance;

        [Header("Kéo nguyên cái cụm làm Main Menu vào đây")]
        public GameObject menuContainer;

        // Biến kiểm tra xem có đang ở trong màn chơi không để cho phép ấn Pause
        public bool CanPause { get; private set; }

        private void Awake()
        {
            // Thiết lập DontDestroyOnLoad để file chung này chạy xuyên game
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Reset thời gian
            Time.timeScale = 1f;

            // Nếu đang ở màn hình đầu tiên (Index = 0, tức Main Menu)
            if (scene.buildIndex == 0)
            {
                // Bật Menu lên, tắt chức năng Pause
                if (menuContainer) menuContainer.SetActive(true);
                CanPause = false; 
            }
            else
            {
                // Nếu load sang map game (Ví dụ: Chuong1_GocDa)
                // Cất Menu đi, mở khoá cho phép Pause
                if (menuContainer) menuContainer.SetActive(false);
                CanPause = true;
            }
        }
    }
}
