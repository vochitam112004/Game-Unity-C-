using UnityEngine;
using UnityEngine.UI;
using ThachSanh.Systems;

/// <summary>
/// Điều khiển Main Menu - kết nối các nút New Game, Load Game, Continue, Settings, Quit.
/// Gắn script này vào Canvas trong scene MainMenu.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Nút (có thể kéo thả trong Inspector, hoặc để trống để tự tìm)")]
    [SerializeField] private Button btnNewGame;
    [SerializeField] private Button btnLoadGame;
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnQuit;

    private void Awake()
    {
        FindButtonsIfNeeded();
        FindButtonsByGameObjectName();
        SetupListeners();
        UpdateButtonStates();
    }

    private void FindButtonsIfNeeded()
    {
        Transform buttons = transform.Find("Buttons");
        if (buttons == null) return;

        if (btnNewGame == null) btnNewGame = buttons.Find("New Game")?.GetComponent<Button>();
        if (btnLoadGame == null) btnLoadGame = buttons.Find("Load Game")?.GetComponent<Button>();
        if (btnContinue == null) btnContinue = buttons.Find("Continue Game")?.GetComponent<Button>();
        if (btnSettings == null) btnSettings = buttons.Find("Settings")?.GetComponent<Button>();
        if (btnQuit == null) btnQuit = buttons.Find("Quit Game")?.GetComponent<Button>();
    }

    /// <summary>
    /// Fallback: tìm nút theo tên GameObject (không cần TMPro).
    /// </summary>
    private void FindButtonsByGameObjectName()
    {
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (var btn in allButtons)
        {
            if (btn == null) continue;
            string name = btn.gameObject.name;

            if (btnNewGame == null && name == "New Game") btnNewGame = btn;
            else if (btnLoadGame == null && name == "Load Game") btnLoadGame = btn;
            else if (btnContinue == null && (name == "Continue Game" || name == "Extras")) btnContinue = btn;
            else if (btnSettings == null && name == "Settings") btnSettings = btn;
            else if (btnQuit == null && name == "Quit Game") btnQuit = btn;
        }
    }

    private void SetupListeners()
    {
        if (btnNewGame) { btnNewGame.onClick.AddListener(OnNewGameClicked); Debug.Log("[MainMenu] Đã kết nối New Game"); }
        else Debug.LogWarning("[MainMenu] Không tìm thấy nút New Game!");
        if (btnLoadGame) btnLoadGame.onClick.AddListener(OnLoadGameClicked);
        if (btnContinue) btnContinue.onClick.AddListener(OnContinueClicked);
        if (btnSettings) btnSettings.onClick.AddListener(OnSettingsClicked);
        if (btnQuit) btnQuit.onClick.AddListener(OnQuitClicked);
    }

    private void UpdateButtonStates()
    {
        bool hasSave = SaveSystem.HasSaveData();
        if (btnLoadGame) btnLoadGame.interactable = hasSave;
        if (btnContinue) btnContinue.interactable = hasSave;
    }

    private void OnNewGameClicked()
    {
        SaveSystem.DeleteSave();
        LevelLoader.LoadChapter(0);
    }

    private void OnLoadGameClicked()
    {
        if (!SaveSystem.HasSaveData())
        {
            Debug.Log("[MainMenu] Chưa có dữ liệu save.");
            return;
        }

        var data = SaveSystem.LoadGame();
        if (data != null)
        {
            LevelLoader.LoadGame(data);
        }
    }

    private void OnContinueClicked()
    {
        OnLoadGameClicked();
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[MainMenu] Settings - chưa triển khai.");
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
