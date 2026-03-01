using UnityEngine;
using ThachSanh.Systems;

/// <summary>
/// Chạy khi scene game (Chuong1_GocDa, v.v.) load.
/// Nếu có PendingLoadData từ LevelLoader.LoadGame(), sẽ restore karma và vị trí player.
/// Gắn script này vào một GameObject trong scene game (ví dụ empty "GameBootstrap").
/// </summary>
public class GameSceneBootstrap : MonoBehaviour
{
    [Header("Optional: Tag của Player (mặc định 'Player')")]
    [SerializeField] private string playerTag = "Player";

    private void Start()
    {
        var data = LevelLoader.PendingLoadData;
        if (data == null) return;

        LevelLoader.PendingLoadData = null;

        if (KarmaManager.Instance != null)
        {
            KarmaManager.Instance.SetKarma(data.karma);
            Debug.Log($"[GameSceneBootstrap] Đã restore Karma: {data.karma}");
        }

        RestorePlayerPosition(data);
    }

    private void RestorePlayerPosition(GameSaveData data)
    {
        GameObject player = FindPlayer();
        if (player != null)
        {
            player.transform.position = data.PlayerPosition;
            Debug.Log($"[GameSceneBootstrap] Đã restore vị trí player: {data.PlayerPosition}");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveCurrentGame();
        }
    }

    /// <summary>
    /// Lưu game hiện tại. Có thể gọi từ UI button hoặc phím F5.
    /// </summary>
    public void SaveCurrentGame()
    {
        var data = new GameSaveData
        {
            chapterIndex = LevelLoader.GetCurrentChapterIndex() >= 0
                ? LevelLoader.GetCurrentChapterIndex()
                : 0,
            karma = KarmaManager.Instance != null ? KarmaManager.Instance.GetCurrentKarma() : 0
        };

        GameObject player = FindPlayer();
        if (player != null)
        {
            data.SetPlayerPosition(player.transform.position);
        }

        SaveSystem.SaveGame(data);
    }

    private GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            var playerCombat = Object.FindFirstObjectByType<PlayerCombat>();
            if (playerCombat != null) player = playerCombat.gameObject;
        }
        return player;
    }
}
