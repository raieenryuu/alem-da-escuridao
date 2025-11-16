using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    public System.Action onHealthChanged;
    public System.Action onPlayerDied;

    [Header("Damage Cooldown")]
    public float invulnerabilityTime = 0.5f;
    private float lastDamageTime = -999f;

    void Start()
    {
        currentHealth = maxHealth;
        onHealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (Time.time - lastDamageTime < invulnerabilityTime)
            return;

        lastDamageTime = Time.time;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        onHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager.Instance.GameOver("You ran out of health!");
        gameObject.SetActive(false);
    }
}
