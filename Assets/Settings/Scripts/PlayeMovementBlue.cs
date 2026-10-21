
using UnityEngine;

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

    [Header("Yellow Heart Movement")]
    public float yellowMoveSpeed = 5f;

    [Header("Screen Flip")]
    public bool useScreenFlip = true;
    public float flipStartTime = 5f;
    public float flipEndTime = 10f;
    public bool startFlipped = false;
    public bool reverseControls = true;
    public Camera gameCamera;

    [Header("Ground Check")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public float groundCheckRadius = 0.4f;
    public LayerMask groundLayer;

    [Header("Heart Appearance")]
    public SpriteRenderer soulRenderer;
    public Sprite greenSoulSprite;
    public Sprite yellowHeartSprite;

    private float moveInput;
    private float verticalInput;
    private float fightStartTime;

    private bool isGrounded;
    private bool isHoldingJump;
    private float jumpHoldTimer;
    private bool isFlipped;

    private Quaternion normalCameraRotation;
    private Quaternion flippedCameraRotation;

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

        if (gameCamera != null)
        {
            normalCameraRotation = gameCamera.transform.rotation;
            flippedCameraRotation = normalCameraRotation *
                Quaternion.Euler(0f, 0f, 180f);
        }
        else
        {
            Debug.LogWarning(
                "Screen flip: No camera found. Tag your camera MainCamera " +
                "or assign it in the Inspector.");
        }

        rb.freezeRotation = true;

        // Prevent the player from sticking to walls.
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

        // Start with the green soul.
        SetSoulMode(SoulMode.Green);

        SetScreenFlipped(startFlipped);
    }

    void Update()
    {
        float fightTime = Time.time - fightStartTime;

        // Automatically switch between hearts.
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

        // Automatically flip and unflip the screen.
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
                // If end time is earlier than start time,
                // the flip remains active after the start time.
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

        if (currentSoul == SoulMode.Green)
            GreenSoulMovement();
        else
            YellowHeartMovement();
    }

    // =========================
    // SCREEN FLIP
    // =========================

    void SetScreenFlipped(bool flipped)
    {
        isFlipped = flipped;

        if (gameCamera != null)
        {
            gameCamera.transform.rotation = flipped
                ? flippedCameraRotation
                : normalCameraRotation;
        }
    }

    // Can be called by another script or a UI button.
    public void FlipScreenNow()
    {
        SetScreenFlipped(!isFlipped);
    }

    // =========================
    // CONTROL INPUT
    // =========================

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

    // =========================
    // GREEN SOUL INPUT
    // =========================

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

        // Jump using Space, W, or Up Arrow.
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isGrounded)
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

        // Also stop jump-hold when the reversed up direction
        // is released. Jumping itself still uses the physical
        // Space/W/Up keys.
    }

    void GreenSoulMovement()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        if (isHoldingJump &&
            jumpHoldTimer < maxHoldTime &&
            rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity += Vector2.up *
                jumpHoldForce * Time.fixedDeltaTime;

            jumpHoldTimer += Time.fixedDeltaTime;
        }
    }

    // =========================
    // YELLOW HEART INPUT
    // =========================

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

    // =========================
    // CHANGE SOUL
    // =========================

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
