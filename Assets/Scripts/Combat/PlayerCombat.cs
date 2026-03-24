using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public float moveSpeed = 5f;
    public float turnSmoothTime = 0.1f;
    public float gravity = -9.81f;

    [Header("Vũ khí")]
    public GameObject axeModel;

    [Header("Âm thanh Vũ khí")]
    public AudioSource audioSource;
    public AudioClip swingSound;

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocity;
    private float turnSmoothVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // --- PHẦN 1: DI CHUYỂN & XOAY ---
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(x, 0f, z).normalized;

        // --- MỚI THÊM: Gửi tín hiệu cho Animator ---
        // Nếu có bấm nút (direction.magnitude >= 0.1), Speed = 1 (Chạy)
        // Nếu không bấm, Speed = 0 (Đứng yên)
        // Chú ý: Bạn phải tạo tham số "Speed" trong Animator (xem Bước 2 bên dưới)
        anim.SetFloat("Speed", direction.magnitude);

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        // --- PHẦN 2: TRỌNG LỰC ---
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- PHẦN 3: TẤN CÔNG (VUNG RÌU) ---
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Attack"); // Yêu cầu có Trigger "Attack" trong Animator
            
            // Có thể chơi âm thanh ngay khi click chuột, hoặc bạn có thể gọi hàm PlaySwingSound() bằng Animation Event
            PlaySwingSound();
        }
    }

    // Hàm này phát ra tiếng vung rìu (Có thể gọi từ Animation Event để khớp frame)
    public void PlaySwingSound()
    {
        if (audioSource == null)
        {
            Debug.LogError("[PlayerCombat] Lỗi: audioSource trống! Kéo component AudioSource vào ô audioSource trên Inspector.");
            return;
        }
        if (swingSound == null)
        {
            Debug.LogWarning("[PlayerCombat] Lỗi: swingSound bị trống! Kéo âm thanh chém vào ô swingSound.");
            return;
        }

        audioSource.PlayOneShot(swingSound);
        Debug.Log("[PlayerCombat] Đang phát âm thanh chém: " + swingSound.name);
    }
}