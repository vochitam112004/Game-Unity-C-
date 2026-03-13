using UnityEngine;

public class AnimationProxy : MonoBehaviour
{
    public BossBrain bossBrain; // Ô để kéo thằng cha vào

    // Hàm này sẽ được Animation Event gọi
    public void TriggerHitPlayer()
    {
        if (bossBrain != null)
        {
            bossBrain.TriggerHitPlayer(); // Bảo thằng cha: "Đấm trúng rồi, trừ máu đi!"
        }
    }
}