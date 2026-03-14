using UnityEngine;
using UnityEngine.Playables; 

public class TalkToLyThong : MonoBehaviour
{
    [Header("Cài đặt khoảng cách")]
    [Tooltip("Khoảng cách lúc ở xa để Lý Thông tự động nói chuyện")]
    public float autoTalkDistance = 8f;
    [Tooltip("Khoảng cách tiến sát vào để hiện nút bấm E và xem rạp chiếu phim")]
    public float interactDistance = 3f;

    private Transform playerTransform;
    private bool hasAutoSpoken = false;           // Biến đánh dấu Lý Thông đã tự mở lời chưa
    private bool hasTriggeredInteract = false;    // Biến đánh dấu đã kích hoạt hội thoại sát gần chưa
    private bool autoTalkFinished = false;        // Biến đánh dấu thoại tự động đã KẾT THÚC hoàn toàn
    private bool savedAutoClose = true;           // Lưu lại giá trị autoClose gốc trước khi cutscene
    private bool isReleasedByStopPoint = false;   // Đã qua điểm kết thúc → player đi tự do, thoại vẫn chạy
    private bool hasCutscenePlayed = false;       // Cutscene đã diễn xong → dùng thoại afterCutscene
    private float savedDisplayDuration = 3f;      // Lưu displayDuration gốc để khôi phục sau auto-talk
    private LookController playerLookController;  // Của người chơi

    [Header("Cutscene (Tùy chọn)")]
    [Tooltip("Kéo thả object chứa Timeline Cutscene vào đây")]
    public PlayableDirector cutsceneDirector; 

    [Tooltip("Kéo GameObject Camera góc quay phim (Cutscene Camera) vào đây")]
    public GameObject cutsceneCamera; 

    private LookController lookController; // Để Lý Thông nhìn Player
    private Animator npcAnimator; // Để điều khiển hành động Lý Thông
    private Animator playerAnimator; // Để điều khiển hành động Player

    [Header("1. Thoại Tự Động (Lúc đứng từ xa — Lần đầu tiên)")]
    [TextArea(3, 10)]
    public string[] autoTalkDialogues;

    [Header("1b. Thoại Tự Động (Sau khi Cutscene đã diễn xong)")]
    [Tooltip("Vd: 'Làm xong việc chưa mà về đây' — hiện khi player quay lại sau khi đã xem cutscene")]
    [TextArea(3, 10)]
    public string[] afterCutsceneDialogues;

    [Tooltip("Thời gian hiện mỗi câu auto-talk trước khi tự động săng sang câu tiếp (giây)")]
    public float autoTalkDuration = 3f;

    [Header("2. Thoại Trong Phim (Khi nhấn E / Diễn Cutscene)")]
    [Tooltip("Dùng cái này để có sự đối đáp giữa Player và Lý Thông")]
    public DialogueLine[] cutsceneDialogues;
    
    [Header("Giao Diện")]
    [Tooltip("Kéo thả nút hiển thị 'Nhấn phím E' (vd: một Panel Text) vào ô này")]
    public GameObject interactUI; // GameObject chứa nút báo bấm E

