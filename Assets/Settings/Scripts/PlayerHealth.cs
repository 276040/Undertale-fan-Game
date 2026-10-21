
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth = 10;

    [Header("Health UI")]
    public Slider healthBar;
    public TMP_Text healthText;

    [Header("Healing")]
    public int healAmount = 3;
    public int maxHeals = 3;
    public TMP_Text healsText;
    public GameObject objectToDestroy;

    private int healsUsed = 0;

    [Header("Damage")]
    public float invincibilityTime = 1f;
    private bool isInvincible = false;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateHealthUI();
        UpdateHealsUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Heal();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
            return;
        }

        StartCoroutine(InvincibilityFrames());
    }

    public void Heal()
    {
        if (healsUsed >= maxHeals)
            return;

        if (currentHealth >= maxHealth)
            return;

        currentHealth = Mathf.Min(
            currentHealth + healAmount,
            maxHealth
        );

        healsUsed++;

        UpdateHealthUI();
        UpdateHealsUI();

        if (healsUsed >= maxHeals)
        {
            if (objectToDestroy != null)
            {
                Destroy(objectToDestroy);
            }
        }
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
            healthText.text = "HP: " +
                currentHealth + " / " + maxHealth;
        }
    }

    void UpdateHealsUI()
    {
        if (healsText != null)
        {
            healsText.text = "Heals: " +
                (maxHeals - healsUsed);
        }
    }

    IEnumerator InvincibilityFrames()
    {
        isInvincible = true;

        float timer = 0f;

        while (timer < invincibilityTime)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled =
                    !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }
}