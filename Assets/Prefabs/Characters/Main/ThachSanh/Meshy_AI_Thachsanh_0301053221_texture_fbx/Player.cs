using UnityEngine;

public class Player : MonoBehaviour
{
    public Animator playerAnim;
    public Rigidbody playerRigid;
    public Transform playerTrans;

    [Header("Movement Speeds")]
    public float walk_speed = 12f;
    public float run_speed = 18f;
    public float back_speed = 8f;
    public float ro_speed = 150f;
    public float roll_speed = 15f;

    [Header("Roll Settings")]
    public float rollDuration = 2f;
    private bool isRolling = false;
    private float rollTimer = 0f;

    // Biến lưu trữ trạng thái hiện tại để chống "spam" trigger
    private string currentAnim = "idle";

    void FixedUpdate()
    {
        if (isRolling)
        {
            Vector3 rollVelocity = transform.forward * roll_speed;
            rollVelocity.y = playerRigid.linearVelocity.y;
            playerRigid.linearVelocity = rollVelocity;
            return;
        }

        float moveVertical = Input.GetAxis("Vertical");

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

    void Update()
    {
        HandleRoll();

        if (!isRolling)
        {
            HandleRotation();
            HandleAnimations();
        }
    }

    void HandleRoll()
    {
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;
            if (rollTimer <= 0)
            {
                isRolling = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            isRolling = true;
            rollTimer = rollDuration;

            ChangeAnimation("roll"); // Gọi hàm đổi animation thông minh
        }
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.A))
        {
            playerTrans.Rotate(0, -ro_speed * Time.deltaTime, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerTrans.Rotate(0, ro_speed * Time.deltaTime, 0);
        }
    }

    void HandleAnimations()
    {
        string newAnim = "idle";

        if (Input.GetKey(KeyCode.W))
        {
            newAnim = Input.GetKey(KeyCode.LeftShift) ? "fastRun" : "slowRun";
        }
        else if (Input.GetKey(KeyCode.S))
        {
            newAnim = "goBack";
        }
        else if (Input.GetKey(KeyCode.A))
        {
            newAnim = "leftTurn";
        }
        else if (Input.GetKey(KeyCode.D))
        {
            newAnim = "rightTurn";
        }

        // Thay vì spam Trigger, ta truyền tên trạng thái mới vào đây
        ChangeAnimation(newAnim);
    }

    // Hàm quản lý Animation chuyên nghiệp (Chỉ đổi khi thực sự cần)
    void ChangeAnimation(string newAnim)
    {
        if (currentAnim != newAnim)
        {
            ResetAllTriggers();
            playerAnim.SetTrigger(newAnim);
            currentAnim = newAnim; // Cập nhật lại trạng thái hiện tại
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
    }
}