using UnityEngine;

public class GameDebugger : MonoBehaviour
{
    public Transform player;
    public Transform boss;
    public KeyCode teleportKey = KeyCode.T; // Bấm nút T để bay đến Boss

    void Update()
    {
        if (Input.GetKeyDown(teleportKey))
        {
            if (player != null && boss != null)
            {
                // Vô hiệu hóa Controller tạm thời để Unity cho phép dịch chuyển tọa độ
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                // Bay đến vị trí Boss (cộng thêm 2 mét để không dẫm lên đầu nhau)
                player.position = boss.position + Vector3.forward * 2f;

                if (cc != null) cc.enabled = true;

                Debug.Log("Hô biến! Đã dịch chuyển Thạch Sanh đến chỗ Boss.");
            }
        }
    }
}