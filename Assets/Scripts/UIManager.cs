using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private const string levelLockedObjectName = "LevelLocked";

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

    [Header("Game Manager Object (Assign in Inspector)")]
    public GameObject gameManagerObject;
    private GameManager gameManager;

    [Header("Buttons for each level (Assign in Inspector)")]
    public List<GameObject> levelButtons;

    private void Awake()
    {
        gameManager = gameManagerObject.GetComponent<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("gameManager not found in UIManager. Ensure GameManager is present in the scene.");
            return;
        }
    }

    private void Start()
    {
        // Check if sliders are assigned before using them
        if (musicVolumeSlider == null || sfxVolumeSlider == null)
        {
            Debug.LogWarning("MusicVolumeSlider or SfxVolumeSlider not assigned in UIManager. Please assign them in the Inspector.");
            return;
        }

        // Initialize volume sliders with the current values from GameManager
        musicVolumeSlider.value = GameManager.musicVolume;
        sfxVolumeSlider.value = GameManager.sfxVolume;

        // Add listeners to the sliders
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        for (int i = 0; i < levelButtons.Count; ++i)
        {
            bool isUnlocked = gameManager.IsLevelUnlocked(i);
            SetLevelLock(i, isUnlocked);
        }
    }

    #region UI Panel Management

    public void SetLevelLock(int level, bool isUnlocked)
    {
        if (level < 0 || level >= levelButtons.Count)
        {
            return;
        }

        levelButtons[level].GetComponent<Button>().enabled = isUnlocked;
        bool foundChildLocked = false;
        
        foreach (Transform child in levelButtons[level].transform)
        {
            if (child.name == levelLockedObjectName)
            {
                child.GetComponent<RawImage>().enabled = !isUnlocked;
                foundChildLocked = true;
                break;
            }
        }

        if (!foundChildLocked)
        {
            Debug.LogWarning("Failed to find level " + level + "'s locked image.");
        }
    }

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
        gameManager.SetMusicVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        gameManager.SetSFXVolume(value);
    }

    #endregion

    #region Button Handlers

    public void OnStartGameButtonPressed()
    {
        gameManager.OpenLevelSelection();
    }

    public void OnSettingsButtonPressed()
    {
        gameManager.OpenSettings();
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    public void OnResumeGameButtonPressed()
    {
        gameManager.ResumeGame();
    }

    public void OnRestartLevelButtonPressed()
    {
        gameManager.RestartLevel();
    }

    public void OnReturnToMainMenuButtonPressed()
    {
        gameManager.ReturnToMainMenu();
    }

    #endregion
}
