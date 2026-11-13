using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private int processAmount;
    public Slider processSlider;

    public GameObject player;
    public GameObject LoadCanvas;
    public List<GameObject> levels;
    private int currentLevelIndex;

    public GameObject gameOverScreen;
    public TMP_Text survivedText;
    private int survivedLevelCount;

    public static event Action OnReset;

    private void Start()
    {
        processAmount = 0;
        if (processSlider != null) processSlider.value = 0;

        Fruit.OnFuitCollect -= IncreaseProcessAmount;
        Fruit.OnFuitCollect += IncreaseProcessAmount;
        HoldToLoadLevel.OnHoldComplete -= LoadNextLevel;
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
        PlayerHealth.OnPlayedDied -= GameOverScreen;
        PlayerHealth.OnPlayedDied += GameOverScreen;

        if (LoadCanvas != null) LoadCanvas.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);

        // Đồng bộ chỉ số với level đang active khi khởi tạo.
        int activeIndex = FindActiveLevelIndex();
        if (activeIndex >= 0) currentLevelIndex = activeIndex;
    }

    private void GameOverScreen()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
        MusicManager.PauseBackgroundMusic();
        if (survivedText != null)
            survivedText.text = "You survived " + survivedLevelCount + (survivedLevelCount == 1 ? " level" : " levels");
        Time.timeScale = 0f;
    }

    public void ResetGame()
    {
        Time.timeScale = 1f;
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        MusicManager.PlayBackgroundMusic(false);
        survivedLevelCount = 0;
        if (!SetActiveLevel(0)) return;
        ResetLevelProgress();
        if (player != null) player.transform.position = Vector3.zero;
        if (LoadCanvas != null) LoadCanvas.SetActive(false);

        OnReset?.Invoke();
    }

    private void IncreaseProcessAmount(int amount)
    {
        processAmount += amount;
        if (processSlider != null) processSlider.value = processAmount;
        if (processAmount >= 100 && LoadCanvas != null)
        {
            LoadCanvas.SetActive(true);
            Debug.Log("Level Up!");
        }
    }

    private void LoadLevel(int level, bool wantSurvivedIncrease)
    {
        if (!SetActiveLevel(level)) return;

        if (wantSurvivedIncrease) SoundEffectManager.Play("LevelComplete");

        if (LoadCanvas != null) LoadCanvas.SetActive(false);
        if (player != null) player.transform.position = Vector3.zero;
        ResetLevelProgress();

        survivedLevelCount++;
        if (wantSurvivedIncrease) survivedLevelCount++;
    }

    private bool SetActiveLevel(int level)
    {
        if (levels == null || levels.Count == 0 || level < 0 || level >= levels.Count || levels[level] == null)
        {
            Debug.LogError("GameController: level index/list không hợp lệ hoặc level chưa được gán.", this);
            return false;
        }

        // Tắt tất cả trước khi bật level đích, không phụ thuộc currentLevelIndex.
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] != null) levels[i].SetActive(false);
        }

        levels[level].SetActive(true);
        currentLevelIndex = level;
        return true;
    }

    private void ResetLevelProgress()
    {
        processAmount = 0;
        if (processSlider != null) processSlider.value = 0;
    }

    private int FindActiveLevelIndex()
    {
        if (levels == null) return -1;
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] != null && levels[i].activeSelf) return i;
        }
        return -1;
    }

    private void LoadNextLevel()
    {
        if (levels == null || levels.Count == 0) return;
        int nextLevelIndex = (currentLevelIndex + 1) % levels.Count;
        LoadLevel(nextLevelIndex, true);
    }

    private void OnDestroy()
    {
        Fruit.OnFuitCollect -= IncreaseProcessAmount;
        HoldToLoadLevel.OnHoldComplete -= LoadNextLevel;
        PlayerHealth.OnPlayedDied -= GameOverScreen;
    }
}
