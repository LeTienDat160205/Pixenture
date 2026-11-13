using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    private static SoundEffectManager Instance;

    private AudioSource audioSource;
    private SoundEffectLibrary soundEffectLibrary;
    private const string VolumeKey = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            if (audioSource != null)
                audioSource.volume = PlayerPrefs.GetFloat(VolumeKey, audioSource.volume);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Play(string soundName)
    {
        if (Instance == null || Instance.audioSource == null || Instance.soundEffectLibrary == null)
            return;

        AudioClip audioClip = Instance.soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
            Instance.audioSource.PlayOneShot(audioClip);
    }

    public static void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, volume);
        if (Instance != null && Instance.audioSource != null)
            Instance.audioSource.volume = volume;
    }
}