    private void Start()
    {
        // Tự động tìm LookController trên cùng Object này
        if (lookController == null) lookController = GetComponent<LookController>();

        // Tắt nút bấm E lúc mới vào game đi cho chắc chắn
        if (interactUI != null) interactUI.SetActive(false);

        // Tự động tìm nhân vật người chơi trong Scene (phải tag là Player)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) 
        {
            playerTransform = player.transform;
            if (playerAnimator == null) playerAnimator = player.GetComponent<Animator>();

            // Tắt LookController của Player nếu có sẵn lúc bắt đầu game
            LookController pLook = player.GetComponent<LookController>();
            if (pLook != null) pLook.canLook = false;
        }

        // Tự động tìm Animator của Lý Thông nếu chưa gán
        if (npcAnimator == null) npcAnimator = GetComponent<Animator>();

        // Mặc định tắt việc nhìn để không bị xoay lung tung lúc chưa vào phim
        if (lookController != null) lookController.canLook = false;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Tính khoảng cách từ Lý Thông tới Người chơi
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 0. Kiểm tra phạm vi khi đang nói chuyện/xem phim
        // Bỏ qua nếu đã qua điểm kết thúc (đã được giải phóng) — player được đi tự do
        if (hasTriggeredInteract && !isReleasedByStopPoint)
        {
            // Nếu đi quá xa (ví dụ: quá khoảng cách tự động nói chuyện + 2m) thì hủy ngang
            if (distance > autoTalkDistance + 2f)
            {
                ForceEndInteraction();
                return;
            }
        }

        // Nếu đang trong quá trình diễn Timeline thì không chạy tiếp logic hội thoại tự động bên dưới
        if (cutsceneDirector != null && cutsceneDirector.state == PlayState.Playing)
        {
            // NHƯNG vẫn xử lý phím E để người chơi chuyển câu / skip chữ đang gõ
            if (Input.GetKeyDown(KeyCode.E) && DialogueSystem.Instance != null
                && DialogueSystem.Instance.dialoguePanel.activeSelf)
            {
                DialogueSystem.Instance.DisplayNextSentence();
            }
            return;
        }

        // ==========================================
        // 1. VÒNG NGOÀI: TỰ ĐỘNG NÓI CHUYỆN KHI VỪA CẬP BẾN
        // ==========================================
        if (distance <= autoTalkDistance && !hasAutoSpoken)
        {
            if (DialogueSystem.Instance != null)
            {
                // Chọn thoại phù hợp: nếu cutscene đã diễn rồi thì dùng afterCutsceneDialogues
                string[] linesToPlay = (hasCutscenePlayed && afterCutsceneDialogues.Length > 0)
                    ? afterCutsceneDialogues
                    : autoTalkDialogues;

                if (linesToPlay.Length > 0)
                {
                    // Lưu và ép autoClose + displayDuration
                    savedAutoClose = DialogueSystem.Instance.autoClose;
                    savedDisplayDuration = DialogueSystem.Instance.displayDuration;
                    DialogueSystem.Instance.autoClose = true;
                    DialogueSystem.Instance.displayDuration = autoTalkDuration;

                    DialogueSystem.Instance.StartDialogue("Lý Thông", linesToPlay);
                    DialogueSystem.OnDialogueEnded += OnAutoTalkEnded;
                }
            }
            hasAutoSpoken = true;
        }

        // Vừa ra khỏi vùng auto-talk → TẮT DIALOGUE NGAY (không đợi timer duration)
        if (distance > autoTalkDistance && hasAutoSpoken && !hasTriggeredInteract)
        {
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
            {
                DialogueSystem.Instance.EndDialogue();
            }
        }

        // Ra xa hơn +3m → reset state để lần sau vào lại sẽ nói lại
        if (distance > autoTalkDistance + 3f)
        {
            hasAutoSpoken = false;
            hasTriggeredInteract = false;
            autoTalkFinished = false;

            // RESET việc nhìn khi đi quá xa
            if (lookController != null) 
            {
                lookController.target = null;
                lookController.canLook = false;
            }
            if (playerLookController != null) 
            {
                playerLookController.target = null;
                playerLookController.canLook = false;
            }
        }

        // ==========================================
        // VÒNG TRONG: ĐỨNG SÁT VÀO ĐỂ XEM PHIM / BẤM E
        // ==========================================
        if (distance <= interactDistance)
        {
            if (!hasTriggeredInteract)
            {
                if (!hasCutscenePlayed)
                {
                    // ===== LẦN ĐẦU TIÊN: VÀO VÙNG → CẮT AUTO-TALK, CUTSCENE NGAY =====
                    hasTriggeredInteract = true;
                    if (interactUI != null) interactUI.SetActive(false);
                    // Dừng auto-talk ngay (nếu còn đang chạy) để vào cutscene
                    if (DialogueSystem.Instance != null) DialogueSystem.Instance.EndDialogue();

                        if (cutsceneDirector != null)
                        {
                            if (cutsceneDirector.state != PlayState.Playing)
                            {
                                if (cutsceneCamera != null) cutsceneCamera.SetActive(true);
                                EnableMutualLook();
                                DialogueSystem.OnLineStarted += HandleCinematicStep;
                                cutsceneDirector.Play();
                                if (cutsceneDialogues.Length > 0 && DialogueSystem.Instance != null)
                                {
                                    // Ép autoClose = true để tự động chuyển câu theo Duration
                                    savedAutoClose = DialogueSystem.Instance.autoClose;
                                    DialogueSystem.Instance.autoClose = true;
                                    DialogueSystem.Instance.StartDialogueWithLines(cutsceneDialogues);
                                }
                                StartCoroutine(WaitCutsceneFinish());
                            }
                        }
                        else if (cutsceneDialogues.Length > 0 && DialogueSystem.Instance != null)
                        {
                            if (cutsceneCamera != null) cutsceneCamera.SetActive(true);
                            EnableMutualLook();
                            DialogueSystem.OnLineStarted += HandleCinematicStep;
                            // Ép autoClose = true để tự động chuyển câu theo Duration
                            savedAutoClose = DialogueSystem.Instance.autoClose;
                            DialogueSystem.Instance.autoClose = true;
                        DialogueSystem.Instance.StartDialogueWithLines(cutsceneDialogues);
                            DialogueSystem.OnDialogueEnded += CleanupCinematicState;
                        }
                }
                else
                {
                    // ===== SAU CUTSCENE: không trigger gì trong zone =====
                    // afterCutsceneDialogue sẽ phát qua auto-talk zone
                    // khi player rời zone (> autoTalkDistance+3f) rồi vào lại
                    // (không đặt hasTriggeredInteract = true để không lock player)
                }
            }
            else
            {
                // Đã kích hoạt rồi, nhấn E để chuyển câu tiếp theo
                if (Input.GetKeyDown(KeyCode.E) && DialogueSystem.Instance != null)
                    DialogueSystem.Instance.DisplayNextSentence();
            }
        }
        else
        {
            // Đi dạt ra xa khỏi vùng xem phim thì TẮT nút bấm E đi
            if (interactUI != null && interactUI.activeSelf) interactUI.SetActive(false);
        }

        // ==========================================
        // PHÍM E CHUNG: Chuyển câu khi đang ở vòng ngoài (nếu đang nói chuyện tự động)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.E) && hasAutoSpoken && !hasTriggeredInteract)
        {
            if (DialogueSystem.Instance != null) 
            {
                DialogueSystem.Instance.DisplayNextSentence();
            }
        }

        // ==========================================
        // PHÍM E CHUNG: DÙNG ĐỂ CHUYỂN CÂU THOẠI KHI ĐỨNG XA (NGOÀI VÙNG CUTSCENE)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.E) && distance > interactDistance)
        {
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
            {
                DialogueSystem.Instance.DisplayNextSentence();
            }
        }
    }

