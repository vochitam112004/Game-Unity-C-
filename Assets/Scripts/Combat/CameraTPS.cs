using UnityEngine;

public class CameraTPS : MonoBehaviour
{
    public Transform target; // Kéo nhân vật vào đây
    public float mouseSensitivity = 2f; // Độ nhạy chuột
    public float distanceFromTarget = 5f; // Khoảng cách camera (zoom)
    public Vector2 pitchMinMax = new Vector2(-10, 85); // Giới hạn ngẩng lên/cúi xuống
    public float smoothTime = 0.1f; // Độ mượt khi xoay

    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;
    private float yaw; // Góc xoay ngang (Trái/Phải)
    private float pitch; // Góc xoay dọc (Lên/Xuống)

    void Start()
    {
        // Dòng này để ẩn con trỏ chuột đi giúp xoay thoải mái (Bấm ESC để hiện lại)
        // Nếu mới test thấy khó chịu thì tạm thời comment dòng dưới lại //
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Nhận tín hiệu từ chuột
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Giới hạn không cho camera chui xuống đất hoặc ngửa ra sau quá đà
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        // 2. Làm mượt chuyển động xoay
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, smoothTime);

        // 3. Xoay Camera theo góc đã tính
        transform.eulerAngles = currentRotation;

        // 4. Tính vị trí: Luôn nằm sau lưng nhân vật một khoảng 'distanceFromTarget'
        // Vector3.up * 1.5f là để camera nhìn vào vai/đầu thay vì nhìn vào chân
        Vector3 targetPos = target.position + Vector3.up * 1.5f - transform.forward * distanceFromTarget;

        transform.position = targetPos;
    }
}