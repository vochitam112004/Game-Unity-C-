using UnityEngine;

public class GameDebugger : MonoBehaviour
{
    [Header("Gắn các vật thể vào đây")]
    public Transform player;
    public Transform boss;
    public BossBrain bossBrain;

    void Update()
    {
        // 1. CHỨC NĂNG DỊCH CHUYỂN (Phím T)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TeleportPlayerToBoss();
        }

        // 2. CHỨC NĂNG TEST TRỪ MÁU (Phím Y)
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (bossBrain != null)
            {
                bossBrain.TakeDamage(100f);
                Debug.Log("Đã trừ 100 máu của Boss bằng phím Y!");
            }
        }
    }

    void TeleportPlayerToBoss()
    {
        if (player == null || boss == null) return;

        // Tính toán vị trí mới: Đứng cách mặt Boss 5 mét
        Vector3 newPosition = boss.position + boss.forward * 5f;
        // Đảm bảo không bị chui xuống đất
        newPosition.y = boss.position.y;

        // LƯU Ý VẬT LÝ QUAN TRỌNG: 
        // Nếu Player của bạn dùng CharacterController, nó sẽ "chống cự" lại lệnh dịch chuyển.
        // Ta phải tạm tắt nó đi, dịch chuyển xong rồi bật lại.
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.position = newPosition;
            cc.enabled = true;
        }
        else
        {
            // Nếu dùng Rigidbody bình thường
            player.position = newPosition;
        }

        // Ép camera của Player nhìn thẳng vào mặt Boss luôn cho ngầu
        player.LookAt(boss.position);

        Debug.Log("Đã dịch chuyển Player đến khu vực Boss!");
    }
}