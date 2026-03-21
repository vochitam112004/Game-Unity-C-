using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[System.Serializable]
public struct IntroSlide
{
    [Tooltip("Hình ảnh sẽ hiển thị")]
    public Sprite image;
    [Tooltip("Danh sách các câu thoại cho hình ảnh này")]
    public DialogueLine[] dialogues;
}

public class StoryIntroManager : MonoBehaviour
{
    [Header("Cài đặt cốt truyện")]
    [Tooltip("Danh sách các ảnh và câu thoại tương ứng")]
    public IntroSlide[] slides;
    [Tooltip("Kéo Component Image của UI vào đây để thay đổi ảnh nền")]
    public Image backgroundImage;
    
    [Header("Chuyển Scene khi kết thúc")]
    [Tooltip("Tên màn chơi chính sẽ load sau khi xem xong Intro")]
    public string nextSceneName = "Chuong1_GocDa";

    [Header("Màn hình Tải game (Kèm Hướng dẫn)")]
    [Tooltip("Panel chứa giao diện hướng dẫn chơi và chữ Loading. Kéo vào nếu muốn dùng.")]
    public GameObject loadingScreenPanel;
    [Tooltip("Thanh trượt hiển thị tiến độ tải (Tùy chọn)")]
    public Slider loadingProgressBar;
    [Tooltip("Dòng chữ 'Nhấn phím bất kỳ để vào game' - Hiện ra khi load xong. (Tùy chọn)")]
    public GameObject pressAnyKeyText;

    private int currentSlideIndex = 0;
    private bool isReadyToStartGame = false;
    private AsyncOperation loadOperation;

    private void Start()
    {
        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("[StoryIntroManager] Chưa cài đặt ảnh (Slides) trong Inspector!");
            LoadNextScene();
            return;
        }

        // Đăng ký sự kiện khi hội thoại kết thúc
        DialogueSystem.OnDialogueEnded += HandleDialogueEnded;
        
        // Bắt đầu slide đầu tiên
        currentSlideIndex = 0;
        StartSlide(currentSlideIndex);
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh lỗi memory leak
        DialogueSystem.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void StartSlide(int index)
    {
        if (index >= 0 && index < slides.Length)
        {
            // Đổi hình ảnh nền Fullscreen
            if (backgroundImage != null && slides[index].image != null)
            {
                backgroundImage.sprite = slides[index].image;
                backgroundImage.gameObject.SetActive(true);
            }

            // Chạy hội thoại của slide này bằng DialogueSystem sẵn có
            if (slides[index].dialogues != null && slides[index].dialogues.Length > 0)
            {
                StartCoroutine(WaitSystemReadyAndStartDialogue(slides[index].dialogues));
            }
            else
            {
                // Nếu slide này không có câu thoại nào, hiển thị ảnh trong 3 giây rồi tự động đi tiếp
                StartCoroutine(WaitAndNextSlide(3f));
            }
        }
        else
        {
            // Nếu đã chạy qua hết mảng slide -> Tự động sang Scene chính
            LoadNextScene();
        }
    }

