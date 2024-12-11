using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public enum GameState { MainMenu, SettingsMenu, LevelSelection, Playing, Paused }
    static public GameState CurrentState { get; private set; } = GameState.MainMenu;

    static private PlayerData playerData;

    static public float musicVolume = 1f;
    static public float sfxVolume = 1f;

    private const int totalLevels = 7;
    static private int currentLevel = 0;

    [Header("UI Manager Object (Assign in Inspector)")]
    public GameObject uiManagerObject;
    private UIManager uiManager;

    private AudioManager audioManager;

    [Header("Prefabs to spawn (Assign in Inspector)")]
    public GameObject ball;
    public GameObject ballCamera;
    
    [Header("Level specific settings (Assign in Inspector)")]
    public GameObject spawnPoint;
    public int strokeLimit = 3;

    private void Awake()
    {
        LoadPlayerData();

        //Attempt to get instances
        uiManager = uiManagerObject.GetComponent<UIManager>();
        audioManager = AudioManager.Instance;

        if (uiManager == null)
            Debug.LogError("UIManager.Instance not found in GameManager Awake.");
        if (audioManager == null)
            Debug.LogError("AudioManager.Instance not found in GameManager Awake.");
        if (spawnPoint == null && CurrentState != GameState.MainMenu)
            Debug.LogWarning("No Spawn Point provided.");

        //FPS invariant sensitivity
        Cinemachine.CinemachineCore.GetInputAxis = (axisName) => Input.GetAxis(axisName) / Time.unscaledDeltaTime;
    }

    private void Start()
    {
        switch (CurrentState)
        {
        case GameState.MainMenu:
            uiManager.ShowMainMenu();
            ApplySettings();
            break;
        case GameState.Playing:
            uiManager.ShowHUD();
            ball = Instantiate(ball, spawnPoint.transform.position, spawnPoint.transform.rotation);
            ballCamera = Instantiate(ballCamera, spawnPoint.transform.position, spawnPoint.transform.rotation);
            break;
        default:
            Debug.LogError("Unexpected starting state " + CurrentState + " for GameManager.");
            break;
        }

    }

    #region Game State Management

    public void StartGame(int levelNumber)
    {
        if (levelNumber > playerData.currentLevel)
        {
            Debug.LogWarning("Level " + levelNumber + "isn't unlocked yet.");
            return;
        }

        CurrentState = GameState.Playing;
        currentLevel = levelNumber;
        StartCoroutine(LoadLevelAsync("Level" + levelNumber));

        //May not be necessary, managers have switched to instances now
        //uiManager.ShowHUD();
        //uiManager.UpdateLevelNumber(currentLevel); <- This needs to be accommodated for however
        //Best option would be public variable in UI Manager assigned in editor for each level
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    #endregion

    #region Level Management

    public bool IsLevelUnlocked(int levelNumber)
    {
        return playerData.currentLevel >= levelNumber;
    }

    public void UnlockNextLevel()
    {
        if (CurrentState != GameState.Playing)
        {
            return;
        }

        int nextLevel = currentLevel + 1;

        if (nextLevel < totalLevels && playerData.currentLevel < nextLevel)
        {
            playerData.currentLevel = nextLevel;
            SavePlayerData();
            uiManager.SetLevelLock(nextLevel, true);
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
        public int currentLevel = 0; //0-based indexing
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
