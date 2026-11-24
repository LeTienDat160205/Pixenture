using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>Connects a scene's Setting button and authored settings panel.</summary>
public sealed class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button settingExitButton;
    [SerializeField] private Button levelButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button restartButton;
    private const string BgmKey = "BGMVolume";
    private const string SfxKey = "SFXVolume";

    private void Awake()
    {
        ResolveSceneReferences();

        if (settingsPanel == null || musicSlider == null || sfxSlider == null ||
            settingButton == null || settingExitButton == null)
        {
            Debug.LogError(
                $"Settings menu references are incomplete. Panel: {settingsPanel != null}, " +
                $"BGM slider: {musicSlider != null}, SFX slider: {sfxSlider != null}, " +
                $"open button: {settingButton != null}, close button: {settingExitButton != null}.", this);
            enabled = false;
            return;
        }

        musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(BgmKey, musicSlider.value));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxKey, sfxSlider.value));
        musicSlider.onValueChanged.AddListener(MusicManager.SetVolume);
        sfxSlider.onValueChanged.AddListener(SoundEffectManager.SetVolume);
        settingButton.onClick.AddListener(OpenSettings);
        settingExitButton.onClick.AddListener(CloseSettings);
        if (levelButton != null) levelButton.onClick.AddListener(OpenLevelScene);
        if (backButton != null) backButton.onClick.AddListener(GoBack);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        settingsPanel.SetActive(false);
    }

    private void ResolveSceneReferences()
    {
        foreach (Transform sceneTransform in FindObjectsByType<Transform>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (settingsPanel == null && sceneTransform.name == "SettingPanel")
                settingsPanel = sceneTransform.gameObject;

            if (settingButton == null && sceneTransform.name == "SettingButton")
                settingButton = sceneTransform.GetComponent<Button>();

            if (levelButton == null && sceneTransform.name == "LevelButton")
                levelButton = sceneTransform.GetComponent<Button>();
            if (backButton == null && sceneTransform.name == "BackButton")
                backButton = sceneTransform.GetComponent<Button>();
            if (restartButton == null && sceneTransform.name == "RestartButton")
                restartButton = sceneTransform.GetComponent<Button>();

            if (settingExitButton == null &&
                (sceneTransform.name == "SettingExitButton" ||
                 sceneTransform.name == "SetttingExitButton"))
                settingExitButton = sceneTransform.GetComponent<Button>();
        }

        foreach (Slider sceneSlider in FindObjectsByType<Slider>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string objectName = sceneSlider.gameObject.name.Trim();
            if (musicSlider == null && objectName == "MusicVolumeSlider")
                musicSlider = sceneSlider;
            else if (sfxSlider == null && objectName == "SFXVolumeSlider")
                sfxSlider = sceneSlider;
        }
    }

    private void OnDestroy()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(MusicManager.SetVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SoundEffectManager.SetVolume);
        if (settingButton != null)
            settingButton.onClick.RemoveListener(OpenSettings);
        if (settingExitButton != null)
            settingExitButton.onClick.RemoveListener(CloseSettings);
        if (levelButton != null) levelButton.onClick.RemoveListener(OpenLevelScene);
        if (backButton != null) backButton.onClick.RemoveListener(GoBack);
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
    }

    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    private void OpenLevelScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelScene");
    }

    private void GoBack()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainScene") OpenLevelScene();
        else if (sceneName == "LevelScene") SceneManager.LoadScene("StartScene");
    }

    private void Restart()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainScene")
        {
            GameController gameController = FindFirstObjectByType<GameController>();
            if (gameController != null)
            {
                PlayerPrefs.SetInt(LevelManager.SelectedLevelKey, gameController.CurrentLevelIndex);
                PlayerPrefs.Save();
            }
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
