using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image heartPrefab;
    public Sprite fullHeartSprite;
    public Sprite brokenHeartSprite;

    private List<Image> hearts = new List<Image>();

    public void SetMaxHearts(int maxHearts)
    {
        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            if (hearts[i] != null) Destroy(hearts[i].gameObject);
        }

        hearts.Clear();

        if (heartPrefab == null)
        {
            Debug.LogError("HealthUI chưa được gán Heart Prefab.", this);
            return;
        }

        for (int i = 0; i < maxHearts; i++)
        {
            Image newHeart = Instantiate(heartPrefab, transform);
            newHeart.sprite = fullHeartSprite;
            hearts.Add(newHeart);
        }
    }

    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] == null) continue;
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeartSprite;
            }
            else
            {
                hearts[i].sprite = brokenHeartSprite;
            }
        }
    }
}
