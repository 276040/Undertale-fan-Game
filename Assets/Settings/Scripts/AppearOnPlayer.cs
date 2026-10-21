
using UnityEngine;

public class PlayerLaserAttack : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Scene Timing")]
    public float startTime = 5f;
    public float stopTime = 15f;

    [Header("Attack Timing")]
    public float repeatEvery = 2f;
    public float warningDuration = 1f;
    public float attackDuration = 0.5f;

    [Header("Damage")]
    public int damage = 1;

    [Header("Laser Appearance")]
    [Range(0f, 1f)]
    public float warningAlpha = 0.25f;

    [Range(0f, 1f)]
    public float attackAlpha = 1f;

    [Header("Laser Position")]
    public Vector3 positionOffset = Vector3.zero;

    [Header("Hit Range")]
    public float hitDistance = 0.5f;

    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;

    private bool warningActive = false;
    private bool attacking = false;
    private bool started = false;

    private float nextWarningTime;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerLaserAttack needs a SpriteRenderer!");
            return;
        }

        // Hide ONLY the sprite.
        // Do NOT disable the GameObject.
        spriteRenderer.enabled = false;
    }

    void Update()
    {
        float sceneTime = Time.timeSinceLevelLoad;

        // Before start time
        if (sceneTime < startTime)
            return;

        // Stop all attacks after stop time
        if (sceneTime >= stopTime)
        {
            warningActive = false;
            attacking = false;

            spriteRenderer.enabled = false;

            return;
        }

        // Start the attack system
        if (!started)
        {
            started = true;
            nextWarningTime = startTime;
        }

        // Start another warning
        if (sceneTime >= nextWarningTime &&
            !warningActive &&
            !attacking)
        {
            StartWarning();

            nextWarningTime += repeatEvery;
        }

        // While warning, follow the player
        if (warningActive && player != null)
        {
            transform.position = player.position + positionOffset;
        }
    }

    void StartWarning()
    {
        if (player == null)
        {
            Debug.LogError("PlayerLaserAttack: Player is not assigned!");
            return;
        }

        warningActive = true;
        attacking = false;

        // Put warning on player
        transform.position = player.position + positionOffset;

        // Show laser
        spriteRenderer.enabled = true;

        // Make it faint
        SetAlpha(warningAlpha);

        CancelInvoke();

        Invoke(nameof(StartAttack), warningDuration);
    }

    void StartAttack()
    {
        if (player == null)
            return;

        // LOCK the position where the player was
        transform.position = player.position + positionOffset;

        // Stop following the player
        warningActive = false;
        attacking = true;

        // Make laser fully visible
        SetAlpha(attackAlpha);

        // Get PlayerHealth
        if (playerHealth == null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Check distance between player and laser
        float distance = Vector2.Distance(
            player.position,
            transform.position
        );

        // Damage if player didn't escape
        if (distance <= hitDistance)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        CancelInvoke(nameof(EndAttack));

        Invoke(nameof(EndAttack), attackDuration);
    }

    void EndAttack()
    {
        attacking = false;

        // Hide only the sprite
        spriteRenderer.enabled = false;
    }

    void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}