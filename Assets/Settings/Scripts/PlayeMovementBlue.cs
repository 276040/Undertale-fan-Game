
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

        // Reset the timer whenever this scene starts.
        fightStartTime = Time.time;

        // Always start the fight with the green soul.
        SetSoulMode(SoulMode.Green);
    }

    void Update()
    {
        if (useAutomaticSwitching)
        {
            float fightTime = Time.time - fightStartTime;

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

        if (currentSoul == SoulMode.Green)
        {
            GreenSoulInput();
        }
        else
        {
            YellowHeartInput();
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (currentSoul == SoulMode.Green)
        {
            GreenSoulMovement();
        }
        else
        {
            YellowHeartMovement();
        }
    }

    // =========================
    // GREEN SOUL INPUT
    // =========================

    void GreenSoulInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

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
        moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void YellowHeartMovement()
    {
        Vector2 movement = new Vector2(
            moveInput,
            verticalInput
        );

        // Keep diagonal movement at the same speed.
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
            // Yellow heart: move in all directions.
            rb.gravityScale = 0f;

            if (soulRenderer != null &&
                yellowHeartSprite != null)
            {
                soulRenderer.sprite = yellowHeartSprite;
            }
        }
        else
        {
            // Green soul: gravity and jumping.
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

    // Restart the switching timer if another script needs it.
    public void ResetSoulTimer()
    {
        fightStartTime = Time.time;
        SetSoulMode(SoulMode.Green);
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