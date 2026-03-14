using UnityEngine;

public class LookController : MonoBehaviour
{
    [Header("Mục tiêu cần nhìn (Kéo Player vào đây)")]
    public Transform target;
    
    [Header("Tốc độ xoay (Càng lón càng nhanh)")]
    [Range(1f, 15f)]
    public float rotationSpeed = 5f;

    [Header("Trạng thái")]
    public bool canLook = false; // Mặc định tắt, chỉ bật khi vào phim

    [Header("Chỉ xoay trục ngang (Trục Y)")]
    public bool onlyRotateY = true;

    void LateUpdate()
    {
        // Chỉ nhìn khi được phép và có mục tiêu
        if (canLook && target != null)
        {
            // Tìm hướng từ nhân vật này trỏ tới Player
            Vector3 direction = target.position - transform.position;

            // Nếu chỉ muốn xoay ngang người (ngăn nhân vật bị lật ngửa cắm mặt xuống đất khi người chơi nhảy lên cao hoặc chui xuống thấp)
            if (onlyRotateY)
            {
                direction.y = 0f;
            }

            // Phòng trường hợp nhân vật đứng trùng khít vị trí với Player gây lỗi xoay tung tóe
            if (direction != Vector3.zero)
            {
                // Tính toán góc quay mong muốn
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                // Dùng Slerp để xoay một cách mượt mà từ từ, không bị khựng gắt lật mặt rụp một cái
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
}