    // Hàm bật việc nhìn nhau (Mutual Look)
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

    // Hàm tắt việc nhìn nhau (Mutual Look)
    private void DisableMutualLook()
    {
        if (lookController != null) 
        {
            lookController.target = null;
            lookController.canLook = false;
        }
        if (playerLookController != null) 
        {
            playerLookController.target = null;
            playerLookController.canLook = false;
        }

        // Hủy đăng ký các sự kiện (để không bị thừa thãi)
        DialogueSystem.OnDialogueEnded -= DisableMutualLook;
    }

    // Callback khi thoại tự động kết thúc
    private void OnAutoTalkEnded()
    {
        autoTalkFinished = true;
        if (DialogueSystem.Instance != null)
        {
            // Khôi phục displayDuration và autoClose về giá trị gốc
            DialogueSystem.Instance.displayDuration = savedDisplayDuration;
            DialogueSystem.Instance.autoClose = savedAutoClose;
        }
        DialogueSystem.OnDialogueEnded -= OnAutoTalkEnded;
    }

    // Coroutine safeguard đóng auto-talk dialogue — bất kể autoClose có bật hay không
    // lineCount: số câu trong mảng auto-talk đang chạy
    private System.Collections.IEnumerator ForceCloseAutoTalk(int lineCount)
    {
        for (int i = 0; i < lineCount; i++)
        {
            // Chờ đến khi đang gõ chữ (câu vừa bắt đầu)
            yield return null;

            // Chờ đến khi gõ xong hết chữ của câu hiện tại
            while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsTyping)
                yield return null;

            // Đợi đúng autoTalkDuration giây
            yield return new WaitForSeconds(autoTalkDuration);

            // Nếu dialogue vẫn còn hiện, force advance (sang câu tiếp hoặc đóng)
            if (DialogueSystem.Instance == null || !DialogueSystem.Instance.dialoguePanel.activeSelf)
                yield break; // Đã đóng bởi cơ chế khác, dừng lại

            DialogueSystem.Instance.DisplayNextSentence();
        }
    }

    // Hàm dọn dẹp trạng thái khi kết thúc thoại
    private void CleanupCinematicState()
    {
        DisableMutualLook();
        
        // Hủy đăng ký để tránh bị gọi lại khi không cần thiết
        DialogueSystem.OnLineStarted -= HandleCinematicStep;
        DialogueSystem.OnDialogueEnded -= CleanupCinematicState;

        // Tắt camera đang active nếu là camera cutscene
        if (currentActiveCamera != null)
        {
            currentActiveCamera.SetActive(false);
            currentActiveCamera = null;
        }

        // Reset cờ để có thể nói chuyện lại nếu nhấn E
        hasTriggeredInteract = false;

        // KHÔI PHỤC cờ sau khi cutscene kết thúc
        autoTalkFinished = false;
        isReleasedByStopPoint = false;
        hasCutscenePlayed = true;
        // Đặt hasAutoSpoken = true để ngăn auto-talk chạy ngay
        // Player phải rời zone (> autoTalkDistance+3f) rồi vào lại mới nghe afterCutsceneDialogue
        hasAutoSpoken = true;
        // Khôi phục autoClose
        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.autoClose = savedAutoClose;
    }

    // Coroutine theo dõi khi nào phim Timeline diễn xong
    private System.Collections.IEnumerator WaitCutsceneFinish()
    {
        // Chờ 1 frame để Timeline kịp đổi trạng thái
        yield return null;

        // Chờ Timeline kết thúc
        while (cutsceneDirector != null && cutsceneDirector.state == PlayState.Playing)
        {
            yield return null;
        }

        // ĐỢI THÊM: Chờ cả Dialogue kết thúc nốt trước khi tắt camera
        // (Tránh camera bị tắt giữa chừng khi Thạch Sanh chưa nói hết câu)
        while (DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf)
        {
            yield return null;
        }

        // RESET dọn dẹp
        CleanupCinematicState();
    }

    // Hàm kết thúc mọi thứ ngay lập tức (dùng khi người chơi chạy mất)
    public void ForceEndInteraction()
    {
        // 1. Dừng Timeline
        if (cutsceneDirector != null) cutsceneDirector.Stop();

        // 2. Dọn dẹp trạng thái chung
        CleanupCinematicState();
    }

    private GameObject currentActiveCamera;

    // Hàm xử lý việc đổi Camera mỗi khi sang câu thoại mới
    private void HandleCinematicStep(DialogueLine line)
    {
        // --- Xử LÝ ĐIỂM KẾT THÚC (STOP POINT) ---
        if (line.isStopPoint && !isReleasedByStopPoint)
        {
            ReleaseCinematicCamera();
            // Không return — vẫn cho hiện câu thoại này bình thường
        }

        // Nếu đã giải phóng (sau stop point) thì không đổi camera nữa
        if (isReleasedByStopPoint) return;

        // --- Xử LÝ CAMERA BÌNH THƯỜNG ---
        GameObject targetCam = null;

        if (line.customCamera != null)
        {
            targetCam = line.customCamera;
        }
        else if (cutsceneCamera != null)
        {
            targetCam = cutsceneCamera;
        }

        // Chỉ switching nếu thực sự có camera mục tiêu
        if (targetCam != null)
        {
            // Nếu camera mục tiêu khác với camera đang hiện tại thì mới đổi
            if (currentActiveCamera != targetCam)
            {
                if (currentActiveCamera != null) currentActiveCamera.SetActive(false);
                
                targetCam.SetActive(true);
                currentActiveCamera = targetCam;
            }
        }
        // Nếu không có camera nào mới (cả custom lẫn mặc định), 
        // thì CỨ GIỮ NGUYÊN camera hiện tại, không tắt đi để tránh bị nhảy về Main Camera.
    }

    // Hàm giải phóng camera cutscene khi gặp Stop Point
    // Player được đi tự do, thoại vẫn tiếp tục chạy
    private void ReleaseCinematicCamera()
    {
        isReleasedByStopPoint = true;

        // Tắt camera cutscene đang active → trả về Main Camera
        if (currentActiveCamera != null)
        {
            currentActiveCamera.SetActive(false);
            currentActiveCamera = null;
        }

        // Tắt mutual look
        DisableMutualLook();
    }
}