    private void Update()
    {
        // Giai đoạn 2: Đang ở màn hình Loading và đã tải xong Data của scene tiếp theo
        if (isReadyToStartGame)
        {
            if (Input.anyKeyDown)
            {
                // Cho phép Unity mở Scene mới lên
                if (loadOperation != null) loadOperation.allowSceneActivation = true;
            }
            return;
        }

        // Cấm bấm linh tinh nếu đang trong quá trình tải mà chưa xong
        if (loadingScreenPanel != null && loadingScreenPanel.activeSelf) return;

        // Giai đoạn 1: Cho phép bấm chuột trái, Enter hoặc phím Space để chuyển nhanh thoại / ảnh
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
            {
                // Đang hiển thị thoại -> Bấm để chạy nhanh chữ / qua câu tiếp theo
                DialogueSystem.Instance.DisplayNextSentence();
            }
            else
            {
                // Không có thoại hoặc bị lỗi -> Bấm để bỏ qua số giây chờ và sang ảnh luôn
                StopAllCoroutines();
                currentSlideIndex++;
                StartSlide(currentSlideIndex);
            }
        }
    }

    private IEnumerator WaitSystemReadyAndStartDialogue(DialogueLine[] lines)
    {
        // Đợi 1 frame để DialogueSystem định hình
        yield return null; 
        
        // Kiểm tra xem đã cho DialogueSystem vào Scene chưa
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel != null)
        {
            DialogueSystem.Instance.StartDialogueWithLines(lines);
        }
        else
        {
            Debug.LogError("[StoryIntroManager] Không tìm thấy DialogueSystem! Tự động chuyển ảnh sau 3s...");
            // Không có thoại, tự chuyển ảnh sau 3 giây hoặc bấm phím để qua luôn
            StartCoroutine(WaitAndNextSlide(3f));
        }
    }

    private void HandleDialogueEnded()
    {
        // Hàm này tự động được DialogueSystem gọi khi chạy xong thoại
        currentSlideIndex++;
        StartSlide(currentSlideIndex);
    }

    private IEnumerator WaitAndNextSlide(float time)
    {
        yield return new WaitForSeconds(time);
        currentSlideIndex++;
        StartSlide(currentSlideIndex);
    }

    // Nút Bỏ qua này giờ sẽ hoạt động như "Chuyển sang ảnh/đoạn thoại tiếp theo"
    public void SkipIntro()
    {
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
        {
            // Chuyển sang câu thoại tiếp theo thay vì kết thúc toàn bộ đoạn thoại của ảnh
            DialogueSystem.Instance.DisplayNextSentence();
        }
        else
        {
            StopAllCoroutines();
            // Nếu đang trong thời gian hiển thị hình ảnh không có thoại (đã hiển thị đủ số giây), thì tự nhảy slide luôn
            currentSlideIndex++;
            StartSlide(currentSlideIndex);
        }
    }

    // Nút Bỏ qua toàn bộ Intro (Tự về màn hình hướng dẫn/game ngay lập tức)
    public void SkipAllIntro()
    {
        // Gỡ sự kiện HandleDialogueEnded để không bị tự nhảy slide tiếp tục khi cố tình thoát intro
        DialogueSystem.OnDialogueEnded -= HandleDialogueEnded;
        
        StopAllCoroutines();
        
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel != null)
        {
            DialogueSystem.Instance.EndDialogue(); // Tắt bảng thoại cho an toàn
        }
        
        LoadNextScene();
    }

    // Nút Bỏ qua ở màn hình Hướng dẫn (Loading Screen) để vào ngay màn chơi
    public void SkipLoadingScreen()
    {
        if (loadOperation != null)
        {
            // Cho phép Unity mở Scene mới lên ngay khi tải xong (hoặc mở lặp tức nếu đã tải xong 90%)
            loadOperation.allowSceneActivation = true;
        }
    }

    private void LoadNextScene()
    {
        // Nếu có thiết lập màn hình Loading thì xài, không có thì Load thẳng
        if (loadingScreenPanel != null)
        {
            // Bật Panel chứa hướng dẫn / Thanh tải game lên
            loadingScreenPanel.SetActive(true);
            
            // Giấu ảnh nền của cốt truyện đi để nhường chỗ
            if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
            if (pressAnyKeyText != null) pressAnyKeyText.SetActive(false);
            
            StartCoroutine(LoadSceneAsync());
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        // Yêu cầu Unity tải scene mới dưới chạy ngầm
        loadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        
        // Ngăn không cho tự động chuyển Scene kể cả khi đã tải xong 100%
        loadOperation.allowSceneActivation = false; 

        while (!loadOperation.isDone)
        {
            // Trong Unity, tiến độ load chạy từ 0 đến 0.9 là hoàn tất tải data
            float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            
            if (loadingProgressBar != null) 
            {
                loadingProgressBar.value = progress;
            }

            // Nếu đã tải xong data (0.9)
            if (loadOperation.progress >= 0.9f)
            {
                // Điểm dừng: Hiển thị chữ "Bấm phím bất kỳ để vào game"
                isReadyToStartGame = true;
                if (pressAnyKeyText != null) pressAnyKeyText.SetActive(true);
                break; // Thoát vòng lặp chờ tải
            }

            yield return null;
        }
    }
}
