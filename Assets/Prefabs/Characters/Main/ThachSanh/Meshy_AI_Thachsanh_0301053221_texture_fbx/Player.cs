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

    void Start()
    {
        if (axeHand != null) axeHand.SetActive(false);
        if (axeBack != null) axeBack.SetActive(true);
    }

    void FixedUpdate()
    {
        playerRigid.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        if (isRolling || isActing)
        {
            if (isRolling)
            {
                // Khi lăn, lao về phía trước dựa trên hướng hiện tại của model
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

        // Cập nhật trạng thái khóa phím dựa trên Animator thực tế
        isActing = stateInfo.IsName("equip") || stateInfo.IsName("unequip");

        if (isActing)
        {
            // Tôn trọng Animator: Không can thiệp, để nó tự diễn và gọi Animation Event
            return;
        }

        HandleRoll();

        // Chỉ cập nhật Blend Tree khi không bận diễn equip/unequip
        float targetBlend = isWeaponDrawn ? 1f : 0f;
        playerAnim.SetFloat("Blend", targetBlend, 0.1f, Time.deltaTime);

        // Xử lý Input
        if (Input.GetKeyDown(KeyCode.E) && !isRolling)
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

            // Thoát frame hiện tại để Animator có thời gian chuyển trạng thái
            return;
        }

        if (!isRolling)
        {
            // BỎ HandleRotation() cũ vì hướng nhìn giờ phụ thuộc vào Camera
            HandleAnimations();
        }

        // Giữ hướng model thẳng, không bị nghiêng ngả
        if (playerTrans != null && playerTrans.childCount > 0)
        {
            playerTrans.GetChild(0).localRotation = Quaternion.identity;
        }
    }

    // --- HÀM ĐƯỢC GỌI BỞI ANIMATION EVENT ---
    public void ShowAxe()
    {
        if (isWeaponDrawn)
        {
            if (axeHand != null) axeHand.SetActive(true);
            if (axeBack != null) axeBack.SetActive(false);
            Debug.Log("Animation Event: Đã rút rìu.");
        }
        else
        {
            if (axeHand != null) axeHand.SetActive(false);
            if (axeBack != null) axeBack.SetActive(true);
            Debug.Log("Animation Event: Đã cất rìu.");
        }
    }

    // --- CẬP NHẬT: DI CHUYỂN THEO CAMERA (CAMERA-RELATIVE MOVEMENT) ---
    void HandleMovement()
    {
        // Kiểm tra xem có Camera chính không
        if (Camera.main == null)
        {
            Debug.LogWarning("Chưa có Main Camera!");
            return;
        }

        // Lấy hướng trục Z (tiến/lùi) của Camera và chuẩn hóa về mặt phẳng ngang
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        // Lấy hướng trục X (trái/phải) của Camera và chuẩn hóa
        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveVelocity = Vector3.zero;

        // Xử lý phím di chuyển
        if (Input.GetKey(KeyCode.W))
        {
            moveVelocity += camForward * (Input.GetKey(KeyCode.LeftShift) ? run_speed : walk_speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVelocity -= camForward * back_speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveVelocity -= camRight * walk_speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVelocity += camRight * walk_speed;
        }

        // Áp dụng vận tốc
        moveVelocity.y = playerRigid.linearVelocity.y; // Giữ nguyên trục Y (Trọng lực)
        playerRigid.linearVelocity = moveVelocity;

        // Nếu có phím bấm, xoay nhân vật hướng về phía đang di chuyển (Hoặc hướng Camera)
        // Trong trường hợp này, mình ép nhân vật luôn quay lưng về phía Camera để giống RPG
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
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            isRolling = true;
            rollTimer = rollDuration;
            ChangeAnimation("roll");
        }
    }

    // XÓA HÀM HandleRotation() CŨ

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