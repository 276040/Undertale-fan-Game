
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(AudioSource))]
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

    [Header("Hurt Sound")]
    public AudioClip hurtSound;
    [Range(0f, 1f)]
    public float hurtSoundVolume = 1f;

    [Header("Healing")]
    public KeyCode healKey = KeyCode.C;
    public int healAmount = 3;
    public int maxHeals = 3;
    public GameObject objectToDestroyWhenOutOfHeals;
    public TMP_Text healsText;

    [Header("Heal Sound")]
    public AudioClip healSound;
    [Range(0f, 1f)]
    public float healSoundVolume = 1f;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public Button retryButton;

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
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(Retry);

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

        if (hurtSound != null)
            audioSource.PlayOneShot(hurtSound, hurtSoundVolume);

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

        if (healSound != null)
            audioSource.PlayOneShot(healSound, healSoundVolume);

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

        int oldHealth = currentHealth;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (currentHealth > oldHealth && healSound != null)
            audioSource.PlayOneShot(healSound, healSoundVolume);

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
            healthText.text = "HP: " + currentHealth + " / " + maxHealth;
    }

    void UpdateHealsUI()
    {
        if (healsText != null)
            healsText.text = "Heals: " + (maxHeals - healsUsed);
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

        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
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

