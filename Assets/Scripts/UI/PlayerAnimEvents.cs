using UnityEngine;

public class PlayerAnimEvents : MonoBehaviour
{
    [Header("Kéo object Player (cha) vào đây")]
    public Player playerScript;

    // Các hàm này sẽ đón lệnh từ Animator và gửi thẳng ra cho script Player ở ngoài
    public void ShowAxe()
    {
        if (playerScript != null) playerScript.ShowAxe();
    }

    public void EnableHitbox()
    {
        if (playerScript != null) playerScript.EnableHitbox();
    }

    public void DisableHitbox()
    {
        if (playerScript != null) playerScript.DisableHitbox();
    }
}