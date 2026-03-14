using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Luôn luôn quay mặt của Canvas về phía Main Camera để chữ không bị ngược
        transform.LookAt(transform.position + mainCamera.transform.forward);
    }
}
