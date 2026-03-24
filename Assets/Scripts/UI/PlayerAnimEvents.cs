using UnityEngine;

public class PlayerAnimEvents : MonoBehaviour
{
    [Header("Kéo object Player (cha) vào đây")]
    public Player playerScript;

    // Các hàm này sẽ đón lệnh từ Animator và gửi thẳng ra cho script Player ở ngoài
    public void ShowAxe()
    {
        if (playerScript != null) 
            playerScript.ShowAxe();
        else 
            Debug.LogError("[PlayerAnimEvents] Không tìm thấy playerScript! Hãy kéo Player (Cha) vào ô trong Inspector.");
    }

    public void EnableHitbox()
    {
        if (playerScript != null) playerScript.EnableHitbox();
    }

    public void DisableHitbox()
    {
        if (playerScript != null) playerScript.DisableHitbox();
    }

    public void PlaySwingSound()
    {
        if (playerScript != null) 
            playerScript.PlaySwingSound();
        else 
            Debug.LogError("[PlayerAnimEvents] Lỗi: playerScript bị NULL! Chưa kéo Player vào PlayerAnimEvents.");
    }

    public void PlayEquipSound()
    {
        if (playerScript != null) 
            playerScript.PlayEquipSound();
        else 
            Debug.LogError("[PlayerAnimEvents] Lỗi: playerScript bị NULL! Chưa kéo Player vào PlayerAnimEvents.");
    }
}