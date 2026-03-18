using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace ThachSanh.World
{
    public class GameWinTrigger : MonoBehaviour
    {
        [Header("Thoại của Thạch Sanh khi đến vườn hoa")]
        public DialogueLine[] victoryDialogues;

        [Header("UI & Chuyển Scene")]
        [Tooltip("Kéo thả Panel Chiến thắng (nếu có). Nếu rỗng, sẽ tự động chuyển về Menu.")]
        public GameObject winPanel;
        
        [Tooltip("Tên Scene Menu để quay về khi không gắn winPanel (Mặc định: UI)")]
        public string mainMenuSceneName = "UI";

        private bool hasTriggered = false;
        private GameObject currentActiveCamera;

        private void Start()
        {
            if (winPanel != null)
            {
                winPanel.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            // Kiểm tra Player thông qua Tag
            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                
                if (DialogueSystem.Instance != null && victoryDialogues != null && victoryDialogues.Length > 0)
                {
                    // Đăng ký sự kiện thoại kết thúc để xử lý thắng game
                    DialogueSystem.OnDialogueEnded += HandleVictory;
                    DialogueSystem.OnLineStarted += HandleCinematicStep; // Bật tắt Camera / Hình ảnh riêng
                    DialogueSystem.Instance.StartDialogueWithLines(victoryDialogues);
                }
                else
                {
                    // Nếu không có thoại, thắng luôn
                    HandleVictory();
                }
            }
        }

        private void HandleVictory()
        {
            // Hủy đăng ký sự kiện để tránh gọi lặp
            DialogueSystem.OnDialogueEnded -= HandleVictory;
            DialogueSystem.OnLineStarted -= HandleCinematicStep;

            if (currentActiveCamera != null)
            {
                currentActiveCamera.SetActive(false);
                currentActiveCamera = null;
            }

            Debug.Log("[GameWinTrigger] Kích hoạt màn chiến thắng!");

            if (winPanel != null)
            {
                // Bật Panel chiến thắng lên màn hình
                winPanel.SetActive(true);
                
                // Tự động quay về Main Menu sau 4 giây
                StartCoroutine(WaitAndReturnToMenu(4f));
            }
            else
            {
                // Nếu không có Panel, chuyển thẳng về Main Menu
                if (!string.IsNullOrEmpty(mainMenuSceneName))
                {
                    SceneManager.LoadScene(mainMenuSceneName);
                }
                else
                {
                    Debug.LogWarning("[GameWinTrigger] Không tìm thấy winPanel lẫn tên Scene hơp lệ!");
                }
            }
        }

        private void HandleCinematicStep(DialogueLine line)
        {
            GameObject targetCam = line.customCamera;

            if (targetCam != null && currentActiveCamera != targetCam)
            {
                if (currentActiveCamera != null) currentActiveCamera.SetActive(false);
                targetCam.SetActive(true);
                currentActiveCamera = targetCam;
            }
        }

        private IEnumerator WaitAndReturnToMenu(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        private void OnDestroy()
        {
            // Đảm bảo không bị leak event
            DialogueSystem.OnDialogueEnded -= HandleVictory;
            DialogueSystem.OnLineStarted -= HandleCinematicStep;
        }
    }
}
