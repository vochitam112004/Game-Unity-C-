using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mục tiêu chính (Vị trí bám theo)")]
    public Transform target; // Nhân vật chính để Camera bám theo vị trí
    public Vector3 offset; // Khoảng cách tương đối
    public float smoothSpeed = 15f; 

    [Header("Mục tiêu phụ (Góc nhìn)")]
    public Transform secondaryTarget; // Nhân vật thứ 2 để xoay sang nhìn
    public bool lookAtSecondary = false; // Tích vào để quay 180 độ sang nhìn người kia
    
    [Header("Cài đặt góc nhìn")]
    public float lookAtHeight = 1.2f; // Độ cao điểm nhìn của mục tiêu chính
    public float secondaryLookAtHeight = 1.2f; // Độ cao điểm nhìn của mục tiêu phụ
    public bool autoCalculateOffset = true;

    void Start()
    {
        if (target != null && autoCalculateOffset)
        {
            offset = target.InverseTransformDirection(transform.position - target.position);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Luôn bám theo vị trí của nhân vật chính (Target)
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 2. Xoay Camera
        if (lookAtSecondary && secondaryTarget != null)
        {
            // Quay sang nhìn nhân vật phụ (Thạch Sanh)
            Vector3 lookTarget = secondaryTarget.position + Vector3.up * secondaryLookAtHeight;
            transform.LookAt(lookTarget);
        }
        else
        {
            // Nhìn vào nhân vật chính (Lý Thông)
            Vector3 lookTarget = target.position + Vector3.up * lookAtHeight;
            transform.LookAt(lookTarget);
        }
    }
}