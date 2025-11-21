using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] public AudioClip deathSound;
    [SerializeField] public AudioClip[] hurtSounds;
    [SerializeField] public AudioClip hitSound;

    
    

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

        
        SoundFXManager.instance.PlaySoundEffectClip(hitSound, transform, 1f);
        lastDamageTime = Time.time;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        onHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        
        SoundFXManager.instance.PlayRandomSoundEffectClip(hurtSounds, transform, 1f);
        
    }

    void Die()
    {
        SoundFXManager.instance.PlaySoundEffectClip(deathSound, transform, 1f);
        GameManager.Instance.GameOver("You ran out of health!");
        gameObject.SetActive(false);
    }
}
