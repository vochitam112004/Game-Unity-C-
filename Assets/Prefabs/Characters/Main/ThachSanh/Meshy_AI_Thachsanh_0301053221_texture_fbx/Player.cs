using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Animator playerAnim;
    public Rigidbody playerRigid;
    public Transform playerTrans;

    [Header("Axe Settings")]
    public GameObject axeHand;
    public GameObject axeBack;

    [Header("Movement Settings")]
    public float walk_speed = 12f;
    public float run_speed = 18f;
    public float back_speed = 8f;
    public float ro_speed = 15f;
    public float roll_speed = 15f;

    [Header("Roll & Physics")]
    public float rollDuration = 2f;
    public float extraGravity = 40f;
    private bool isRolling = false;
    private float rollTimer = 0f;

    private string currentAnim = "idle";
    private bool isWeaponDrawn = false;
    private bool isActing = false;
    public bool isBlocking = false;

    [Header("Combat Settings")]
    public Collider axeHitbox;

    [Header("Audio Settings")]
    public AudioClip[] swingSounds; // m thanh chém
    public AudioClip equipSound;    // m thanh rút/cất vũ khí
    public AudioClip footstepSound; // [MỚI] m thanh bước chân (Chỉ cần 1 file)
    public float walkStepDelay = 0.5f; // [MỚI] Tần suất bước đi bộ
    public float runStepDelay = 0.35f; // [MỚI] Tần suất bước chạy
    public AudioClip rollSound;        // [MỚI] m thanh khi lăn cuộn người
    [Range(0f, 1f)] public float rollVolume = 0.5f; // [MỚI] m lượng tiếng lăn
    private float stepTimer = 0f;      // [MỚI] Bộ đếm thời gian
    private AudioSource audioSource;

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;

        // Thiết lập ban đầu: Rìu ở trên lưng
        isWeaponDrawn = false;
        if (axeHand != null) axeHand.SetActive(false);
        if (axeBack != null) axeBack.SetActive(true);

        DisableHitbox();

        // FALLBACK: Tự động gán playerAnim nếu đang trống
        if (playerAnim == null)
        {
            playerAnim = GetComponentInChildren<Animator>();
            if (playerAnim != null)
            {
                Debug.Log("[Player] Đã tự động tìm và gán playerAnim từ object con: " + playerAnim.gameObject.name);
            }
        }

        // Tự động tìm hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        else
        {
            // Bảo vệ: Ép buộc 2D Sound để không bị đè âm thanh 3D cách xa
            audioSource.spatialBlend = 0f;
        }

        // Tự động thêm PlayerAnimEvents vào object chứa Animator để nhận Animation Event
        if (playerAnim != null)
        {
            PlayerAnimEvents animEvents = playerAnim.GetComponent<PlayerAnimEvents>();
            if (animEvents == null)
            {
                animEvents = playerAnim.gameObject.AddComponent<PlayerAnimEvents>();
            }
            animEvents.playerScript = this;
        }
    }

    void FixedUpdate()
    {
        playerRigid.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        if (isRolling || isActing)
        {
            if (isRolling)
            {
                Vector3 rollVelocity = transform.forward * roll_speed;
                rollVelocity.y = playerRigid.linearVelocity.y;
                playerRigid.linearVelocity = rollVelocity;
            }
            else
            {
                // Dừng di chuyển khi đang act (đánh, rút vũ khí...)
                playerRigid.linearVelocity = new Vector3(0, playerRigid.linearVelocity.y, 0);

                // THÊM MỚI: Khóa nhân vật hướng về phía trước camera khi đang tấn công
                if (currentAnim == "ATK1" || currentAnim == "combo1" || currentAnim == "combo2")
                {
                    if (playerCamera != null)
                    {
                        Vector3 camForward = playerCamera.transform.forward;
                        camForward.y = 0f; // QUAN TRỌNG: Đưa Y về 0 để không bị lỗi nghiêng nhân vật / khóa chiều cao camera
                        camForward.Normalize();

                        if (camForward != Vector3.zero)
                        {
                            // Xoay mượt về phía trước mặt
                            Quaternion targetRotation = Quaternion.LookRotation(camForward);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ro_speed * Time.deltaTime);
                        }
                    }
                }
            }
            return;
        }

        HandleMovement();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = playerAnim.GetCurrentAnimatorStateInfo(0);

        isActing = (currentAnim == "equip" || currentAnim == "unequip" ||
                    currentAnim == "ATK1" || currentAnim == "combo1" || currentAnim == "combo2");

        if (isActing)
        {
            if (stateInfo.IsName(currentAnim))
            {
                if (stateInfo.normalizedTime >= 0.85f)
                {
                    Debug.Log($"[Player] Kết thúc hành động: {currentAnim} (Xong animation)");
                    currentAnim = "idle";
                    DisableHitbox();
                }
            }
            else if (!playerAnim.IsInTransition(0) && stateInfo.IsName("idle"))
            {
                Debug.Log($"[Player] Trả về idle sớm vì Animator phát idle thay vì {currentAnim}. Thường do Trigger bị trượt.");
                currentAnim = "idle";
                DisableHitbox();
            }
            return;
        }

        HandleRoll();

        isBlocking = Input.GetMouseButton(1) && isWeaponDrawn && !isRolling;
        playerAnim.SetBool("block", isBlocking);

        if (isBlocking) DisableHitbox();

        float targetBlend = isWeaponDrawn ? 1f : 0f;
        playerAnim.SetFloat("Blend", targetBlend, 0.1f, Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.E) && !isRolling && !isBlocking)
        {
            isWeaponDrawn = !isWeaponDrawn;
            if (isWeaponDrawn) ChangeAnimation("equip");
            else ChangeAnimation("unequip");
            return;
        }

        if (isWeaponDrawn && !isRolling && !isBlocking)
        {
            if (Input.GetMouseButtonDown(0)) { Debug.Log("[Player] CLICK TẤN CÔNG (Chuột trái)"); ChangeAnimation("ATK1"); return; }
            if (Input.GetKeyDown(KeyCode.Q)) { ChangeAnimation("combo1"); return; }
            if (Input.GetKeyDown(KeyCode.F)) { ChangeAnimation("combo2"); return; }
        }

        if (!isRolling && !isBlocking)
        {
            HandleAnimations();
        }

        HandleFootsteps(); // [MỚI] Tính toán bước chân mỗi frame
    }

    void LateUpdate()
    {
        if (playerTrans != null && playerTrans.childCount > 0)
        {
            playerTrans.GetChild(0).localRotation = Quaternion.identity;
        }
    }

    void HandleMovement()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * moveZ + right * moveX).normalized;

        bool isCutsceneActive = DialogueSystem.Instance != null && DialogueSystem.Instance.dialoguePanel.activeSelf;
        if (!isCutsceneActive && moveDirection.magnitude >= 0.1f)
        {
            // Chế độ Strafe khi đang thủ rìu, Chế độ tự do khi chạy bình thường
            Vector3 faceDirection = isBlocking ? forward : moveDirection;

            Quaternion targetRotation = Quaternion.LookRotation(faceDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ro_speed * Time.deltaTime);
        }

        if (moveDirection.magnitude >= 0.1f)
        {
            float speed = Input.GetKey(KeyCode.LeftShift) ? run_speed : walk_speed;
            if (isBlocking) speed = back_speed; // Giảm tốc độ khi đang giơ rìu thủ
            if (!isBlocking && moveZ < 0) speed = walk_speed; // Không chạy nhanh lùi

            Vector3 targetVelocity = moveDirection * speed;
            targetVelocity.y = playerRigid.linearVelocity.y;
            playerRigid.linearVelocity = targetVelocity;
        }
        else
        {
            playerRigid.linearVelocity = new Vector3(0f, playerRigid.linearVelocity.y, 0f);
        }
    }

    void HandleRoll()
    {
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0) isRolling = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !isBlocking)
        {
            isRolling = true;
            rollTimer = rollDuration;
            ChangeAnimation("roll");

            // --- PHÁT TIẾNG LĂN ---
            if (rollSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(rollSound, rollVolume); 
            }
        }
    }

    void HandleAnimations()
    {
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (inputDir.magnitude < 0.1f) { ChangeAnimation("idle"); return; }

        if (isBlocking)
        {
            // Strafe animations khi đang cầm vũ khí chống đỡ
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            if (v < -0.1f) ChangeAnimation("goBack");
            else if (h < -0.1f && Mathf.Abs(v) < 0.5f) ChangeAnimation("leftTurn");
            else if (h > 0.1f && Mathf.Abs(v) < 0.5f) ChangeAnimation("rightTurn");
            else ChangeAnimation("slowRun");
        }
        else
        {
            // Chạy tự do bình thường
            if (Input.GetKey(KeyCode.LeftShift)) ChangeAnimation("fastRun");
            else ChangeAnimation("slowRun");
        }
    }

    void ChangeAnimation(string newAnim)
    {
        if (currentAnim != newAnim)
        {
            Debug.Log($"[Player] ChangeAnimation: {currentAnim} -> {newAnim}");
            ResetAllTriggers();
            playerAnim.SetTrigger(newAnim);
            currentAnim = newAnim;
        }
    }

    // --- HÀM NÀY ĐÃ ĐƯỢC SỬA LẠI ĐỂ ĐẢM BẢO HIỆN RÌU ---
    public void ShowAxe()
    {
        if (axeHand == null || axeBack == null) return;

        // Dùng chính trạng thái thực tế để tráo đổi cho chính xác
        bool shouldShowInHand = isWeaponDrawn;

        axeHand.SetActive(shouldShowInHand);
        axeBack.SetActive(!shouldShowInHand);
        Debug.Log($"[ShowAxe] Rút vũ khí: {shouldShowInHand}. Tay: {axeHand.activeSelf}, Lưng: {axeBack.activeSelf}");
    }

    public void EnableHitbox() { if (axeHitbox != null) axeHitbox.enabled = true; }
    public void DisableHitbox() { if (axeHitbox != null) axeHitbox.enabled = false; }

    void ResetAllTriggers()
    {
        string[] trigs = { "slowRun", "fastRun", "goBack", "leftTurn", "rightTurn", "idle", "roll", "equip", "unequip", "ATK1", "combo1", "combo2" };
        foreach (var t in trigs) playerAnim.ResetTrigger(t);
    }

    // --- CÁC HÀM PHÁT M THANH CHO BƯỚC CHÂN ---
    void HandleFootsteps()
    {
        if (isRolling || isActing || playerRigid == null) return;

        // 1. Dùng phím bấm Input thay vì Vận tốc để tránh lỗi trễ và giật cục nhịp chân
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        bool isInputMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;

        if (isInputMoving)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float delay = isRunning ? runStepDelay : walkStepDelay;

            stepTimer += Time.deltaTime;
            if (stepTimer >= delay)
            {
                PlayFootstepSound();
                stepTimer = 0f; // Reset đếm sau mỗi nhịp
            }
        }
        else
        {
            // 2. Mẹo: Khi đứng im ép timer lên mức cao để vừa bấm nút là PHÁT SOUND NGAY LẬP TỨC
            stepTimer = 10f; 
        }
    }

    void PlayFootstepSound()
    {
        if (audioSource != null && footstepSound != null)
        {
            audioSource.PlayOneShot(footstepSound, 0.6f); 
        }
    }

    // --- CÁC HÀM PHÁT M THANH CHO ANIMATION EVENT ---
    public void PlaySwingSound()
    {
        Debug.Log("[Player] PlaySwingSound được gọi từ Animation Event!");
        if (audioSource == null) 
        {
            Debug.LogError("[Player] audioSource bị NULL! Không thể phát tiếng chém.");
            return;
        }
        if (swingSounds == null || swingSounds.Length == 0)
        {
            Debug.LogWarning("[Player] swingSounds trống! Hãy kéo âm thanh chém vào mảng swingSounds trên Inspector.");
            return;
        }
        
        AudioClip clip = swingSounds[Random.Range(0, swingSounds.Length)];
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log("[Player] Đang phát âm thanh chém: " + clip.name + " trên AudioSource: " + audioSource.gameObject.name);
        }
        else
        {
            Debug.LogWarning("[Player] clip được chọn bị NULL!");
        }
    }

    public void PlayEquipSound()
    {
        if (audioSource == null)
        {
            Debug.LogError("[Player] audioSource bị NULL! Không thể phát tiếng rút/cất vũ khí.");
            return;
        }
        if (equipSound == null)
        {
            Debug.LogWarning("[Player] equipSound bị NULL! Hãy kéo file âm thanh vào ô equipSound trên Inspector.");
            return;
        }
        
        audioSource.PlayOneShot(equipSound);
        Debug.Log("[Player] Đang phát âm thanh trang bị: " + equipSound.name);
    }
}
