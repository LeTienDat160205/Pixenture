using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public HealthUI healthUI;

    private Animator animator;

    public static event Action OnPlayedDied;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        ResetHealth();
        GameController.OnReset += ResetHealth;
    }

    private void OnDestroy()
    {
        GameController.OnReset -= ResetHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            TakeDamage(enemy.damage);
            SoundEffectManager.Play("Hit");
        }
    }

    void ResetHealth()
    {
        currentHealth = maxHealth;
        if (healthUI != null) healthUI.SetMaxHearts(maxHealth);
        else Debug.LogError("PlayerHealth chưa được gán HealthUI.", this);
    }

    private void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        if (healthUI != null) healthUI.UpdateHearts(currentHealth);

        if (animator != null) animator.Play("Base Layer.Hit", 0, 0f);

        if (currentHealth <= 0)
        {
            //player dead! -- call game over, animation, etc
            SoundEffectManager.Play("Die");
            OnPlayedDied?.Invoke();
        }
    }

}
