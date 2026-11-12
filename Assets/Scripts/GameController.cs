using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    int processAmount;
    public Slider processSlider;

    public GameObject player;
    public GameObject LoadCanvas;
    public List<GameObject> levels;
    private int currentLevelIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        processAmount = 0;
        processSlider.value = 0;
        Fruit.OnFuitCollect += IncreaseProcessAmount;
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
        LoadCanvas.SetActive(false);
    }



    void IncreaseProcessAmount(int amount)
    {
        processAmount += amount;
        processSlider.value = processAmount;
        if (processAmount >= 100)
        {
            // Level up
            LoadCanvas.SetActive(true);
            Debug.Log("Level Up!");
        }
    }

    void LoadNextLevel()
    {
        int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
        LoadCanvas.SetActive(false);

        levels[currentLevelIndex].gameObject.SetActive(false);
        levels[nextLevelIndex].gameObject.SetActive(true);

        player.transform.position = new Vector3(0, 0, 0);

        currentLevelIndex = nextLevelIndex;
        processAmount = 0;
        processSlider.value = 0;
    }
    // được thêm bơi AI khắc phục lỗi không phá hủy sự kiện khi GameController bị hủy
    public void OnDestroy()
    {
        Fruit.OnFuitCollect -= IncreaseProcessAmount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
