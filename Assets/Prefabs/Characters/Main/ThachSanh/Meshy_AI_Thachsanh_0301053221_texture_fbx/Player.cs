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
    public float ro_speed = 150f;
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

    // Camera riêng của player (child camera), không dùng Camera.main vì scene có nhiều camera
    private Camera playerCamera;

    void Start()
    {
        // Lấy camera gắn vào Player (child), ưu tiên hơn Camera.main
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (axeHand != null) axeHand.SetActive(false);
        if (axeBack != null) axeBack.SetActive(true);

        DisableHitbox();
    }

    void FixedUpdate()
    {
        playerRigid.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        if (isRolling || isActing || isBlocking)
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
                    DisableHitbox(); // [MỚI] Khi chém xong về thế thủ, bắt buộc tắt Hitbox
                }
            }
            else if (!playerAnim.IsInTransition(0) && stateInfo.IsName("idle"))
            {
                currentAnim = "idle";
                DisableHitbox(); // [MỚI] Đề phòng kẹt animation, cứ về idle là tắt
            }
            return;
        }

        HandleRoll();

        isBlocking = Input.GetMouseButton(1) && isWeaponDrawn && !isRolling;
        playerAnim.SetBool("block", isBlocking);

        // [MỚI] KHOÁ CHẶT SÁT THƯƠNG KHI ĐANG ĐỠ
        if (isBlocking)
        {
            DisableHitbox();
        }

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
            if (Input.GetMouseButtonDown(0))
            {
                ChangeAnimation("ATK1");
                return;
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                ChangeAnimation("combo1");
                return;
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeAnimation("combo2");
                return;
            }
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

    public void ShowAxe()
    {
        if (isWeaponDrawn)
        {
            if (axeHand != null) axeHand.SetActive(true);
            if (axeBack != null) axeBack.SetActive(false);
        }
        else
        {
            if (axeHand != null) axeHand.SetActive(false);
            if (axeBack != null) axeBack.SetActive(true);
        }
    }

    void HandleMovement()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;
        if (Input.GetKey(KeyCode.A)) h = -1f;
        if (Input.GetKey(KeyCode.D)) h = 1f;

        // 1. Xoay nhân vật bằng phím A và D
        if (h != 0)
        {
            float rotation = h * ro_speed * Time.deltaTime;
            playerTrans.Rotate(0, rotation, 0);
        }

        // 2. Di chuyển tiến/lùi theo hướng nhìn của nhân vật
        Vector3 moveDir = playerTrans.forward * v;

        if (v == 0)
        {
            // Nếu không nhấn W/S thì dừng lại theo trục X/Z, giữ nguyên vận tốc rơi Y
            playerRigid.linearVelocity = new Vector3(0f, playerRigid.linearVelocity.y, 0f);
            return;
        }

        // Tính toán tốc độ
        float speed = (v < 0f) ? back_speed : (Input.GetKey(KeyCode.LeftShift) ? run_speed : walk_speed);

        Vector3 moveVelocity = moveDir * speed;
        moveVelocity.y = playerRigid.linearVelocity.y; // Giữ nguyên trọng lực
        playerRigid.linearVelocity = moveVelocity;
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
        string newAnim = "idle";
        if (Input.GetKey(KeyCode.W)) newAnim = Input.GetKey(KeyCode.LeftShift) ? "fastRun" : "slowRun";
        else if (Input.GetKey(KeyCode.S)) newAnim = "goBack";
        else if (Input.GetKey(KeyCode.A)) newAnim = "leftTurn";
        else if (Input.GetKey(KeyCode.D)) newAnim = "rightTurn";

        ChangeAnimation(newAnim);
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

    public void EnableHitbox()
    {
        if (axeHitbox != null) axeHitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (axeHitbox != null) axeHitbox.enabled = false;
    }

    void ResetAllTriggers()
    {
        playerAnim.ResetTrigger("slowRun");
        playerAnim.ResetTrigger("fastRun");
        playerAnim.ResetTrigger("goBack");
        playerAnim.ResetTrigger("leftTurn");
        playerAnim.ResetTrigger("rightTurn");
        playerAnim.ResetTrigger("idle");
        playerAnim.ResetTrigger("roll");
        playerAnim.ResetTrigger("equip");
        playerAnim.ResetTrigger("unequip");
        playerAnim.ResetTrigger("ATK1");
        playerAnim.ResetTrigger("combo1");
        playerAnim.ResetTrigger("combo2");
    }
}