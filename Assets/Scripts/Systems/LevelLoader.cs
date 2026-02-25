using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThachSanh.Systems
{
    /// <summary>
    /// Quản lý chuyển scene/chương truyện.
    /// </summary>
    public static class LevelLoader
    {
        /// <summary>
        /// Tên scene theo index chương. Phải khớp với Build Settings.
        /// Thêm scene mới khi tạo chương 2, 3...
        /// </summary>
        public static readonly string[] ChapterScenes =
        {
            "Chuong1_GocDa"        // Index 0 - Chương 1: Góc Đá
            // "02_MieuChanTinh",  // Index 1 - thêm khi có scene
            // "03_HangDaiBang"    // Index 2 - thêm khi có scene
        };

        private const string MainMenuSceneName = "MainMenu";

        /// <summary>
        /// Dữ liệu load game đang chờ - GameSceneBootstrap sẽ đọc và áp dụng khi scene load.
        /// </summary>
        public static GameSaveData PendingLoadData { get; set; }

        /// <summary>
        /// Load Main Menu.
        /// </summary>
        public static void LoadMainMenu()
        {
            PendingLoadData = null;
            SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Load chương theo index (New Game thường dùng index 0).
        /// </summary>
        public static void LoadChapter(int chapterIndex)
        {
            PendingLoadData = null;

            if (chapterIndex < 0 || chapterIndex >= ChapterScenes.Length)
            {
                Debug.LogError($"[LevelLoader] Chapter index {chapterIndex} không hợp lệ. Load Chương 0.");
                chapterIndex = 0;
            }

            string sceneName = ChapterScenes[chapterIndex];
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Load game từ save - lưu data vào PendingLoadData, load scene, GameSceneBootstrap sẽ restore.
        /// </summary>
        public static void LoadGame(GameSaveData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[LevelLoader] Không có dữ liệu save để load.");
                return;
            }

            PendingLoadData = data;

            int chapterIndex = Mathf.Clamp(data.chapterIndex, 0, ChapterScenes.Length - 1);
            string sceneName = ChapterScenes[chapterIndex];
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Lấy tên scene hiện tại.
        /// </summary>
        public static string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Lấy index chương hiện tại (-1 nếu không phải scene chương).
        /// </summary>
        public static int GetCurrentChapterIndex()
        {
            string current = GetCurrentSceneName();
            for (int i = 0; i < ChapterScenes.Length; i++)
            {
                if (ChapterScenes[i] == current) return i;
            }
            return -1;
        }
    }
}
