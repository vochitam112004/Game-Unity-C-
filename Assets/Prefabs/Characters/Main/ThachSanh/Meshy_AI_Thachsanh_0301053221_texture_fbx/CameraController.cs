using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mục Tiêu Theo Dõi")]
    public Transform target; // Kéo object Thạch Sanh vào đây

    [Header("Cài Đặt Góc Nhìn")]
    public float distance = 4f; // Khoảng cách từ cam đến nhân vật
    public float heightOffset = 1.5f; // Chiều cao tâm điểm (ngang vai/đầu nhân vật)

    [Header("Độ Nhạy Chuột")]
    public float sensitivityX = 3f;
    public float sensitivityY = 2f;

    [Header("Giới Hạn Góc Nhìn Lên/Xuống")]
    public float minYAngle = -15f;
    public float maxYAngle = 60f;

    private float currentX = 0f;
    private float currentY = 20f;

    void Start()
    {
        // Khóa chuột vào giữa màn hình và làm ẩn con trỏ chuột
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Nhận tín hiệu di chuyển từ chuột
        currentX += Input.GetAxis("Mouse X") * sensitivityX;
        currentY -= Input.GetAxis("Mouse Y") * sensitivityY;

        // 2. Chặn góc xoay trục Y để camera không lật ngược qua đầu hoặc cắm hẳn xuống đất
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        // 3. Tính toán vị trí xoay quanh nhân vật
        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // 4. Áp dụng vị trí và hướng nhìn cho Camera
        Vector3 lookPosition = target.position + Vector3.up * heightOffset;
        transform.position = lookPosition + rotation * direction;
        transform.LookAt(lookPosition);
    }
}