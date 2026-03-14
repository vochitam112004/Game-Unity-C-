using UnityEngine;

/// <summary>
/// Script điều khiển di chuyển nhân vật chính (Thạch Sanh).
/// Được gắn trực tiếp vào Player GameObject (qua prefab Player 1).
/// Sử dụng Rigidbody để di chuyển với camera-relative movement.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Tham chiếu")]
    public Animator playerAnim;
    public Rigidbody playerRigid;
    public Transform playerTrans;

    [Header("Vũ khí / Hitbox")]
    public GameObject axeHand;    // Rìu đang cầm
    public GameObject axeBack;    // Rìu cất sau lưng
    public Collider axeHitbox;    // Collider gây sát thương

    [Header("Tốc độ di chuyển")]
    public float walk_speed = 12f;
    public float run_speed  = 18f;
    public float back_speed = 3f;
    public float ro_speed   = 150f;  // Tốc độ xoay (độ/giây)
    public float roll_speed = 15f;
    public float rollDuration = 2f;
    public float extraGravity = 40f;

    // ─── Trạng thái nội bộ ────────────────────────────────────────────
    private bool isRolling = false;
    private float rollTimer  = 0f;
    private bool isGrounded  = false;
    private Camera mainCam;

    // Animator parameter hashes (hiệu năng tốt hơn string)
    private static readonly int HashSpeed      = Animator.StringToHash("Speed");
    private static readonly int HashIsRunning  = Animator.StringToHash("IsRunning");
    private static readonly int HashRoll       = Animator.StringToHash("Roll");
    private static readonly int HashGrounded   = Animator.StringToHash("IsGrounded");

    void Awake()
    {
        mainCam = Camera.main;

        // Tự gán nếu chưa kéo trong Inspector
        if (playerRigid == null) playerRigid = GetComponent<Rigidbody>();
        if (playerTrans  == null) playerTrans  = transform;
        if (playerAnim   == null) playerAnim   = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        CheckGrounded();
        HandleRoll();
        HandleMovement();
        HandleWeaponToggle();
    }

    void FixedUpdate()
    {
        // Thêm trọng lực phụ để nhân vật không bay lên khi nhảy/xuống dốc
        if (!isGrounded)
            playerRigid.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    // ─── Di chuyển ────────────────────────────────────────────────────
    void HandleMovement()
    {
        if (isRolling) return;   // Không nhận input khác khi đang lăn

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);

        bool isMoving = input.sqrMagnitude > 0.01f;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        float speed = 0f;
        if (isMoving)
            speed = isRunning ? run_speed : (v < -0.1f ? back_speed : walk_speed);

        // Xoay theo camera
        if (isMoving && mainCam != null)
        {
            Vector3 camForward = mainCam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = mainCam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 moveDir = (camForward * v + camRight * h).normalized;

            // Chỉ xoay khi đi tiến/sang
            if (v >= 0f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                playerTrans.rotation = Quaternion.RotateTowards(
                    playerTrans.rotation, targetRot, ro_speed * Time.deltaTime);
            }

            // Di chuyển bằng Rigidbody
            Vector3 velocity = moveDir * speed;
            velocity.y = playerRigid.linearVelocity.y;   // Giữ lại vận tốc Y (gravity)
            playerRigid.linearVelocity = velocity;
        }
        else
        {
            // Dừng lại (giữ Y)
            playerRigid.linearVelocity = new Vector3(0f, playerRigid.linearVelocity.y, 0f);
        }

        // Cập nhật Animator
        if (playerAnim != null)
        {
            playerAnim.SetFloat(HashSpeed, isMoving ? speed / run_speed : 0f, 0.1f, Time.deltaTime);
            playerAnim.SetBool(HashIsRunning, isRunning);
            playerAnim.SetBool(HashGrounded, isGrounded);
        }
    }

    // ─── Lăn / Roll ───────────────────────────────────────────────────
    void HandleRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRolling)
        {
            isRolling = true;
            rollTimer  = rollDuration;

            playerRigid.linearVelocity = playerTrans.forward * roll_speed + Vector3.up * playerRigid.linearVelocity.y;

            if (playerAnim != null)
                playerAnim.SetTrigger(HashRoll);
        }

        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0f)
                isRolling = false;
        }
    }

    // ─── Kiểm tra chạm đất ────────────────────────────────────────────
    void CheckGrounded()
    {
        // Raycast ngắn từ đáy CapsuleCollider
        float rayLen = 0.25f;
        isGrounded = Physics.Raycast(
            playerTrans.position + Vector3.up * 0.1f,
            Vector3.down,
            rayLen,
            ~LayerMask.GetMask("Player"));
    }

    // ─── Vũ khí / Hitbox (được gọi từ PlayerAnimEvents) ──────────────
    public void ShowAxe()
    {
        if (axeHand != null) axeHand.SetActive(true);
        if (axeBack != null) axeBack.SetActive(false);
    }

    public void HideAxe()
    {
        if (axeHand != null) axeHand.SetActive(false);
        if (axeBack != null) axeBack.SetActive(true);
    }

    public void EnableHitbox()
    {
        if (axeHitbox != null) axeHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (axeHitbox != null) axeHitbox.enabled = false;
    }

    void HandleWeaponToggle()
    {
        // Ví dụ: nhấn E để bật/tắt vũ khí
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (axeHand != null) axeHand.SetActive(!axeHand.activeSelf);
            if (axeBack != null) axeBack.SetActive(!axeBack.activeSelf);
        }
    }
}
