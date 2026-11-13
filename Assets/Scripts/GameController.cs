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
    private bool checkpointTransitionInProgress;

    public GameObject gameOverScreen;
    public TMP_Text survivedText;
    private int survivedLevelCount;

    public static event Action OnReset;

    private void Start()
    {
        ResolveMissingLevels();
        processAmount = 0;
        if (processSlider != null) processSlider.value = 0;

        Fruit.OnFuitCollect -= IncreaseProcessAmount;
        Fruit.OnFuitCollect += IncreaseProcessAmount;
        PlayerHealth.OnPlayedDied -= GameOverScreen;
        PlayerHealth.OnPlayedDied += GameOverScreen;

        if (LoadCanvas != null) LoadCanvas.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);

        int activeIndex = FindActiveLevelIndex();
        currentLevelIndex = activeIndex >= 0 ? activeIndex : 0;
        if (levels != null && levels.Count > 0 && levels[currentLevelIndex] != null)
        {
            SetActiveLevel(currentLevelIndex);
            PlacePlayerAtLevelStart(currentLevelIndex);
        }
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
        checkpointTransitionInProgress = false;
        if (LoadCanvas != null)
        {
            LoadCanvas.GetComponent<HoldToLoadLevel>()?.CancelLoading();
            LoadCanvas.SetActive(false);
        }
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        MusicManager.PlayBackgroundMusic(false);
        survivedLevelCount = 0;
        if (!SetActiveLevel(0)) return;
        ResetLevelProgress();
        PlacePlayerAtLevelStart(0);

        OnReset?.Invoke();
    }

    private void IncreaseProcessAmount(int amount)
    {
        if (checkpointTransitionInProgress) return;

        processAmount = Mathf.Clamp(processAmount + amount, 0, 100);
        if (processSlider != null) processSlider.value = processAmount;
    }

    public bool TryBeginCheckpointTransition(CheckpointController checkpoint)
    {
        bool processIsFull = processAmount >= 100 ||
                             (processSlider != null && processSlider.value >= processSlider.maxValue);
        if (checkpoint == null || !processIsFull || checkpointTransitionInProgress)
            return false;

        if (LoadCanvas == null || !LoadCanvas.TryGetComponent(out HoldToLoadLevel loadCircle))
        {
            Debug.LogError("GameController needs HoldToLoadLevel on LoadCanvas.", this);
            return false;
        }

        checkpointTransitionInProgress = true;
        float animationDuration = checkpoint.PlayActivationAnimation();
        LoadCanvas.SetActive(true);
        if (!loadCircle.BeginLoading(animationDuration, LoadNextLevel))
        {
            checkpointTransitionInProgress = false;
            LoadCanvas.SetActive(false);
            return false;
        }
        return true;
    }

    private void LoadLevel(int level, bool wantSurvivedIncrease)
    {
        if (!SetActiveLevel(level))
        {
            checkpointTransitionInProgress = false;
            return;
        }

        checkpointTransitionInProgress = false;
        if (LoadCanvas != null)
        {
            LoadCanvas.GetComponent<HoldToLoadLevel>()?.CancelLoading();
            LoadCanvas.SetActive(false);
        }

        if (wantSurvivedIncrease) SoundEffectManager.Play("LevelComplete");

        PlacePlayerAtLevelStart(level);
        ResetLevelProgress();

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

    private void ResolveMissingLevels()
    {
        if (levels == null) return;

        Transform[] sceneTransforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] != null) continue;

            string expectedName = $"Level{i + 1}";
            foreach (Transform sceneTransform in sceneTransforms)
            {
                if (sceneTransform.name == expectedName)
                {
                    levels[i] = sceneTransform.gameObject;
                    break;
                }
            }
        }
    }

    private void PlacePlayerAtLevelStart(int levelIndex)
    {
        if (player == null || levels == null || levelIndex < 0 || levelIndex >= levels.Count ||
            levels[levelIndex] == null)
            return;

        Transform startPoint = null;
        foreach (Transform child in levels[levelIndex].GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Start")
            {
                startPoint = child;
                break;
            }
        }

        if (startPoint == null)
        {
            Debug.LogWarning($"{levels[levelIndex].name} has no child object named Start.", levels[levelIndex]);
            return;
        }

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = startPoint.position;
        }

        player.transform.position = startPoint.position;
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
        if (!checkpointTransitionInProgress || levels == null || levels.Count == 0) return;
        int nextLevelIndex = (currentLevelIndex + 1) % levels.Count;
        LoadLevel(nextLevelIndex, true);
    }

    private void OnDestroy()
    {
        Fruit.OnFuitCollect -= IncreaseProcessAmount;
        PlayerHealth.OnPlayedDied -= GameOverScreen;
    }
}
