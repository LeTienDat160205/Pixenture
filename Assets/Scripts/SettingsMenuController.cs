using UnityEngine;
using UnityEngine.UI;

/// <summary>Connects a scene's Setting button and authored settings panel.</summary>
public sealed class SettingsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string BgmKey = "BGMVolume";
    private const string SfxKey = "SFXVolume";

    private void Awake()
    {
        ResolveSceneReferences();

        if (settingsPanel == null || musicSlider == null || sfxSlider == null)
        {
            Debug.LogError(
                $"Settings menu references are incomplete. Panel: {settingsPanel != null}, " +
                $"BGM slider: {musicSlider != null}, SFX slider: {sfxSlider != null}. " +
                "Assign them in the Inspector or name the scene objects SettingPanel, " +
                "MusicVolumeSlider, and SFXVolumeSlider.", this);
            enabled = false;
            return;
        }

        musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(BgmKey, musicSlider.value));
        sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(SfxKey, sfxSlider.value));
        musicSlider.onValueChanged.AddListener(MusicManager.SetVolume);
        sfxSlider.onValueChanged.AddListener(SoundEffectManager.SetVolume);
        GetComponent<Button>().onClick.AddListener(ToggleSettings);
        settingsPanel.SetActive(false);
    }

    private void ResolveSceneReferences()
    {
        foreach (Transform sceneTransform in FindObjectsByType<Transform>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (settingsPanel == null && sceneTransform.name == "SettingPanel")
                settingsPanel = sceneTransform.gameObject;
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
    }

    private void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
}
