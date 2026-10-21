
using UnityEngine;

public class BlueBone : MonoBehaviour
{
    [Header("Timing")]
    public float appearTime = 0f;
    public float disappearTime = 5f;

    [Header("Bone Movement")]
    public float speed = 3f;

    public enum MoveDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public MoveDirection direction = MoveDirection.Left;

    [Header("Damage")]
    public int damage = 1;
    public float damageCooldownTime = 1f;

    private bool active = false;
    private float sceneStartTime;
    private float damageCooldown = 0f;
    private PlayerHealth playerHealth;

    void Start()
    {
        sceneStartTime = Time.timeSinceLevelLoad;
        SetBoneVisible(false);
    }

    void Update()
    {
        float elapsed = Time.timeSinceLevelLoad - sceneStartTime;

        // Hide the bone when its time is over.
        if (elapsed >= disappearTime)
        {
            active = false;
            SetBoneVisible(false);
            return;
        }

        // Show the bone when its appearance time arrives.
        if (elapsed >= appearTime && !active)
        {
            active = true;
            SetBoneVisible(true);
        }

        if (!active)
            return;

        MoveBone();

        if (damageCooldown > 0f)
        {
            damageCooldown -= Time.deltaTime;
        }
    }

    void MoveBone()
    {
        Vector3 movement = Vector3.zero;

        switch (direction)
        {
            case MoveDirection.Left:
                movement = Vector3.left;
                break;

            case MoveDirection.Right:
                movement = Vector3.right;
                break;

            case MoveDirection.Up:
                movement = Vector3.up;
                break;

            case MoveDirection.Down:
                movement = Vector3.down;
                break;
        }

        transform.position += movement * speed * Time.deltaTime;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!active || damageCooldown > 0f)
            return;

        if (!other.CompareTag("Player"))
            return;

        // The player is considered moving when pressing WASD
        // OR any of the four arrow keys.
        bool pressingMovement =
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.UpArrow) ||
            Input.GetKey(KeyCode.DownArrow);

        // Blue bones only hurt the player if a movement key is held.
        if (!pressingMovement)
            return;

        if (playerHealth == null)
        {
            playerHealth = other.GetComponent<PlayerHealth>();

            // Also check the parent if PlayerHealth is on a parent object.
            if (playerHealth == null)
                playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            damageCooldown = damageCooldownTime;
        }
    }

    void SetBoneVisible(bool visible)
    {
        foreach (SpriteRenderer sr in
                 GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.enabled = visible;
        }

        foreach (Collider2D col in
                 GetComponentsInChildren<Collider2D>(true))
        {
            col.enabled = visible;
        }
    }
}