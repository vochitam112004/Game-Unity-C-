using UnityEngine;
using UnityEngine.UI;

namespace ThachSanh.UI
{
    public class Settings : MonoBehaviour
    {
        [Header("Kéo cái Khung/Panel Settings vào đây")]
        public GameObject settingsPanel;

        // Biến tự động lưu lại BẢNG chính xác (Không dùng Tên Text nữa để chống lỗi)
        private GameObject previousMenuPanel;

        private void Awake()
        {
            // Mới vào game thì bảng Settings phải tắt đi lập tức
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Đăng ký sự kiện tự động gỡ Settings mỗi khi load cảnh mới
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene var1, UnityEngine.SceneManagement.LoadSceneMode var2)
        {
            // Mỗi lần qua cảnh mới, đóng bảng Settings lại cho an toàn
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        // --- HÀM MỚI SIÊU VIỆT DÀNH CHO MENU VÀ PAUSE GỌI ---
        public void OpenSettingsAuto(GameObject callerPanel)
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
            
            // TỰ ĐỘNG GHI NHỚ ai gọi bảng Settings ra bằng THỂ XÁC (không dùng tên)
            previousMenuPanel = callerPanel;
        }

        // --- GẮN HÀM NÀY VÀO NÚT "X" HOẶC "BACK" Ở TRONG BẢNG SETTINGS ---
        public void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            
            // KÍCH HOẠT LẠI THẰNG VỪA GỌI TỰ ĐỘNG
            if (previousMenuPanel != null)
            {
                previousMenuPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("Settings.cs: LỖI! Nó quên mất thằng nào đã gọi nó rồi. Báo Code đền mạng ngay!");
            }
        }

        // ==========================================
        //         CÁC HÀM CÀI ĐẶT HÌNH ẢNH
        // ==========================================

        // 1. CHỌN ĐỘ PHÂN GIẢI (RESOLUTION)
        // Gắn vào hàm OnValueChanged của một Dropdown
        public void SetResolution(int resolutionIndex)
        {
            Resolution[] resolutions = Screen.resolutions;
            if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
            {
                Resolution res = resolutions[resolutionIndex];
                Screen.SetResolution(res.width, res.height, Screen.fullScreen);
                Debug.Log($"Đã đổi độ phân giải: {res.width}x{res.height}");
            }
        }

        // 2. BẬT/TẮT FULLSCREEN
        // Gắn vào OnValueChanged của một Toggle (Ô checkbox)
        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
            Debug.Log($"Đã đổi Fullscreen thành: {isFullscreen}");
        }

        // 3. CHẤT LƯỢNG ĐỒ HỌA (GRAPHICS QUALITY)
        // Gắn vào OnValueChanged của một Dropdown (Thường có các mức: Low, Medium, High, Ultra)
        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            Debug.Log($"Đã đổi Graphic Quality sang mức số: {qualityIndex}");
        }

        // 4. DLSS / FSR TỰ CHẾ BẰNG RENDER SCALE
        // Vì Unity không có hàm dựng sẵn để nạp DLSS/FSR chuẩn của Nvidia/AMD nếu không cài thêm package,
        // Cách nhanh nhất và tương thích nhất là dùng Render Scale (giảm độ phân giải render bên trong để máy mượt hơn, nhưng UI vẫn nét).
        // Gắn vào OnValueChanged của Slider (Kéo từ 0.5 đến 1.0)
        public void SetRenderScale(float scale)
        {
#if UNITY_URP // Nếu game bạn xài Universal Render Pipeline
            var urpAsset = (UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            if (urpAsset != null)
            {
                urpAsset.renderScale = scale;
                Debug.Log($"Đã đổi hệ số FSR/DLSS (Render Scale) thành: {scale}");
            }
#else
            Debug.LogWarning("Project không xài URP nên không chỉnh Render Scale tự động được. Bạn cần cài package DLSS riêng (VD: Nvidia DLSS plugin).");
#endif
        }
    }
}
