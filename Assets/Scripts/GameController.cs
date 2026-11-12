using System;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    int processAmount;
    public Slider processSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        processAmount = 0;
        processSlider.value = 0;
        Fruit.OnFuitCollect += IncreaseProcessAmount;
    }

    void IncreaseProcessAmount(int amount)
    {
        processAmount += amount;
        processSlider.value = processAmount;
        if (processAmount >= 100)
        {
            // Level up
            Debug.Log("Level Up!");
        }
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
