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
    private bool isActing = false; // Biến kiểm soát trạng thái bận

    void Start()
    {
        if (axeHand != null) axeHand.SetActive(false);
        if (axeBack != null) axeBack.SetActive(true);
    }

    void FixedUpdate()
    {
        playerRigid.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // NẾU ĐANG DIỄN HÀNH ĐỘNG (RÚT/CẤT RÌU HOẶC LĂN), KHÓA DI CHUYỂN
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
                // Đứng im tại chỗ khi rút/cất rìu
                playerRigid.linearVelocity = new Vector3(0, playerRigid.linearVelocity.y, 0);
            }
            return;
        }

        HandleMovement();
    }

    void Update()
    {
        // 1. CẬP NHẬT TRẠNG THÁI BẬN (ISACTING)
        AnimatorStateInfo stateInfo = playerAnim.GetCurrentAnimatorStateInfo(0);
        isActing = stateInfo.IsName("equip") || stateInfo.IsName("unequip");

        // 2. NẾU ĐANG BẬN DIỄN ANIMATION EQUIP/UNEQUIP -> KHÓA SẠCH PHÍM
        if (isActing)
        {
            // Chỉ khi diễn gần xong mới cho phép reset currentAnim
            if (stateInfo.normalizedTime >= 0.95f && currentAnim != "idle")
            {
                currentAnim = "idle";
                // Lưu ý: Không SetTrigger "idle" ở đây để tránh xung đột với Has Exit Time
            }
            return; // Thoát Update sớm, không nhận bất kỳ input nào khác
        }

        HandleRoll();

        // 3. CHỈ CẬP NHẬT BLEND TREE KHI THỰC SỰ ĐANG Ở IDLE
        if (currentAnim == "idle")
        {
            float targetBlend = isWeaponDrawn ? 1f : 0f;
            playerAnim.SetFloat("Blend", targetBlend, 0.1f, Time.deltaTime);
        }

        // 4. BẤM E ĐỂ RÚT/CẤT RÌU
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
        }

        if (!isRolling)
        {
            HandleRotation();
            HandleAnimations();
        }

        // Giữ model thẳng hướng
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
        if (Input.GetKey(KeyCode.W))
        {
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? run_speed : walk_speed;
            Vector3 moveVelocity = transform.forward * currentSpeed;
            moveVelocity.y = playerRigid.linearVelocity.y;
            playerRigid.linearVelocity = moveVelocity;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Vector3 moveVelocity = -transform.forward * back_speed;
            moveVelocity.y = playerRigid.linearVelocity.y;
            playerRigid.linearVelocity = moveVelocity;
        }
        else
        {
            playerRigid.linearVelocity = new Vector3(0, playerRigid.linearVelocity.y, 0);
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

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.A)) playerTrans.Rotate(0, -ro_speed * Time.deltaTime, 0);
        if (Input.GetKey(KeyCode.D)) playerTrans.Rotate(0, ro_speed * Time.deltaTime, 0);
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