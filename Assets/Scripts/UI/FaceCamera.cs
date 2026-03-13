using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        // Luôn nhìn về phía Camera chính
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}