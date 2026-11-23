
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase.Auth;

public class StartMenuNavigation : MonoBehaviour
{
    [Header("PANELS")]
    public GameObject gameModePanel;
    public GameObject settingsPanel;

    [Header("MAIN MENU BUTTONS")]
    public GameObject startButton;
    public GameObject settingsButton;
    public GameObject logoutButton;

    [Header("SCENE NAMES")]
    public string loginScene = "LoginRegister";
    public string singlePlayerScene = "GameplayTest";
    public string multiplayerScene = "MultiplayerLobby";

    [Header("AUDIO SETTINGS")]
    public Slider volumeSlider;

    // =====================================
    // 1. KHOI TAO START MENU
    // =====================================

    void Start()
    {
        // Chi cho phep vao menu khi da dang nhap
        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            LoadGameScene(loginScene);
            return;
        }

        ClosePanels();

        float volume =
            PlayerPrefs.GetFloat("MasterVolume", 1f);

        AudioListener.volume = volume;

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
        }

        Debug.Log("StartMenu READY!");
    }

    // =====================================
    // 2. MO BANG CHON CHE DO CHOI
    // =====================================

    public void OpenStartGame()
    {
        if (gameModePanel == null)
        {
            Debug.LogWarning(
                "Chua gan GameModePanel!"
            );
            return;
        }

        HideAllPanels();
        SetMainMenuButtons(false);

        gameModePanel.SetActive(true);

        Debug.Log("Open Game Mode Panel");
    }

    // =====================================
    // 3. SINGLE PLAYER
    // =====================================

    public void OpenSinglePlayer()
    {
        // GameplayTest hien moi la man thu nghiem.
        // Bot se duoc bo sung sau.
        LoadGameScene(singlePlayerScene);
    }

    // =====================================
    // 4. MULTIPLAYER
    // =====================================

    public void OpenMultiplayer()
    {
        // Vao sanh cho Multiplayer
        LoadGameScene(multiplayerScene);
    }

    // =====================================
    // 5. SETTINGS
    // =====================================

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning(
                "Chua tao SettingsPanel!"
            );
            return;
        }

        HideAllPanels();
        SetMainMenuButtons(false);

        settingsPanel.SetActive(true);
    }

    // =====================================
    // 6. CHINH AM LUONG
    // =====================================

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);

        AudioListener.volume = value;

        PlayerPrefs.SetFloat(
            "MasterVolume", value
        );

        PlayerPrefs.Save();
    }

    // =====================================
    // 7. NUT BACK
    // =====================================

    public void ClosePanels()
    {
        HideAllPanels();

        // Hien lai 3 nut menu chinh
        SetMainMenuButtons(true);
    }

    // =====================================
    // 8. AN CAC PANEL
    // =====================================

    private void HideAllPanels()
    {
        if (gameModePanel != null)
            gameModePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // =====================================
    // 9. AN / HIEN 3 NUT MENU
    // =====================================

    private void SetMainMenuButtons(bool show)
    {
        if (startButton != null)
            startButton.SetActive(show);

        if (settingsButton != null)
            settingsButton.SetActive(show);

        if (logoutButton != null)
            logoutButton.SetActive(show);
    }

    // =====================================
    // 10. LOGOUT
    // =====================================

    public void Logout()
    {
        FirebaseAuth auth =
            FirebaseAuth.DefaultInstance;

        if (auth != null)
        {
            auth.SignOut();
        }

        Debug.Log("LOGOUT SUCCESS!");

        LoadGameScene(loginScene);
    }

    // =====================================
    // 11. CHUYEN SCENE
    // =====================================

    private void LoadGameScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(
            sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError(
                "Scene chua co trong Build Settings: "
                + sceneName
            );
        }
    }
}
