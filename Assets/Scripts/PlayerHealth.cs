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
        currentHealth = maxHealth;
        healthUI.SetMaxHearts(maxHealth);

        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            TakeDamage(enemy.damage);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        animator.Play("Base Layer.Hit", 0, 0f);

        if (currentHealth <= 0)
        {
            //player dead! -- call game over, animation, etc
            OnPlayedDied.Invoke();
        }
    }

}
