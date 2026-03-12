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

    // BIẾN MỚI: Quản lý trạng thái Đỡ đòn
    private bool isBlocking = false;

    void Start()
    {
        if (axeHand != null) axeHand.SetActive(false);
        if (axeBack != null) axeBack.SetActive(true);
    }

    void FixedUpdate()
    {
        playerRigid.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // CẬP NHẬT: Khóa di chuyển khi đang Block để nhân vật đứng trụ lại
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
                // Đứng im tại chỗ khi rút/cất rìu HOẶC đang đỡ đòn
                playerRigid.linearVelocity = new Vector3(0, playerRigid.linearVelocity.y, 0);
            }
            return;
        }

        HandleMovement();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = playerAnim.GetCurrentAnimatorStateInfo(0);

        isActing = stateInfo.IsName("equip") || stateInfo.IsName("unequip");

        if (isActing)
        {
            return;
        }

        HandleRoll();

        // --- CƠ CHẾ ĐỠ ĐÒN (BLOCK) ---
        // Giữ Chuột Phải để đỡ đòn. Điều kiện: Phải đang cầm rìu và không lộn nhào
        isBlocking = Input.GetMouseButton(1) && isWeaponDrawn && !isRolling;
        playerAnim.SetBool("block", isBlocking);

        float targetBlend = isWeaponDrawn ? 1f : 0f;
        playerAnim.SetFloat("Blend", targetBlend, 0.1f, Time.deltaTime);

        // Khóa phím E khi đang đỡ đòn
        if (Input.GetKeyDown(KeyCode.E) && !isRolling && !isBlocking)
        {
            isWeaponDrawn = !isWeaponDrawn;

            if (isWeaponDrawn)
            {
                ChangeAnimation("equip");
            }
            else
            {
                ChangeAnimation("unequip");
            }
            return;
        }

        // Nếu đang đỡ đòn thì không cập nhật các animation di chuyển khác
        if (!isRolling && !isBlocking)
        {
            HandleAnimations();
        }

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
        if (Camera.main == null) return;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveVelocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveVelocity += camForward * (Input.GetKey(KeyCode.LeftShift) ? run_speed : walk_speed);
        if (Input.GetKey(KeyCode.S)) moveVelocity -= camForward * back_speed;
        if (Input.GetKey(KeyCode.A)) moveVelocity -= camRight * walk_speed;
        if (Input.GetKey(KeyCode.D)) moveVelocity += camRight * walk_speed;

        moveVelocity.y = playerRigid.linearVelocity.y;
        playerRigid.linearVelocity = moveVelocity;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            Quaternion targetRotation = Quaternion.LookRotation(camForward);
            playerTrans.rotation = Quaternion.Slerp(playerTrans.rotation, targetRotation, ro_speed * Time.deltaTime);
        }
    }

    void HandleRoll()
    {
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0) isRolling = false;
        }
        // Không cho lộn nhào khi đang đỡ đòn
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
    }
}