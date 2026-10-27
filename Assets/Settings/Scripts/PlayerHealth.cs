
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth = 10;
    public int contactDamage = 1;

    [Header("Health UI")]
    public Slider healthBar;
    public TMP_Text healthText;

    [Header("Damage Settings")]
    public float invincibilityDuration = 1f;
    public float flashInterval = 0.1f;
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitSoundVolume = 1f;

    [Header("Healing")]
    public KeyCode healKey = KeyCode.C;
    public int healAmount = 3;
    public int maxHeals = 3;
    public GameObject objectToDestroyWhenOutOfHeals;

    [Header("Healing UI")]
    public TMP_Text healsText;

    private int healsUsed = 0;
    private bool isInvincible = false;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        audioSource = GetComponent<AudioSource>();

        UpdateHealthUI();
        UpdateHealsUI();
    }

    void Update()
    {
        if (isDead)
            return;

        if (Input.GetKeyDown(healKey))
            UseHeal();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (hitSound != null)
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.PlayOneShot(hitSound, hitSoundVolume);
        }

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(Invincibility());
    }

    void UseHeal()
    {
        if (isDead || healsUsed >= maxHeals)
            return;

        if (currentHealth >= maxHealth)
            return;

        healsUsed++;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + healAmount
        );

        UpdateHealthUI();
        UpdateHealsUI();

        if (healsUsed >= maxHeals &&
            objectToDestroyWhenOutOfHeals != null)
        {
            Destroy(objectToDestroyWhenOutOfHeals);
        }
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text =
                "HP: " + currentHealth + " / " + maxHealth;
        }
    }

    void UpdateHealsUI()
    {
        if (healsText != null)
        {
            int remaining = Mathf.Max(0, maxHeals - healsUsed);
            healsText.text = "Heals: " + remaining;
        }
    }

    IEnumerator Invincibility()
    {
        isInvincible = true;

        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = color.a > 0.5f ? 0.25f : 1f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        isInvincible = false;
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
        else
        {
            Debug.LogWarning(
                "GameOverManager was not found. Reloading the scene."
            );

            Time.timeScale = 1f;
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            TakeDamage(contactDamage);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            TakeDamage(contactDamage);
    }
}
