# Kế hoạch Lead Dev – Hệ thống Nghiệp, Chuyển chương, Save/Load

**Phạm vi:** `Scripts/Systems/`, `ProjectSettings/`  
**Nhiệm vụ:** Hệ thống Nghiệp báo (Karma), Logic chuyển chương truyện, Lưu trữ dữ liệu (Save/Load)

---

## 1. Tổng quan kiến trúc

```
┌─────────────────────────────────────────────────────────────────┐
│                    PERSISTENT SYSTEMS (DontDestroyOnLoad)         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │
│  │ KarmaManager │  │  SaveSystem   │  │ GameStateManager     │   │
│  │ (đã có)      │  │  (mới)       │  │ (optional - tùy chọn) │   │
│  └──────┬───────┘  └──────┬───────┘  └──────────────────────┘   │
│         │                 │                                      │
│         └────────┬────────┘                                      │
│                  │                                               │
│         LevelLoader (static) ────────────────────────────────────┤
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│  SCENES: MainMenu → Chuong1_GocDa → 02_MieuChanTinh → ...        │
│  MainMenuController (gắn vào Canvas) - gọi LevelLoader, SaveSystem│
└─────────────────────────────────────────────────────────────────┘
```

---

## 2. Thứ tự triển khai

| Bước | Công việc | File cần tạo/sửa | Ước lượng |
|------|-----------|------------------|------------|
| 1 | SaveSystem – lưu/load dữ liệu game | `SaveSystem.cs` | 1–2 giờ |
| 2 | Mở rộng KarmaManager – tích hợp SaveSystem | `KarmaManager.cs` | 30 phút |
| 3 | LevelLoader – chuyển scene theo chương | `LevelLoader.cs` | 1 giờ |
| 4 | Cập nhật EditorBuildSettings | `EditorBuildSettings.asset` | 15 phút |
| 5 | MainMenuController – kết nối nút UI | `MainMenuController.cs` (mới) | 1 giờ |
| 6 | Tích hợp vào scene | MainMenu, Chuong1_GocDa | 30 phút |

---

## 3. Chi tiết từng bước

### Bước 1: SaveSystem

**Mục tiêu:** Lưu/load dữ liệu game (chương, karma, vị trí player, thời gian).

**Cấu trúc dữ liệu:**

```csharp
[System.Serializable]
public class GameSaveData
{
    public int chapterIndex;      // 0 = Chương 1, 1 = Chương 2, ...
    public int karma;
    public float playerPosX, playerPosY, playerPosZ;
    public string saveTimestamp;  // ISO format
}
```

**API cần có:**

- `SaveGame()` – lưu vào `Application.persistentDataPath/save.json`
- `LoadGame()` – đọc file, trả về `GameSaveData` hoặc `null` nếu không có save
- `HasSaveData()` – kiểm tra có file save hay không
- `DeleteSave()` – xóa save (New Game)

**Lưu ý:** Dùng `JsonUtility.ToJson()` / `JsonUtility.FromJson()` (Unity built-in).

---

### Bước 2: Mở rộng KarmaManager

**Thay đổi:**

1. Gọi `SaveSystem` khi karma thay đổi (hoặc khi chuyển scene).
2. Load karma từ `SaveSystem` khi load game.
3. Bỏ hoặc bọc `Input.GetKeyDown(KeyCode.K)` trong `#if UNITY_EDITOR` để không ảnh hưởng build.
4. Thêm event `OnKarmaChanged` (optional) cho UI hoặc logic khác.

**Luồng:**

- **New Game:** `SetKarma(100)` (giá trị mặc định).
- **Load Game:** `SetKarma(SaveSystem.LoadGame().karma)`.

---

### Bước 3: LevelLoader

**Mục tiêu:** Chuyển scene theo index chương.

**Cách làm:**

```csharp
// Danh sách scene (khớp với thứ tự trong Build Settings)
public static readonly string[] ChapterScenes = new string[]
{
    "Chuong1_GocDa",      // Index 0
    "02_MieuChanTinh",    // Index 1 (khi có scene)
    "03_HangDaiBang"      // Index 2 (khi có scene)
};

public static void LoadChapter(int chapterIndex) { ... }
public static void LoadMainMenu() { ... }
```

**Lưu ý:**

