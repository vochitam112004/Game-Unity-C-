using System;
using System.IO;
using UnityEngine;

namespace ThachSanh.Systems
{
    /// <summary>
    /// Dữ liệu lưu game - có thể mở rộng thêm khi cần.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int chapterIndex;
        public int karma;
        public float playerPosX;
        public float playerPosY;
        public float playerPosZ;
        public string saveTimestamp;

        public Vector3 PlayerPosition => new Vector3(playerPosX, playerPosY, playerPosZ);

        public void SetPlayerPosition(Vector3 pos)
        {
            playerPosX = pos.x;
            playerPosY = pos.y;
            playerPosZ = pos.z;
        }
    }

    /// <summary>
    /// Hệ thống lưu/load dữ liệu game.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveFileName = "save.json";

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// Kiểm tra có dữ liệu save hay không.
        /// </summary>
        public static bool HasSaveData()
        {
            return File.Exists(SavePath);
        }

        /// <summary>
        /// Lưu game.
        /// </summary>
        public static void SaveGame(GameSaveData data)
        {
            if (data == null) return;

            data.saveTimestamp = DateTime.Now.ToString("o");
            string json = JsonUtility.ToJson(data, prettyPrint: true);

            try
            {
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveSystem] Đã lưu game tại: {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi lưu: {e.Message}");
            }
        }

        /// <summary>
        /// Load game. Trả về null nếu không có save hoặc lỗi.
        /// </summary>
        public static GameSaveData LoadGame()
        {
            if (!HasSaveData())
            {
                Debug.Log("[SaveSystem] Không có dữ liệu save.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log($"[SaveSystem] Đã load game - Chương {data.chapterIndex}, Karma {data.karma}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi load: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Xóa dữ liệu save (dùng khi New Game).
        /// </summary>
        public static void DeleteSave()
        {
            if (!HasSaveData()) return;

            try
            {
                File.Delete(SavePath);
                Debug.Log("[SaveSystem] Đã xóa save.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Lỗi khi xóa save: {e.Message}");
            }
        }
    }
}
