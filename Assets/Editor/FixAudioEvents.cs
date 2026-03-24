using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class FixAudioEvents
{
    [MenuItem("Tools/Sửa Lỗi Âm Thanh (Gắn Event Tự Động)")]
    public static void AddEvents()
    {
        // Quét TOÀN BỘ thư mục Assets trong Project
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        
        int cleanedCount = 0;
        int modifiedCount = 0;
        int readOnlyErrors = 0;
        string readOnlyList = "";

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Bỏ qua các thư mục nội bộ của Unity (Packages, v.v.)
            if (!path.StartsWith("Assets/")) continue;

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            bool isReadOnlyFBX = path.ToLower().EndsWith(".fbx");

            // Kiểm tra xem có cần dọn rác không
            AnimationEvent[] existingEvents = AnimationUtility.GetAnimationEvents(clip);
            bool hasEmptyEvent = false;
            foreach (var ev in existingEvents)
            {
                if (string.IsNullOrEmpty(ev.functionName) || ev.functionName.Trim() == "")
                {
                    hasEmptyEvent = true;
                    break;
                }
            }

            if (hasEmptyEvent)
            {
                if (isReadOnlyFBX)
                {
                    Debug.LogWarning($"[Phát hiện] Thấy 1 Event RỖNG ở clip '{clip.name}' bên trong file FBX: {path}. \n-> FIle FBX này chỉ có thể sửa bằng cách vào FBX Import Settings (tab Animation) tìm event bị lỗi và xóa đi.");
                    readOnlyErrors++;
                    readOnlyList += $"- {clip.name} (trong FBX)\n";
                }
                else
                {
                    Debug.LogWarning($"[Dọn dẹp] Đã xóa thành công Event rỗng ở file .anim: {path}");
                    CleanAndProcessClip(clip, false, false);
                    cleanedCount++;
                }
            }
            
            // Xử lý cắm cờ m thanh & Rìu riêng cho ThachSanh
            if (path.Contains("ThachSanh"))
            {
                bool isAttack = clip.name.Contains("ATK") || clip.name.Contains("combo") || clip.name.Contains("axeHit");
                bool isEquip = clip.name == "equip" || clip.name == "unequip";

                if (isAttack || isEquip)
                {
                    if (!isReadOnlyFBX && CleanAndProcessClip(clip, isAttack, isEquip))
                    {
                        modifiedCount++;
                    }
                }
            }
        }

        if (readOnlyErrors > 0)
        {
            EditorUtility.DisplayDialog("Vẫn còn lỗi ở FBX!", $"Đã dọn dẹp {cleanedCount} file, cắm cờ {modifiedCount} file.\n\nTUY NHIÊN, có {readOnlyErrors} file FBX đang chứa Event lỗi không thể xóa tự động! Hãy mở Console lên xem tên chức file FBX đó là gì nhé.", "Đã Hiểu");
        }
        else if (cleanedCount > 0 || modifiedCount > 0)
        {
            EditorUtility.DisplayDialog("Thành công!", $"HOÀN TẤT!\nĐã dọn dẹp lỗi rỗng 'Character' trên {cleanedCount} clip.\nCắm cờ âm thanh/vũ khí cho {modifiedCount} clip.\nGiờ game chắc chắn đã chạy bình thường!", "Tuyệt vời");
        }
        else
        {
            EditorUtility.DisplayDialog("Thông báo", "Toàn bộ Game đã sạch sẽ, không tìm thấy Event rỗng nào nữa cả!", "Đóng");
        }
    }

    private static bool CleanAndProcessClip(AnimationClip clip, bool isAttack, bool isEquip)
    {
        AnimationEvent[] existingEvents = AnimationUtility.GetAnimationEvents(clip);
        List<AnimationEvent> validEvents = new List<AnimationEvent>();
        bool requiresSave = false;

        foreach (var ev in existingEvents)
        {
            if (string.IsNullOrEmpty(ev.functionName) || ev.functionName.Trim() == "")
            {
                requiresSave = true; 
                continue; 
            }
            validEvents.Add(ev);
        }

        if (isAttack)
        {
            if (AddOrUpdateEvent(validEvents, 0.3f, "EnableHitbox")) requiresSave = true;
            if (AddOrUpdateEvent(validEvents, 0.3f, "PlaySwingSound")) requiresSave = true;
            if (AddOrUpdateEvent(validEvents, 0.8f, "DisableHitbox")) requiresSave = true;
        }

        if (isEquip)
        {
            float showAxeTime = clip.name == "equip" ? 0.3f : 0.5f;
            float soundTime = clip.name == "equip" ? 0.35f : 0.45f;
            
            if (AddOrUpdateEvent(validEvents, showAxeTime, "ShowAxe")) requiresSave = true;
            if (AddOrUpdateEvent(validEvents, soundTime, "PlayEquipSound")) requiresSave = true;
        }

        if (requiresSave)
        {
            AnimationUtility.SetAnimationEvents(clip, validEvents.ToArray());
            EditorUtility.SetDirty(clip);
            return true;
        }

        return false;
    }

    private static bool AddOrUpdateEvent(List<AnimationEvent> events, float time, string funcName)
    {
        bool changed = false;
        // Xóa event cũ nếu sai time
        for (int i = events.Count - 1; i >= 0; i--)
        {
            if (events[i].functionName == funcName)
            {
                if (Mathf.Abs(events[i].time - time) > 0.01f)
                {
                    events.RemoveAt(i);
                    changed = true;
                }
                else
                {
                    return false; // Đã có và đúng giờ
                }
            }
        }

        AnimationEvent newEv = new AnimationEvent();
        newEv.time = time;
        newEv.functionName = funcName;
        events.Add(newEv);
        return true;
    }
}
