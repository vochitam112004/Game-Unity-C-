using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkToBoss : MonoBehaviour
{
    [Header("Thông tin Boss")]
    public string bossName = "Chằn Tinh";

    [Header("1. Thoại Mở Màn (Trước khi đánh)")]
    [Tooltip("Danh sách các câu Boss sẽ nói MỘT LẦN DUY NHẤT trước khi lao vào đánh nhau")]
    public DialogueLine[] tauntDialogues;

    [Header("2. Thoại khi bị trúng đòn")]
    [Tooltip("Các câu chửi bới khi bị chém trúng")]
    public DialogueLine[] hurtDialogues;

    [Header("3. Thoại lúc gục ngã (Chết)")]
    [Tooltip("Câu trăn trối cuối cùng")]
    public DialogueLine[] deathDialogues;

    [Header("Cài đặt hiển thị")]
    [Tooltip("Thời gian xuất hiện mỗi câu chữ")]
    public float displayDuration = 3f;

    private bool isFighting = false;
    private bool isDead = false;

    // Lưu trữ thông số cũ để dọn dẹp sau khi nói
    private float savedDisplayDuration;
    private bool savedAutoClose;

    // --- QUẢN LÝ NHẠC NỀN (BGM) ---
    private static int activeCombatants = 0;
    private static AudioSource bgmSource;
    // --------------------------------

    [Header("Sự kiện kết thúc")]
    [Tooltip("Kéo thả code của Lý Thông (TalkToNPC -> PlayPostFightDialogues) vào đây để tự động nối tiếp thoại sau khi Boss chết")]
    public UnityEngine.Events.UnityEvent onDeathSequenceEnded;

    private void Start()
    {
        // Không cần làm gì ở Start nữa
    }

    private void Update()
    {
        // Không dùng Update để lảm nhảm theo thời gian nữa
    }

    // ==========================================
    // CÁC HÀM CỔNG: ĐỂ SCRIPT MÁU/ĐÁNH GỌI VÀO
    // ==========================================

    private AudioSource FindBGMAudioSource()
    {
        // 1. Tìm GameObject theo các tên phổ biến để tránh nhận diện sai SFX khác
        string[] bgmNames = { "BGM", "BackgroundMusic", "Music", "Background_Music", "NhacNen" };
        foreach (string name in bgmNames)
        {
            GameObject bgmObj = GameObject.Find(name);
            if (bgmObj != null)
            {
                AudioSource src = bgmObj.GetComponent<AudioSource>();
                if (src != null) return src;
            }
        }
        return null; // Không đoán bừa tránh tắt nhầm tiếng quái/vũ khí
    }

    // Gọi hàm này khi bắt đầu bước vào khu vực Boss
    public void StartBossFight()
    {
        if (isFighting) return; // Đảm bảo chỉ gọi 1 lần lúc mới gặp
        isFighting = true;

        // --- QUẢN LÝ NHẠC NỀN ---
        if (bgmSource == null) bgmSource = FindBGMAudioSource();
        if (bgmSource != null)
        {
            if (activeCombatants == 0)
            {
                bgmSource.Stop();
                Debug.Log("<color=cyan>[BGM] Đã DỪNG nhạc nền vì có quái/boss tấn công!</color>");
            }
            activeCombatants++;
        }
        // ------------------------

        if (tauntDialogues.Length > 0 && DialogueSystem.Instance != null && !isDead)
        {
            // Lưu lại thiết lập cũ
            savedAutoClose = DialogueSystem.Instance.autoClose;
            savedDisplayDuration = DialogueSystem.Instance.displayDuration;
            
            // Ép tự đóng và thời lượng hiển thị
            DialogueSystem.Instance.autoClose = true;
            DialogueSystem.Instance.displayDuration = displayDuration;

            // Bắt đầu chuỗi thoại mở màn
            DialogueSystem.Instance.StartDialogueWithLines(tauntDialogues);
            DialogueSystem.OnDialogueEnded += RestoreDialogueSettings;
        }
    }

    // Gọi hàm này khi Boss vừa bị trừ máu (Trúng búa/rìu/đạn)
    public void TakeHit()
    {
        if (isDead) return;

        if (hurtDialogues.Length > 0)
        {
            DialogueLine line = hurtDialogues[Random.Range(0, hurtDialogues.Length)];
            ForceSpeak(line);
        }
    }

    // Gọi hàm này khi HP <= 0 (Die)
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // --- QUẢN LÝ NHẠC NỀN ---
        if (isFighting)
        {
            activeCombatants = Mathf.Max(0, activeCombatants - 1);
            isFighting = false;

            if (activeCombatants == 0 && bgmSource != null)
            {
                bgmSource.Play();
                Debug.Log("<color=cyan>[BGM] PHÁT LẠI nhạc nền vì hết quái/boss!</color>");
            }
        }
        else
        {
            isFighting = false;
        }
        // ------------------------

        if (deathDialogues.Length > 0 && DialogueSystem.Instance != null)
        {
            savedAutoClose = DialogueSystem.Instance.autoClose;
            savedDisplayDuration = DialogueSystem.Instance.displayDuration;

            DialogueSystem.Instance.autoClose = true;
            DialogueSystem.Instance.displayDuration = 4.5f; // Trăn trối lâu hơn chút

            DialogueSystem.Instance.StartDialogueWithLines(deathDialogues);
            DialogueSystem.OnDialogueEnded += TriggerOnDeathEnded;
        }
        else
        {
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
            {
                DialogueSystem.Instance.EndDialogue();
            }
            TriggerOnDeathEnded();
        }
    }

    private void TriggerOnDeathEnded()
    {
        DialogueSystem.OnDialogueEnded -= TriggerOnDeathEnded;
        RestoreDialogueSettings();
        
        // Kích hoạt nối mạch hội thoại
        if (onDeathSequenceEnded != null)
        {
            onDeathSequenceEnded.Invoke();
        }
    }

    // ==========================================
    // LOGIC XỬ LÝ HỘI THOẠI 
    // ==========================================

    // Xóa hàm TriggerRandomTaunt cũ đi vì chuyển thành thoại mở màn rồi

    // Hàm nói đè: Phá bỏ mọi đoạn hội thoại đang chạy để bắt đầu câu mới ngay tức khắc
    private void ForceSpeak(DialogueLine line, float overrideDuration = -1f)
    {
        if (DialogueSystem.Instance == null) return;

        // Nếu có thằng nào đang chiếm Panel, dẹp nó đi.
        if (DialogueSystem.Instance.dialoguePanel.activeSelf)
        {
            DialogueSystem.Instance.EndDialogue();
        }

        // Ép thời gian hiển thị & auto close của Dialogue System
        savedAutoClose = DialogueSystem.Instance.autoClose;
        savedDisplayDuration = DialogueSystem.Instance.displayDuration;

        DialogueSystem.Instance.autoClose = true;
        DialogueSystem.Instance.displayDuration = (overrideDuration > 0) ? overrideDuration : displayDuration;

        DialogueLine[] singleLine = new DialogueLine[] { line };
        DialogueSystem.Instance.StartDialogueWithLines(singleLine);

        // Lắng nghe sự kiện để trả lại thiết lập cũ cho hệ thống sau khi nói xong
        DialogueSystem.OnDialogueEnded += RestoreDialogueSettings;
    }

    private void RestoreDialogueSettings()
    {
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.autoClose = savedAutoClose;
            DialogueSystem.Instance.displayDuration = savedDisplayDuration;
        }

        DialogueSystem.OnDialogueEnded -= RestoreDialogueSettings;
    }


}
