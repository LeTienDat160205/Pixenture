using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public const string SelectedLevelKey = "SelectedLevelIndex";

    private const string MainSceneName = "MainScene";
    private int selectedLevelIndex = -1;
    private TMP_Text playText;
    private Button playButton;

    private void Awake()
    {
        GameObject playTextObject = GameObject.Find("PlayText");
        if (playTextObject != null)
            playText = playTextObject.GetComponent<TMP_Text>();

        GameObject playButtonObject = GameObject.Find("PlayButton");
        if (playButtonObject != null)
            playButton = playButtonObject.GetComponent<Button>();

        if (playButton != null)
        {
            playButton.interactable = false;
            playButton.onClick.AddListener(PlaySelectedLevel);
        }
        else
            Debug.LogError("LevelManager: khong tim thay PlayButton.", this);

        if (playText != null) playText.text = "Chọn level";
        else Debug.LogWarning("LevelManager: khong tim thay PlayText.", this);

        BindLevelButton("Level_1", 0);
        BindLevelButton("Level_2", 1);
    }

    private void BindLevelButton(string buttonName, int levelIndex)
    {
        GameObject buttonObject = GameObject.Find(buttonName);
        if (buttonObject == null || !buttonObject.TryGetComponent(out Button button))
        {
            Debug.LogWarning($"LevelManager: khong tim thay nut {buttonName}.", this);
            return;
        }

        button.onClick.AddListener(() => SelectLevel(levelIndex));
    }

    public void SelectLevel(int levelIndex)
    {
        if (levelIndex < 0)
        {
            Debug.LogError("LevelManager: chi so level khong hop le.", this);
            return;
        }

        selectedLevelIndex = levelIndex;
        if (playText != null) playText.text = $"Level {levelIndex + 1}";
        if (playButton != null) playButton.interactable = true;
    }

    public void PlaySelectedLevel()
    {
        if (selectedLevelIndex < 0) return;

        PlayerPrefs.SetInt(SelectedLevelKey, selectedLevelIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene(MainSceneName);
    }
}
