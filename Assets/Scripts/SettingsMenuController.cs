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
        if (settingsPanel == null || musicSlider == null || sfxSlider == null)
        {
            Debug.LogError("SettingsMenuController needs its panel and both volume sliders assigned.", this);
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
