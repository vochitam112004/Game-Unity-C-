using UnityEngine;

public class MouseCameraFollow : MonoBehaviour
{
    [Header("Mục Tiêu")]
    public Transform target; // Kéo thả Player vô đây (Hoặc cứ để trống, nó sẽ tự tìm tag Player)

    [Header("Khoảng cách & Chiều cao")]
    public float distance = 5.0f; // Khoảng cách tới nhân vật
    public float height = 2.0f;   // Chiều cao ngắm vào nửa thân trên

    [Header("Độ mượt & Tốc độ chuột")]
    public float smoothness = 25f; // Độ mượt bám theo
    public float mouseSensitivity = 3f; // Tốc độ rê chuột

    private float currentX = 0f;
    private float currentY = 15f; // Nghiêng nhẹ xuống 15 độ lúc mới vào

    void Start()
    {
        // Tự động tìm Player nếu lười kéo thả
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        // Tạm khóa chuột hiển thị ở giữa màn hình (để dễ lắc góc nhìn)
        // Nếu bạn cần bấm nút UI thì ấn phím ESC nó sẽ hiện chuột ra nhé!
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Cho phép dùng chuột để bay lượn ngắm nghía xung quanh nhân vật
        currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Khóa góc quay lên xuống không cho ngửa cổ, chúc đầu quá quắt
        currentY = Mathf.Clamp(currentY, -15f, 50f); 

        // Tính công thức Toán xoay vòng
        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // Tính vị trí cần bay tới
        Vector3 desiredPosition = target.position + Vector3.up * height + rotation * dir;
        
        // Tịnh tiến mượt
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothness * Time.deltaTime);

        // Mắt camera luôn ngắm vào ngực/lưng Thạch Sanh
        transform.LookAt(target.position + Vector3.up * height);
    }
}
