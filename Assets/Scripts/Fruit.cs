using UnityEngine;
using System;

public class Fruit : MonoBehaviour, IItem
{
    public static event Action<int> OnFuitCollect;
    public int worth = 5;
    public void Collect()
    {
        OnFuitCollect.Invoke(worth);
        SoundEffectManager.Play("Fruit");
        Destroy(gameObject);
    }

}
