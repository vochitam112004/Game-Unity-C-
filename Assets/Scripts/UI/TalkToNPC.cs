using UnityEngine;
using UnityEngine.Playables; 
using System.Collections.Generic;
using System.Linq;

public class TalkToNPC : MonoBehaviour
{
    [Header("Thông tin NPC")]
    [Tooltip("Tên của NPC sẽ hiển thị trên bảng thoại")]
    public string npcName = "Lý Thông";

    [Header("Cài đặt khoảng cách")]
    [Tooltip("Khoảng cách sát gần để hiện nút E và xem phim")]
    public float interactDistance = 3f;

    private Transform playerTransform;
    private bool hasTriggeredInteract = false;    
    internal bool isWaitingForBoss = false;        
    private bool isBossDefeated = false;          
    private bool isReadyForPostFight = false;     // Phục vụ Phase 2: Thoại khi quay lại range
    private LookController playerLookController;  

    [Header("Cutscene (Tùy chọn)")]
    public PlayableDirector cutsceneDirector; 
    public GameObject cutsceneCamera; 

    private LookController lookController; 
    private Animator npcAnimator; 
    private Animator playerAnimator; 

    [Header("Kịch Bản Chính")]
    [Tooltip("Chuỗi hội thoại tự khởi động khi vào tầm (Trước khi dừng lại ở stopPoint)")]
    public DialogueLine[] cutsceneDialogues;

    [Header("Kịch Bản Sau Khi Boss Chết")]
    [Tooltip("Nếu nhập thoại ở đây, hệ thống sẽ ưu tiên dùng mảng này làm Post-fight thay vì cắt đôi mảng cutsceneDialogues.")]
    public DialogueLine[] afterBossDeadDialogues;

    [Header("Thoại Lặp Lại (Ấn E Sau Toàn Bộ Sự Kiện)")]
    public DialogueLine[] repeatingDialogues;

    [Header("Liên Kết Boss")]
    [Tooltip("Kéo thả GameObject có script TalkToBoss của Chằn Tinh vào đây để ĐẢM BẢO KHÔNG BỊ NHẦM Sói.")]
    public TalkToBoss targetBoss;

    [Tooltip("Tên bossName của Boss. Chỉ dùng khi ô bên trên bị Rỗng.")]
    public string targetBossName = "Chằn Tinh";

    [Header("Giao Diện & Chức Năng Cốt Truyện")]
    public GameObject interactUI; 
    [Tooltip("Tự động hồi đầy máu Thạch Sanh sau đoạn Stop Point (trước khi đánh Boss)")]
    public bool healPlayerAfterStopPoint = true;

    private GameObject currentActiveCamera;
    private bool savedAutoClose = true;

    // Các biến phụ để phân tách Kịch bản 1 (Trước Stop Point) và Kịch bản 2 (Sau Stop Point)
    private DialogueLine[] preFightLines;
    private DialogueLine[] postFightLines;

    private void Start()
    {
        if (lookController == null) lookController = GetComponent<LookController>();
        if (interactUI != null) interactUI.SetActive(false);
        if (cutsceneCamera != null) cutsceneCamera.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) 
        {
            playerTransform = player.transform;
            if (playerAnimator == null) playerAnimator = player.GetComponent<Animator>();

            LookController pLook = player.GetComponent<LookController>();
            if (pLook != null) pLook.canLook = false;
        }

        if (npcAnimator == null) npcAnimator = GetComponent<Animator>();

        if (cutsceneDirector == null)
        {
            cutsceneDirector = GetComponent<PlayableDirector>();
            if (cutsceneDirector == null) cutsceneDirector = GetComponentInChildren<PlayableDirector>();
        }

        if (cutsceneDirector != null) cutsceneDirector.playOnAwake = false;
        if (lookController != null) lookController.canLook = false;

