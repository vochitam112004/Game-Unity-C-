using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Sử dụng TextMeshPro cho UI Text đẹp hơn

[System.Serializable]
public struct DialogueLine
{
    [TextArea(3, 10)]
    public string sentence;
    [Tooltip("Thời gian chờ sau khi gõ xong. Để 0 để dùng mặc định.")]
    public float duration; 
    [Tooltip("Camera riêng cho câu thoại này (Nếu có gán thì cam chính sẽ tự tắt)")]
    public GameObject customCamera; 
    [Tooltip("Tên trigger Animation (vd: 'Hello', 'Think') để nhân vật thực hiện")]
    public string animationTrigger;
    [Tooltip("Tick vào đây = Điểm kết thúc: từ câu này trở đi, camera tắt và player được đi lại tự do nhưng thoại vẫn tiếp tục")]
    public bool isStopPoint;
    [Tooltip("Giọng lồng tiếng cho câu này (Có thể để trống)")]
    public AudioClip voiceClip;
}

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    // Sự kiện thông báo khi một câu thoại mới bắt đầu hiện lên
    public static event System.Action<DialogueLine> OnLineStarted;
    // Sự kiện thông báo khi toàn bộ chuỗi thoại kết thúc
    public static event System.Action OnDialogueEnded;

    [Header("UI Elements")]
    public GameObject dialoguePanel; // Panel chứa toàn bộ hộp thoại
    public TextMeshProUGUI dialogueText; // Text hiển thị nội dung thoại

    [Header("Settings")]
    public float typingSpeed = 0.05f; // Tốc độ gõ chữ
    public bool autoClose = true; // Có tự động tắt không?
    public float displayDuration = 3f; // Thời gian hiện chữ trước khi biến mất

    private Queue<DialogueLine> sentences; // Hàng đợi chứa các câu thoại (Tên + Câu)
    private bool isTyping = false; // Kiểm tra xem chữ có đang rớt ra không
    public bool IsTyping => isTyping; // Cho phép script khác check trạng thái typing
    private string currentSentence = ""; // Câu thoại hiện tại đang chạy
    private float currentDuration = 0f; // Thời gian chờ cho câu hiện tại
    private float autoCloseTimer = -1f; // Bộ đếm ngược tự đóng (-1 là không chạy)
    
    // Nguồn phát âm thanh lồng tiếng
    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton pattern để dễ gọi từ các script khác
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        sentences = new Queue<DialogueLine>();
        // Ẩn panel lúc mới vào game
        if(dialoguePanel != null) dialoguePanel.SetActive(false); 
        
        // Cố gắng lấy hoặc thêm sẵn AudioSource để phát giọng nói
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        // Xử lý bộ đếm ngược tự đóng bảng / chuyển câu
        if (autoCloseTimer > 0)
        {
            autoCloseTimer -= Time.deltaTime;
            if (autoCloseTimer <= 0)
            {
                autoCloseTimer = -1f; // Dừng đếm
                
                // Nếu còn câu tiếp theo thì tự chuyển, nếu hết rồi thì đóng bảng
                if (sentences != null && sentences.Count > 0)
                {
                    DisplayNextSentence();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    // Cách 1: Bắt đầu thoại với 1 người duy nhất (Tương thích với các script cũ)
    public void StartDialogue(string[] newSentences)
    {
        if (dialoguePanel == null)
        {
            Debug.LogError("[DialogueSystem] Hộp thoại bị trống, vui lòng gán Dialogue Panel vào!");
            return;
        }

        EnsureCanvasActive();

        sentences.Clear();
        foreach (string sentence in newSentences)
        {
            sentences.Enqueue(new DialogueLine { sentence = sentence, duration = 0 });
        }

        DisplayNextSentence();
    }

    // Cách 2: Bắt đầu thoại với nhiều người nói khác nhau (Cho cutscene đối đáp)
    public void StartDialogueWithLines(DialogueLine[] lines)
    {
        if (dialoguePanel == null) return;

        EnsureCanvasActive();

        sentences.Clear();
        foreach (DialogueLine line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    private void EnsureCanvasActive()
    {
        // Tự động tìm và bật "Mái nhà" Canvas nếu người chơi lỡ tắt nó đi ngoài màn hình
        Canvas parentCanvas = dialoguePanel.GetComponentInParent<Canvas>(true);
        if (parentCanvas != null)
        {
            parentCanvas.gameObject.SetActive(true);
        }
        dialoguePanel.SetActive(true);
    }

    public void DisplayNextSentence()
    {
        // Khi người chơi tương tác, dừng bộ đếm tự đóng hiện tại
        autoCloseTimer = -1f;

        // Nếu chữ đang chạy mà người chơi bấm tiếp, cho hiển thị luôn toàn bộ câu
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentSentence;
            isTyping = false;
            
            // Hiện xong câu rồi thì bắt đầu đếm ngược để tắt/chuyển câu
            if (audioSource != null && audioSource.clip != null)
            {
                if (audioSource.isPlaying)
                {
                    autoCloseTimer = (audioSource.clip.length - audioSource.time) + 0.2f;
                }
                else
                {
                    autoCloseTimer = 0.5f;
                }
                
                if (currentDuration > 0) autoCloseTimer = currentDuration;
            }
            else
            {
                autoCloseTimer = currentDuration > 0 ? currentDuration : displayDuration;
            }
            return;
        }

        // Hết câu thoại thì tắt panel
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Lấy câu tiếp theo (bao gồm cả Tên người nói và Nội dung)
        DialogueLine currentLine = sentences.Dequeue();
        currentSentence = currentLine.sentence;
        currentDuration = currentLine.duration;

        // Phát sự kiện để các script khác (như TalkToNPC) biết mà đổi góc cam
        OnLineStarted?.Invoke(currentLine);

        // Phát giọng lồng tiếng nếu có
        if (audioSource != null)
        {
            audioSource.Stop(); // Ngừng câu nói cũ
            if (currentLine.voiceClip != null)
            {
                audioSource.clip = currentLine.voiceClip;
                audioSource.Play();
            }
            else
            {
                audioSource.clip = null; // Cần thêm dòng này để clear clip cũ
            }
        }

        // Dừng chữ đang chạy (nếu có) và bắt đầu chạy chữ câu mới
        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    // Coroutine chạy hiệu ứng type writer
    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;

        // Gõ xong hết chữ rồi thì bắt đầu đếm ngược để tự đóng bảng/chuyển câu
        if (audioSource != null && audioSource.clip != null)
        {
            if (audioSource.isPlaying)
            {
                // Nếu âm thanh vẫn đang phát, chờ tới khi nó phát xong (cộng thêm 0.2s cho đỡ gắt)
                autoCloseTimer = (audioSource.clip.length - audioSource.time) + 0.2f;
            }
            else
            {
                // Âm thanh đã phát xong trước cả khi gõ chữ xong -> Đợi 0.5s rồi chuyển qua câu tiếp
                autoCloseTimer = 0.5f;
            }

            if (currentDuration > 0) autoCloseTimer = currentDuration;
        }
        else
        {
            autoCloseTimer = currentDuration > 0 ? currentDuration : displayDuration;
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        autoCloseTimer = -1f; // Tắt bộ đếm
        isTyping = false;
        if(dialoguePanel != null) dialoguePanel.SetActive(false);
        
        // Thông báo cho các script khác biết là đã hết thoại
        OnDialogueEnded?.Invoke();
    }
}
