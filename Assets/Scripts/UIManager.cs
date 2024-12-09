using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels (Assign in Inspector)")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject levelSelectionPanel;
    public GameObject hudPanel;
    public GameObject pauseMenuPanel;
    public GameObject loadingScreenPanel;

    [Header("HUD Elements (Assign in Inspector)")]
    public TextMeshProUGUI strokeCountText;
    public TextMeshProUGUI levelNumberText;

    [Header("Loading Screen (Assign in Inspector)")]
    public Slider loadingProgressBar;

    [Header("Settings Elements (Assign in Inspector)")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: If you want UI to persist across scene loads:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Check if GameManager is available
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance not found at UIManager Start. Ensure GameManager is present in the scene.");
            return;
        }

        // Check if sliders are assigned before using them
        if (musicVolumeSlider == null || sfxVolumeSlider == null)
        {
            Debug.LogWarning("MusicVolumeSlider or SfxVolumeSlider not assigned in UIManager. Please assign them in the Inspector.");
            return;
        }

        // Initialize volume sliders with the current values from GameManager
        musicVolumeSlider.value = GameManager.Instance.musicVolume;
        sfxVolumeSlider.value = GameManager.Instance.sfxVolume;

        // Add listeners to the sliders
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    #region UI Panel Management

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
    }

    public void ShowSettingsMenu()
    {
        HideAllPanels();
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void ShowLevelSelection()
    {
        HideAllPanels();
        if (levelSelectionPanel) levelSelectionPanel.SetActive(true);
    }

    public void ShowHUD()
    {
        HideAllPanels();
        if (hudPanel) hudPanel.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        if (pauseMenuPanel)
        {
            pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f; // Pause the game
        }
        else
        {
            Debug.LogWarning("PauseMenuPanel not assigned in UIManager.");
        }
    }

    public void HidePauseMenu()
    {
        if (pauseMenuPanel)
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f; // Resume the game
        }
        else
        {
            Debug.LogWarning("PauseMenuPanel not assigned in UIManager.");
        }
    }

    public void ShowLoadingScreen()
    {
        HideAllPanels();
        if (loadingScreenPanel) loadingScreenPanel.SetActive(true);
    }

    public void HideLoadingScreen()
    {
        if (loadingScreenPanel) loadingScreenPanel.SetActive(false);
    }

    private void HideAllPanels()
    {
        // Safely hide each panel if assigned
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (loadingScreenPanel) loadingScreenPanel.SetActive(false);
    }

    #endregion

    #region HUD Updates

    public void UpdateStrokeCount(int strokes)
    {
        if (strokeCountText)
            strokeCountText.text = "Strokes: " + strokes;
        else
            Debug.LogWarning("StrokeCountText not assigned in UIManager.");
    }

    public void UpdateLevelNumber(int levelNumber)
    {
        if (levelNumberText)
            levelNumberText.text = "Level: " + levelNumber;
        else
            Debug.LogWarning("LevelNumberText not assigned in UIManager.");
    }

    public void UpdateLoadingProgress(float progress)
    {
        if (loadingProgressBar)
            loadingProgressBar.value = progress;
        else
            Debug.LogWarning("LoadingProgressBar not assigned in UIManager.");
    }

    #endregion

    #region Settings Management

    private void OnMusicVolumeChanged(float value)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetMusicVolume(value);
        else
            Debug.LogWarning("GameManager.Instance not found when changing music volume.");
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetSFXVolume(value);
        else
            Debug.LogWarning("GameManager.Instance not found when changing SFX volume.");
    }

    #endregion

    #region Button Handlers

    public void OnStartGameButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OpenLevelSelection();
        else
            Debug.LogWarning("GameManager.Instance not found when Start button pressed.");
    }

    public void OnSettingsButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OpenSettings();
        else
            Debug.LogWarning("GameManager.Instance not found when Settings button pressed.");
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    public void OnResumeGameButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
        else
            Debug.LogWarning("GameManager.Instance not found when Resume button pressed.");
    }

    public void OnRestartLevelButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartLevel();
        else
            Debug.LogWarning("GameManager.Instance not found when Restart button pressed.");
    }

    public void OnReturnToMainMenuButtonPressed()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
        else
            Debug.LogWarning("GameManager.Instance not found when ReturnToMainMenu button pressed.");
    }

    #endregion
}
