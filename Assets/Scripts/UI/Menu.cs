using UnityEngine;
using UnityEngine.UI; // Dùng để can thiệp vào các nút
using UnityEngine.EventSystems; // Dùng để can thiệp lỗi kẹt phím UI
using UnityEngine.SceneManagement; // Dùng để chuyển Scene

namespace ThachSanh.UI
{
    public class Menu : MonoBehaviour
    {
        [Header("Kéo bảng Settings vào đây để nút Setting mở được")]
        public GameObject settingsPanel;

        [Header("Tên scene Cốt Truyện (Dành cho New Game)")]
        [Tooltip("Mảnh ghép đầu tiên của game là Intro (chứa 4 ảnh slide)")]
        public string firstLevelSceneName = "IntroScene";

        private void Start()
        {
            // Bật lại con trỏ chuột phòng trường hợp người chơi vừa thoát ra từ màn chơi
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Gắn hàm này vào sự kiện OnClick() của nút New Game
        public void NewGame()
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); // Fix lỗi kẹt nút UI
            Time.timeScale = 1f; // Đảm bảo thời gian chạy bình thường
            
            string sceneToLoad = firstLevelSceneName;
            
            // Ép buộc chuyển qua màn IntroScene nếu trong Inspector vẫn lưu tên cũ
            if (sceneToLoad == "Level1" || sceneToLoad == "Chuong1_GocDa") 
                sceneToLoad = "IntroScene";

            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("Menu.cs: Chưa nhập tên Scene Intro để mở New Game!");
            }
        }

        // (Đã xóa hàm chặn nút Continue game theo yêu cầu của bạn)

        // Gắn hàm này vào sự kiện OnClick() của nút Continue Game
        public void ContinueGame()
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); // Fix lỗi kẹt nút
            if (settingsPanel != null) settingsPanel.SetActive(false); // Ép đóng Settings
            Time.timeScale = 1f; // Đảm bảo game không bị dừng
            
            // XIN LƯU Ý: Phải có code đọc dữ liệu trong SaveSystem, sau đó truyền vào LevelLoader
            // Ví dụ (Tùy theo SaveSystem của bạn):
            // ThachSanh.Systems.SaveData saveData = SaveSystem.LoadSaveData();
            // LevelLoader.LoadChapter(saveData.currentLevelIndex); 
            
            // TẠM THỜI (Do chưa có ruột LoadSaveData): Load đại màn 0
        }

        [Header("Kéo nguyên cụm nút Main Menu vào đây")]
        public GameObject mainMenuPanel;

        // Gắn hàm này vào OnClick() của nút Settings ở Main Menu
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                // Gọi cha nội Settings ra, ném cho nó cái tên của thằng đang mở (Ví dụ: "Menu")
                Settings settingsScript = settingsPanel.GetComponent<Settings>();
                if (settingsScript != null)
                {
                    // Truyền XÁC THỊT TRỰC TIẾP sang cho Settings (Tuyệt đối không trượt đi đâu được)
                    settingsScript.OpenSettingsAuto(mainMenuPanel != null ? mainMenuPanel : gameObject);
                }
                else
                {
                    Debug.LogError("Menu.cs: Bảng Settings không có gắn Script Settings.cs!");
                }
                
                // Tự giấu mình đi cho đỡ chật màn hình
                if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Chưa kéo bảng Settings vào Script Menu.cs!");
            }
        }

        // Gắn hàm này vào OnClick() của nút Quit
        public void QuitGame()
        {
            Debug.Log("Thoát Menu");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