- Dùng `SceneManager.LoadScene(sceneName, LoadSceneMode.Single)`.
- Cần `using UnityEngine.SceneManagement`.
- Tên scene phải khớp với tên file `.unity` (không có extension).

---

### Bước 4: EditorBuildSettings

**Cập nhật thứ tự scene trong Build Settings:**

1. `MainMenu` (index 0)
2. `Chuong1_GocDa` (index 1)
3. `SampleScene` (nếu còn dùng) hoặc thay bằng `02_MieuChanTinh`, `03_HangDaiBang` khi có scene.

**Cách làm:**  
File → Build Settings → kéo thả scene vào danh sách, sắp xếp đúng thứ tự.

---

### Bước 5: MainMenuController

**Mục tiêu:** Script gắn vào Canvas trong MainMenu, kết nối các nút.

**Nút cần xử lý:**

| Nút | Hành động |
|-----|-----------|
| New Game | `SaveSystem.DeleteSave()` (nếu có) → `LevelLoader.LoadChapter(0)` |
| Load Game | `SaveSystem.LoadGame()` → restore data → `LevelLoader.LoadChapter(data.chapterIndex)` |
| Continue Game | Giống Load Game (hoặc load slot mới nhất) |
| Settings | Mở panel Settings (có thể để sau) |
| Quit Game | `Application.Quit()` |

**Cách gắn:**

1. Tạo script `MainMenuController.cs` trong `Scripts/UI/` hoặc `Scripts/Systems/`.
2. Thêm component vào GameObject (ví dụ Canvas hoặc empty object).
3. Trong Inspector, kéo reference các Button vào các field `public Button btnNewGame`, `btnLoadGame`, v.v.
4. Trong `Start()` hoặc `Awake()`: `btnNewGame.onClick.AddListener(OnNewGameClicked);`

**Kiểm tra Load Game:**

- Nếu `!SaveSystem.HasSaveData()` → disable nút Load/Continue hoặc hiện thông báo "Chưa có dữ liệu".

---

### Bước 6: Tích hợp vào scene

**MainMenu:**

1. Thêm `MainMenuController` vào Canvas (hoặc object phù hợp).
2. Gán reference các Button cho MainMenuController.
3. Đảm bảo scene MainMenu có trong Build Settings (index 0).

**Chuong1_GocDa:**

1. Đảm bảo có GameObject chứa `KarmaManager` (đã có trong settingthachsanh hoặc scene).
2. Tạo prefab **PersistentSystems** (optional nhưng nên dùng):
   - Empty GameObject tên "PersistentSystems"
   - Thêm `KarmaManager`, `SaveSystem` (nếu là MonoBehaviour)
   - `DontDestroyOnLoad` cho root
   - Đặt prefab trong scene đầu tiên chơi được (Chuong1_GocDa) hoặc load từ MainMenu

**Lưu ý:**  
`KarmaManager` đã dùng `DontDestroyOnLoad`, nên chỉ cần có trong 1 scene (ví dụ Chuong1_GocDa). Khi load từ MainMenu → Chuong1_GocDa, cần đảm bảo PersistentSystems được khởi tạo. Có 2 cách:

- **Cách A:** Đặt PersistentSystems trong MainMenu, khi New Game/Load Game thì nó vẫn tồn tại.
- **Cách B:** Đặt PersistentSystems trong Chuong1_GocDa; khi Load Game thì load Chuong1_GocDa trước (hoặc scene có PersistentSystems).

Khuyến nghị: Đặt PersistentSystems trong **MainMenu** (scene đầu tiên), để nó luôn tồn tại từ đầu.

---

## 4. Cấu trúc file đề xuất

```
Assets/Scripts/
├── Systems/
│   ├── KarmaManager.cs      (sửa)
│   ├── SaveSystem.cs        (viết mới)
│   ├── LevelLoader.cs       (viết mới)
│   └── GameSaveData.cs      (có thể gộp vào SaveSystem.cs)
├── UI/
│   └── MainMenuController.cs (viết mới)
```

---

## 5. Luồng chạy tổng thể

### New Game

1. User bấm "New Game" trên MainMenu.
2. `MainMenuController.OnNewGameClicked()`:
   - `SaveSystem.DeleteSave()` (nếu có)
   - `LevelLoader.LoadChapter(0)` → load Chuong1_GocDa
