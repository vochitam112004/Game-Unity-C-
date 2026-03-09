using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Nhân vật Thạch Sanh
    public Vector3 offset = new Vector3(0, 3, -5); // Chỉnh độ cao (Y) và độ xa (Z)
    public float smoothSpeed = 15f; // Tăng lên 20-30 nếu muốn cam đi nhanh hơn

    void LateUpdate()
    {
        if (target == null) return;

        // Tính vị trí camera dựa trên hướng xoay của nhân vật
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Di chuyển mượt mà
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Luôn nhìn vào Thạch Sanh
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}