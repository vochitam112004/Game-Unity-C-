using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    public Animator PlayerAnim;
    public Rigidbody PlayerRigid;
    public Transform PlayerCam;

    [Header("Movement Speeds")]
    public float W_speed;    // Slowrun
    public float Wb_speed;   // Jogbackward
    public float Olw_speed;  // Tốc độ khác (như trong hình của bạn)
    public float Rn_speed;   // Sprint
    public float Ro_speed;   // Rotation

    [Header("Status")]
    public bool Walking;

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float moveVertical = Input.GetAxisRaw("Vertical");
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (moveVertical == 0)
        {
            PlayerAnim.SetTrigger("idle");
            Walking = false;
        }
        else if (moveVertical > 0)
        {
            Walking = true;
            if (isSprinting)
            {
                PlayerAnim.SetTrigger("sprint");
                transform.Translate(Vector3.forward * Rn_speed * Time.deltaTime);
            }
            else
            {
                PlayerAnim.SetTrigger("slowrun");
                transform.Translate(Vector3.forward * W_speed * Time.deltaTime);
            }
        }
        else if (moveVertical < 0)
        {
            Walking = true;
            PlayerAnim.SetTrigger("jogbackward");
            transform.Translate(Vector3.back * Wb_speed * Time.deltaTime);
        }

        // Action: Roll
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerAnim.SetTrigger("roll");
        }
    }
}