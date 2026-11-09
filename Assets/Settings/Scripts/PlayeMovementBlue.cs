
using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public enum SoulMode
    {
        Green,
        Yellow
    }

    [Header("Current Soul")]
    public SoulMode currentSoul = SoulMode.Green;

    [Header("Automatic Soul Switching")]
    public bool useAutomaticSwitching = true;
    public float switchToYellowTime = 10f;
    public float switchToGreenTime = 20f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Green Soul Jump")]
    public float jumpForce = 10f;
    public float jumpHoldForce = 20f;
    public float maxHoldTime = 0.3f;
    public float greenGravity = 3f;

    [Header("Slam Jump")]
    [Tooltip("How long normal movement is paused after a slam launch.")]
    public float slamJumpControlTime = 0.5f;

    private float slamMovementOverrideUntil = 0f;

    [Header("Yellow Heart Movement")]
    public float yellowMoveSpeed = 5f;

    [Header("Screen Flip")]
    public bool useScreenFlip = true;
    public float flipStartTime = 5f;
    public float flipEndTime = 10f;
    public bool startFlipped = false;
    public bool reverseControls = true;
    public Camera gameCamera;

    [Header("Smooth Flip Animation")]
    public float flipDuration = 0.7f;
    public bool spinClockwise = true;
    public AnimationCurve flipCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Flip Sound")]
    public AudioClip flipSound;
    [Range(0f, 1f)]
    public float flipSoundVolume = 1f;

    private AudioSource audioSource;

    [Header("Ground Check")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public float groundCheckRadius = 0.4f;
    public LayerMask groundLayer;

    [Header("Heart Appearance")]
    public SpriteRenderer soulRenderer;
    public Sprite greenSoulSprite;
    public Sprite yellowHeartSprite;

    [Header("Jump Lock")]
    public bool isJumpLocked = false;
    public float remainingJumpLockTime = 0f;

    private float moveInput;
    private float verticalInput;
    private float fightStartTime;

    private bool isGrounded;
    private bool isHoldingJump;
    private float jumpHoldTimer;
    private bool isFlipped;

    private float jumpLockedUntil = 0f;

    private Quaternion normalCameraRotation;
    private Quaternion flippedCameraRotation;

    private Coroutine flipCoroutine;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (soulRenderer == null)
            soulRenderer = GetComponent<SpriteRenderer>();

        if (rb == null)
        {
            Debug.LogError("PlayerMovement needs a Rigidbody2D!");
            enabled = false;
            return;
        }

        if (gameCamera == null)
            gameCamera = Camera.main;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (gameCamera != null)
        {
            normalCameraRotation = gameCamera.transform.rotation;

            flippedCameraRotation = normalCameraRotation *
                Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            Debug.LogWarning("No camera found for screen flipping.");
        }

        rb.freezeRotation = true;

        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider != null)
        {
            PhysicsMaterial2D noFriction =
                new PhysicsMaterial2D("No Friction");

            noFriction.friction = 0f;
            noFriction.bounciness = 0f;

            playerCollider.sharedMaterial = noFriction;
        }

        fightStartTime = Time.time;

        SetSoulMode(SoulMode.Green);
        SetScreenFlipped(startFlipped);
    }

    void Update()
    {
        remainingJumpLockTime =
            Mathf.Max(0f, jumpLockedUntil - Time.time);

        isJumpLocked = remainingJumpLockTime > 0f;

        float fightTime = Time.time - fightStartTime;

        if (useAutomaticSwitching)
        {
            if (fightTime >= switchToGreenTime)
            {
                if (currentSoul != SoulMode.Green)
                    ActivateGreenSoul();
            }
            else if (fightTime >= switchToYellowTime)
            {
                if (currentSoul != SoulMode.Yellow)
                    ActivateYellowHeart();
            }
        }

        if (useScreenFlip)
        {
            bool shouldFlip;

            if (flipEndTime >= flipStartTime)
            {
                shouldFlip = fightTime >= flipStartTime &&
                             fightTime < flipEndTime;
            }
            else
            {
                shouldFlip = fightTime >= flipStartTime;
            }

            SetScreenFlipped(shouldFlip);
        }
        else
        {
            SetScreenFlipped(startFlipped);
        }

        if (currentSoul == SoulMode.Green)
            GreenSoulInput();
        else
            YellowHeartInput();
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        // Do not overwrite the launch velocity during a slam jump.
        if (Time.time < slamMovementOverrideUntil)
            return;

        if (currentSoul == SoulMode.Green)
            GreenSoulMovement();
        else
            YellowHeartMovement();
    }

    // Call this from SideSlamController when the player jumps off a wall.
    public void BeginSlamJump(Vector2 launchVelocity)
    {
        if (rb == null)
            return;

        isHoldingJump = false;
        jumpHoldTimer = 0f;

        slamMovementOverrideUntil =
            Time.time + Mathf.Max(0.05f, slamJumpControlTime);

        rb.linearVelocity = launchVelocity;

        Debug.Log("Slam jump launched! Velocity: " + launchVelocity);
    }

    public bool IsSlamJumpActive()
    {
        return Time.time < slamMovementOverrideUntil;
    }

    public void DisableJumpForSeconds(float duration)
    {
        if (duration <= 0f)
            return;

        jumpLockedUntil = Mathf.Max(
            jumpLockedUntil,
            Time.time + duration
        );

        isHoldingJump = false;
        jumpHoldTimer = 0f;

        Debug.Log("Jump disabled for " + duration + " seconds!");
    }

    void SetScreenFlipped(bool flipped)
    {
        if (isFlipped == flipped)
            return;

        isFlipped = flipped;

        if (gameCamera == null)
            return;

        if (flipCoroutine != null)
            StopCoroutine(flipCoroutine);

        if (flipSound != null)
        {
            audioSource.PlayOneShot(
                flipSound,
                flipSoundVolume
            );
        }

        Quaternion targetRotation = flipped
            ? flippedCameraRotation
            : normalCameraRotation;

        flipCoroutine = StartCoroutine(
            SmoothCameraFlip(targetRotation, flipped)
        );
    }

    IEnumerator SmoothCameraFlip(
        Quaternion targetRotation,
        bool flipping
    )
    {
        if (gameCamera == null)
            yield break;

        Quaternion startRotation =
            gameCamera.transform.rotation;

        float direction = spinClockwise ? -1f : 1f;

        if (!flipping)
            direction *= -1f;

        float duration = Mathf.Max(0.01f, flipDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float curveValue = flipCurve.Evaluate(progress);
            float angle = 180f * curveValue * direction;

            gameCamera.transform.rotation =
                startRotation *
                Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        gameCamera.transform.rotation = targetRotation;
        flipCoroutine = null;
    }

    public void FlipScreenNow()
    {
        SetScreenFlipped(!isFlipped);
    }

    float GetHorizontalInput()
    {
        float input = Input.GetAxisRaw("Horizontal");

        if (reverseControls && isFlipped)
            input = -input;

        return input;
    }

    float GetVerticalInput()
    {
        float input = Input.GetAxisRaw("Vertical");

        if (reverseControls && isFlipped)
            input = -input;

        return input;
    }

    void GreenSoulInput()
    {
        moveInput = GetHorizontalInput();

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isGrounded && !isJumpLocked &&
                !IsSlamJumpActive())
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                isHoldingJump = true;
                jumpHoldTimer = 0f;
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) ||
            Input.GetKeyUp(KeyCode.W) ||
            Input.GetKeyUp(KeyCode.UpArrow))
        {
            isHoldingJump = false;
        }
    }

    void GreenSoulMovement()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        if (isHoldingJump &&
            !isJumpLocked &&
            jumpHoldTimer < maxHoldTime &&
            rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity += Vector2.up *
                jumpHoldForce * Time.fixedDeltaTime;

            jumpHoldTimer += Time.fixedDeltaTime;
        }
    }

    void YellowHeartInput()
    {
        moveInput = GetHorizontalInput();
        verticalInput = GetVerticalInput();
    }

    void YellowHeartMovement()
    {
        Vector2 movement = new Vector2(
            moveInput,
            verticalInput
        );

        if (movement.magnitude > 1f)
            movement.Normalize();

        rb.linearVelocity = movement * yellowMoveSpeed;
    }

    public void SetSoulMode(SoulMode newMode)
    {
        currentSoul = newMode;

        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        isHoldingJump = false;
        jumpHoldTimer = 0f;

        if (newMode == SoulMode.Yellow)
        {
            rb.gravityScale = 0f;

            if (soulRenderer != null &&
                yellowHeartSprite != null)
            {
                soulRenderer.sprite = yellowHeartSprite;
            }
        }
        else
        {
            rb.gravityScale = greenGravity;

            if (soulRenderer != null &&
                greenSoulSprite != null)
            {
                soulRenderer.sprite = greenSoulSprite;
            }
        }
    }

    public void ActivateYellowHeart()
    {
        SetSoulMode(SoulMode.Yellow);
    }

    public void ActivateGreenSoul()
    {
        SetSoulMode(SoulMode.Green);
    }

    public void ResetSoulTimer()
    {
        fightStartTime = Time.time;

        SetSoulMode(SoulMode.Green);
        SetScreenFlipped(startFlipped);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}
