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

        // Tự động tìm hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
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
                playerRigid.linearVelocity = new Vector3(0, playerRigid.linearVelocity.y, 0);
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
                    currentAnim = "idle";
                    DisableHitbox();
                }
            }
            else if (!playerAnim.IsInTransition(0) && stateInfo.IsName("idle"))
            {
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
            if (Input.GetMouseButtonDown(0)) { ChangeAnimation("ATK1"); return; }
            if (Input.GetKeyDown(KeyCode.Q)) { ChangeAnimation("combo1"); return; }
            if (Input.GetKeyDown(KeyCode.F)) { ChangeAnimation("combo2"); return; }
        }

        if (!isRolling && !isBlocking)
        {
            HandleAnimations();
        }
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

    // --- CÁC HÀM PHÁT M THANH CHO ANIMATION EVENT ---
    public void PlaySwingSound()
    {
        if (audioSource != null && swingSounds != null && swingSounds.Length > 0)
        {
            AudioClip clip = swingSounds[Random.Range(0, swingSounds.Length)];
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayEquipSound()
    {
        if (audioSource != null && equipSound != null)
        {
            audioSource.PlayOneShot(equipSound);
        }
    }
}