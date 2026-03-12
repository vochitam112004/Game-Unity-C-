using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target; // Kéo object Player vào đây
    public Vector3 targetOffset = new Vector3(0, 1.5f, 0); // Độ cao điểm nhìn (Ngang vai/đầu)
    public float distance = 4.0f; // Khoảng cách từ camera đến nhân vật

    [Header("Camera Sensitivity")]
    public float mouseSensitivity = 2.0f; // Tốc độ xoay chuột
    public float minY = -20f; // Góc nhìn cúi xuống tối đa
    public float maxY = 60f;  // Góc nhìn ngẩng lên tối đa

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // Khóa con trỏ chuột vào giữa màn hình và làm ẩn nó đi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Lấy góc nhìn ban đầu của camera
        Vector3 angles = transform.eulerAngles;
        rotationX = angles.y;
        rotationY = angles.x;
    }

    // Dùng LateUpdate cho Camera để đảm bảo Camera di chuyển SAU KHI nhân vật đã di chuyển
    // Giúp hình ảnh không bị giật lag (jitter)
    void LateUpdate()
    {
        if (target == null) return;

        // 1. Nhận tín hiệu di chuyển từ chuột
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationX += mouseX;
        rotationY -= mouseY;

        // 2. Giới hạn góc nhìn Y (Lên/Xuống) để camera không bị lật ngược
        rotationY = Mathf.Clamp(rotationY, minY, maxY);

        // 3. Tính toán góc quay mới
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);

        // 4. Tính toán vị trí mới của camera sao cho luôn giữ khoảng cách với nhân vật
        Vector3 targetPosition = target.position + targetOffset;
        Vector3 position = targetPosition - (rotation * Vector3.forward * distance);

        // 5. Áp dụng tọa độ và góc quay vào Camera
        transform.rotation = rotation;
        transform.position = position;
    }
}