3. Chuong1_GocDa load → `KarmaManager` (nếu có trong scene) hoặc đã có sẵn từ MainMenu.
4. `KarmaManager` khởi tạo với `ChangeKarma(100)` (New Game).

### Load Game

1. User bấm "Load Game" hoặc "Continue".
2. `MainMenuController`:
   - Kiểm tra `SaveSystem.HasSaveData()`
   - Nếu có: `var data = SaveSystem.LoadGame()`
   - `KarmaManager.Instance?.SetKarma(data.karma)` (nếu đã có Instance)
   - `LevelLoader.LoadChapter(data.chapterIndex)`
3. Khi scene load xong, cần script trong scene game để:
   - Gọi `SaveSystem` lấy `playerPos` và đặt lại vị trí Player.
   - Hoặc dùng `LevelLoader` truyền thêm thông tin spawn point.

### Lưu game trong lúc chơi

- Cần điểm lưu (checkpoint) hoặc nút Save.
- Khi lưu: thu thập `chapterIndex` (từ LevelLoader hoặc config), `KarmaManager.GetCurrentKarma()`, vị trí Player → gọi `SaveSystem.SaveGame(data)`.

---

## 6. Lưu ý kỹ thuật

1. **Scene name:** Dùng tên scene không có `.unity`, ví dụ `"Chuong1_GocDa"` chứ không phải `"Chuong1_GocDa.unity"`.
2. **Build Settings:** Scene phải được thêm vào Build Settings thì `LoadScene` mới hoạt động.
3. **02_MieuChanTinh, 03_HangDaiBang:** Hiện chưa có file scene. Có thể tạo scene trống hoặc tạm dùng Chuong1_GocDa cho mọi chương, sau đó thay thế.
4. **SampleScene:** Trong Build Settings đang có SampleScene. Cần xác nhận có còn dùng không; nếu không thì bỏ hoặc thay bằng Chuong1_GocDa.

---

## 7. Checklist triển khai

- [ ] Viết `SaveSystem.cs` với `GameSaveData`, `SaveGame`, `LoadGame`, `HasSaveData`, `DeleteSave`
- [ ] Cập nhật `KarmaManager.cs` (tích hợp Save, bỏ/ẩn test phím K)
- [ ] Viết `LevelLoader.cs` với `LoadChapter`, `LoadMainMenu`
- [ ] Cập nhật EditorBuildSettings (thêm Chuong1_GocDa, sắp xếp thứ tự)
- [ ] Viết `MainMenuController.cs` và gắn vào MainMenu
- [ ] Gán reference Button cho MainMenuController
- [ ] Đặt PersistentSystems (KarmaManager) trong MainMenu hoặc Chuong1_GocDa
- [ ] Test: New Game → vào Chuong1_GocDa
- [ ] Test: Chơi → Save (khi có checkpoint) → Quit → Load Game
- [ ] Test: Karma thay đổi → Save → Load → karma được restore

---

## 8. Đã triển khai (Implementation)

Các file đã được tạo/cập nhật:

| File | Trạng thái |
|------|------------|
| `Scripts/Systems/SaveSystem.cs` | ✅ Hoàn thành |
| `Scripts/Systems/LevelLoader.cs` | ✅ Hoàn thành |
| `Scripts/Systems/KarmaManager.cs` | ✅ Cập nhật (test phím K chỉ trong Editor) |
| `Scripts/UI/MainMenuController.cs` | ✅ Hoàn thành |
| `Scripts/Systems/GameSceneBootstrap.cs` | ✅ Hoàn thành |
| `ProjectSettings/EditorBuildSettings.asset` | ✅ Chuong1_GocDa thay SampleScene |
| `Scenes/00_MainMenu/MainMenu.unity` | ✅ MainMenuController gắn vào Canvas |
| `Scenes/01_GocDa/Chuong1_GocDa.unity` | ✅ GameSceneBootstrap gắn vào _GameManager |

**Lưu ý:** Đảm bảo nhân vật chơi có tag `"Player"` hoặc component `PlayerCombat` để Load Game restore đúng vị trí. Phím **F5** dùng để lưu game khi đang chơi.

---

## 9. Mở rộng sau này

- Nhiều slot save (SaveSlot1, SaveSlot2, ...)
- Auto-save khi chuyển chương
- UI hiển thị Karma
- Điều kiện chuyển chương dựa trên Karma (ví dụ: Karma < -50 mới mở đường tới Lý Thông)
