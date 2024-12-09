using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, SettingsMenu, LevelSelection, Playing, Paused }
    static public GameState CurrentState { get; private set; }

    static private PlayerData playerData;

    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public int totalLevels = 7;
    public int currentLevel = 1;

    [Header("UI Manager Object (Assign in Inspector)")]
    public GameObject uiManagerObject;
    private UIManager uiManager;

    private AudioManager audioManager;

    private void Awake()
    {
        LoadPlayerData();

        // Attempt to get instances:
        uiManager = uiManagerObject.GetComponent<UIManager>();
        audioManager = AudioManager.Instance;

        if (uiManager == null)
            Debug.LogError("UIManager.Instance not found in GameManager Awake.");
        if (audioManager == null)
            Debug.LogError("AudioManager.Instance not found in GameManager Awake.");
    }

    private void Start()
    {
        CurrentState = GameState.MainMenu;
        uiManager.ShowMainMenu();

        ApplySettings();
    }

    #region Game State Management

    public void StartGame(int levelNumber)
    {
        currentLevel = levelNumber;
        CurrentState = GameState.Playing;
        StartCoroutine(LoadLevelAsync("Level" + currentLevel));
        uiManager.ShowHUD();
        uiManager.UpdateLevelNumber(currentLevel);
    }

    public void ReturnToMainMenu()
    {
        CurrentState = GameState.MainMenu;
        SceneManager.LoadScene("MainMenu");
        uiManager.ShowMainMenu();
    }

    public void OpenSettings()
    {
        CurrentState = GameState.SettingsMenu;
        uiManager.ShowSettingsMenu();
    }

    public void OpenLevelSelection()
    {
        CurrentState = GameState.LevelSelection;
        uiManager.ShowLevelSelection();
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            uiManager.ShowPauseMenu();
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            uiManager.HidePauseMenu();
        }
    }

    public void RestartLevel()
    {
        if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level" + currentLevel);
        }
    }

    #endregion

    #region Level Management

    public bool IsLevelUnlocked(int levelNumber)
    {
        return playerData.unlockedLevels.Contains(levelNumber);
    }

    public void UnlockNextLevel()
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel <= totalLevels && !playerData.unlockedLevels.Contains(nextLevel))
        {
            playerData.unlockedLevels.Add(nextLevel);
            SavePlayerData();
        }
    }

    public void LevelCompleted()
    {
        UnlockNextLevel();
        SavePlayerData();
        OpenLevelSelection();
    }

    #endregion

    #region Settings Management

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        audioManager.SetMusicVolume(volume);
        playerData.musicVolume = volume;
        SavePlayerData();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        audioManager.SetEffectsVolume(volume);
        playerData.sfxVolume = volume;
        SavePlayerData();
    }

    private void ApplySettings()
    {
        audioManager.SetMusicVolume(musicVolume);
        audioManager.SetEffectsVolume(sfxVolume);
    }

    #endregion

    #region Player Data Management

    [System.Serializable]
    public class PlayerData
    {
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public List<int> unlockedLevels = new List<int>();
    }

    private void LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/playerdata.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            playerData = new PlayerData();
            playerData.unlockedLevels.Add(1);
            SavePlayerData();
        }

        musicVolume = playerData.musicVolume;
        sfxVolume = playerData.sfxVolume;
    }

    private void SavePlayerData()
    {
        string path = Application.persistentDataPath + "/playerdata.json";
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(path, json);
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }

    #endregion

    #region Asynchronous Level Loading

    private IEnumerator LoadLevelAsync(string sceneName)
    {
        uiManager.ShowLoadingScreen();
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            uiManager.UpdateLoadingProgress(progress);
            yield return null;
        }

        uiManager.HideLoadingScreen();
    }

    #endregion
}
