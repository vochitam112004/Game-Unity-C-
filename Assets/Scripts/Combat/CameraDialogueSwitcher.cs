using UnityEngine;

public class CameraDialogueSwitcher : MonoBehaviour
{
    [Header("Mục tiêu")]
    public Transform playerTarget; 
    
    [Header("Cài đặt")]
    public bool enableAutoLook = true; // Tắt cái này để tự chỉnh hoàn toàn bằng tay
    public bool lookAtPlayer = false; // Tích = nhìn Player, Bỏ tích = nhìn NPC
    public bool lockRotationX = true; // Khóa trục X để không bị chúc xuống/ngước lên
    public bool lockRotationZ = true; // Khóa trục Z để không bị nghiêng

    [Header("Độ cao điểm nhìn")]
    public float playerHeightOffset = 1.4f;
    public float npcHeightOffset = 1.2f;
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        // Nếu không bật tự động, trả quyền điều khiển hoàn toàn cho người dùng/Inspector
        if (!enableAutoLook) return;

        Vector3 focusPoint;
        if (lookAtPlayer && playerTarget != null)
        {
            focusPoint = playerTarget.position + Vector3.up * playerHeightOffset;
        }
        else if (transform.parent != null)
        {
            focusPoint = transform.parent.position + Vector3.up * npcHeightOffset;
        }
        else return;

        // Tính toán góc xoay hướng về mục tiêu
        Vector3 direction = focusPoint - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Lấy góc quay Euler hiện tại (để giữ lại X hoặc Z nếu bị khóa)
            Vector3 currentEuler = transform.eulerAngles;
            Vector3 targetEuler = targetRotation.eulerAngles;

            float finalX = lockRotationX ? currentEuler.x : targetEuler.x;
            float finalY = targetEuler.y;
            float finalZ = lockRotationZ ? currentEuler.z : targetEuler.z;

            Quaternion finalRotation = Quaternion.Euler(finalX, finalY, finalZ);
            
            // Xoay mượt mà
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, smoothSpeed * Time.deltaTime);
        }
    }
}
