using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager Instance;
    private AudioSource audioSource;
    public AudioClip backgroundMusic;
    private const string VolumeKey = "BGMVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
            if (audioSource != null)
                audioSource.volume = PlayerPrefs.GetFloat(VolumeKey, audioSource.volume);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic(false, backgroundMusic);
        }
    }

    public static void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumeKey, volume);
        if (Instance != null && Instance.audioSource != null)
            Instance.audioSource.volume = volume;
    }

    public static void PlayBackgroundMusic(bool resetSong, AudioClip audioClip = null)
    {
        if (Instance == null || Instance.audioSource == null) return;

        if (audioClip != null)
            Instance.audioSource.clip = audioClip;

        if (Instance.audioSource.clip == null) return;

        if (resetSong || !Instance.audioSource.isPlaying)
            Instance.audioSource.Stop();

        Instance.audioSource.loop = true;
        Instance.audioSource.Play();
    }

    public static void PauseBackgroundMusic()
    {
        if (Instance != null && Instance.audioSource != null)
            Instance.audioSource.Pause();
    }
}
