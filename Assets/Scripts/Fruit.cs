using UnityEngine;
using System;

public class Fruit : MonoBehaviour, IItem
{
    public static event Action<int> OnFuitCollect;
    public int worth = 5;
    private bool collected;

    public void Collect()
    {
        if (collected) return;
        collected = true;

        OnFuitCollect?.Invoke(worth);
        SoundEffectManager.Play("Fruit");
        Destroy(gameObject);
    }

}