        // --- ĐẤU NỐI SỰ KIỆN BOSS CHẾT ---
        if (targetBoss != null)
        {
            targetBoss.onDeathSequenceEnded.AddListener(PlayPostFightDialogues);
            Debug.Log($"[TalkToNPC] Đã kết nối TRỰC TIẾP với Boss '{targetBoss.bossName}' cho Post-Fight.");
        }
        else
        {
            TalkToBoss[] bosses = FindObjectsByType<TalkToBoss>(FindObjectsSortMode.None);
            foreach (var b in bosses)
            {
                if (string.IsNullOrEmpty(targetBossName) || b.bossName == targetBossName)
                {
                    b.onDeathSequenceEnded.AddListener(PlayPostFightDialogues);
                    Debug.Log($"[TalkToNPC] Đã tự động kết nối với Boss '{b.bossName}' để nối tiếp hội thoại.");
                    break; // Chỉ lắng nghe 1 boss
                }
            }
        }

        // Phân tách hội thoại Tự Động thông qua isStopPoint
        SplitDialogues();
    }

    private void SplitDialogues()
    {
        // 1. Nếu đã nhập riêng thoại Post-fight tại afterBossDeadDialogues thì ưu tiên dùng luôn
        if (afterBossDeadDialogues != null && afterBossDeadDialogues.Length > 0)
        {
            preFightLines = cutsceneDialogues;
            postFightLines = afterBossDeadDialogues;
            return;
        }

        // 2. Backup: Chia tách mảng thông qua checks isStopPoint giống trước đó
        if (cutsceneDialogues == null || cutsceneDialogues.Length == 0) return;

        List<DialogueLine> pre = new List<DialogueLine>();
        List<DialogueLine> post = new List<DialogueLine>();
        bool foundStopPoint = false;

        foreach(var line in cutsceneDialogues)
        {
            var modifiedLine = line;
            if (string.IsNullOrEmpty(modifiedLine.name)) modifiedLine.name = npcName;

            if (!foundStopPoint)
            {
                pre.Add(modifiedLine);
                if (modifiedLine.isStopPoint) foundStopPoint = true;
            }
            else
            {
                post.Add(modifiedLine);
            }
        }

        preFightLines = pre.ToArray();
        postFightLines = post.ToArray();
    }

    private void Update()
    {
        if (playerTransform == null) return;
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (cutsceneDirector != null && cutsceneDirector.state == PlayState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.E) && DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
                DialogueSystem.Instance.DisplayNextSentence();
            return;
        }

        // Tự động nói chuyện ngay khi vào tầm (Đang ở Phase 1)
        if (distance <= interactDistance && !hasTriggeredInteract && !isWaitingForBoss && !isBossDefeated && !isReadyForPostFight)
        {
            hasTriggeredInteract = true;
            if (interactUI != null) interactUI.SetActive(false);
            StartCutscene(preFightLines);
        }

        // Phase 2: Tự động phát thoại nối tiếp (Post-fight) khi QUAY TRỞ VỀ tầm đứng của NPC sau khi Boss chết
        else if (distance <= interactDistance && isReadyForPostFight && !isBossDefeated)
        {
            isReadyForPostFight = false;
            isBossDefeated = true; // Đánh dấu hoàn tất
            if (interactUI != null) interactUI.SetActive(false);
            StartCutscene(postFightLines);
        }
        // Phase 3: Bấm E để nói chuyện lặp lại sau khi Boss chết và hội thoại kết thúc
        else if (distance <= interactDistance && isBossDefeated && !DialogueSystem.Instance.dialoguePanel.activeSelf)
        {
            if (repeatingDialogues != null && repeatingDialogues.Length > 0)
            {
                if (interactUI != null && !interactUI.activeSelf) interactUI.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (interactUI != null) interactUI.SetActive(false);
                    // Dùng StartCutscene để giữ MutualLook và cam cinematic nếu có
                    StartCutscene(repeatingDialogues);
                }
            }
        }
        else if (distance > interactDistance)
        {
            if (interactUI != null && interactUI.activeSelf) interactUI.SetActive(false);
        }

        if (hasTriggeredInteract && DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.E)) DialogueSystem.Instance.DisplayNextSentence();
        }
    }

    private void StartCutscene(DialogueLine[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0) return;

        if (cutsceneCamera != null) cutsceneCamera.SetActive(true);
        EnableMutualLook();
        DialogueSystem.OnLineStarted += HandleCinematicStep;
        
        savedAutoClose = DialogueSystem.Instance.autoClose;
        DialogueSystem.Instance.autoClose = true;
        
        DialogueSystem.Instance.StartDialogueWithLines(dialogues);

        if (cutsceneDirector != null && cutsceneDirector.state != PlayState.Playing)
        {
            cutsceneDirector.Play();
            StartCoroutine(WaitCutsceneFinish());
        }
        else
        {
            DialogueSystem.OnDialogueEnded += CleanupCinematicState;
        }
    }

    private void EnableMutualLook()
    {
        if (lookController != null) 
        {
            lookController.target = playerTransform;
            lookController.canLook = true;
        }

        if (playerTransform != null)
        {
            playerLookController = playerTransform.GetComponent<LookController>();
            if (playerLookController == null) playerLookController = playerTransform.gameObject.AddComponent<LookController>();
            playerLookController.target = transform;
            playerLookController.canLook = true;
        }
    }

    private void DisableMutualLook()
    {
        if (lookController != null) lookController.target = null;
        if (playerLookController != null) playerLookController.target = null;
    }

    private void CleanupCinematicState()
    {
        DisableMutualLook();
        DialogueSystem.OnLineStarted -= HandleCinematicStep;
        DialogueSystem.OnDialogueEnded -= CleanupCinematicState;

        if (currentActiveCamera != null)
        {
            currentActiveCamera.SetActive(false);
            currentActiveCamera = null;
        }

        if (DialogueSystem.Instance != null) DialogueSystem.Instance.autoClose = savedAutoClose;

        // Xong Phase 1: Chuẩn bị đánh Boss
        if (!isWaitingForBoss && !isBossDefeated)
        {
            isWaitingForBoss = true;
            hasTriggeredInteract = false; 
            
            if (cutsceneDirector != null) cutsceneDirector.Pause(); // Pause timeline phim

            // Hồi đầy máu
            if (healPlayerAfterStopPoint && playerTransform != null)
            {
                PlayerHealth ph = playerTransform.GetComponent<PlayerHealth>();
                if (ph != null) ph.FullHeal();
            }
        }
        // Xong Phase 2: Đã xem xong thoại nối tiếp sau Boss chết
        else if (isBossDefeated)
        {
            hasTriggeredInteract = false; 
            if (cutsceneDirector != null) cutsceneDirector.Resume(); // Nếu timeline còn thì chạy nốt
        }
    }

    private System.Collections.IEnumerator WaitCutsceneFinish()
    {
        yield return null;
        while (cutsceneDirector != null && cutsceneDirector.state == PlayState.Playing) yield return null;
        while (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf) yield return null;
        CleanupCinematicState();
    }

    private void HandleCinematicStep(DialogueLine line)
    {
        GameObject targetCam = null;
        if (line.customCamera != null) targetCam = line.customCamera;
        else if (cutsceneCamera != null) targetCam = cutsceneCamera;

        if (targetCam != null && currentActiveCamera != targetCam)
        {
            if (currentActiveCamera != null) currentActiveCamera.SetActive(false);
            targetCam.SetActive(true);
            currentActiveCamera = targetCam;
        }
    }

    // --- HÀM NÀY SẼ ĐƯỢC BOSS KHÓA MỤC TIÊU VÀ GỌI KHI NÓ NGỎM ---
    public void PlayPostFightDialogues()
    {
        if (isBossDefeated) return;
        isWaitingForBoss = false;
        isReadyForPostFight = true; // Kích hoạt cờ sẵn sàng cho Phase 2 khi lại gần
        Debug.Log("[TalkToNPC] Boss đã ngã. Chờ người chơi quay trở lại gần để phát cuộc thoại nối tiếp.");
    }
}
