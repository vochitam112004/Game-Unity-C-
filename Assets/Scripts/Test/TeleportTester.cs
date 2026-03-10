using UnityEngine;

public class TeleportTester : MonoBehaviour
{
    public Transform player;
    public Transform boss;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Vector3 pos = boss.position + boss.forward * 3f;
            player.position = pos;
        }
    }
}