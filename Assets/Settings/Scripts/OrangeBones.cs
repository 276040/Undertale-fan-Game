
using UnityEngine;

public class OrangeBone : MonoBehaviour
{
    [Header("Timing")]
    public float appearTime = 0f;
    public float disappearTime = 5f;

    [Header("Movement")]
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

    private bool active = false;
    private PlayerHealth playerHealth;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

        // Hide bone at the beginning
        gameObject.SetActive(false);

        Invoke(nameof(Appear), appearTime);
    }

    void Appear()
    {
        gameObject.SetActive(true);
        active = true;

        Invoke(nameof(Disappear), disappearTime - appearTime);
    }

    void Disappear()
    {
        active = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!active)
            return;

        MoveBone();
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
        if (!active)
            return;

        if (!other.CompareTag("Player"))
            return;

        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

        if (playerRb == null)
            return;

        // ORANGE BONE:
        // Player must MOVE.
        // If the player is standing still, take damage.

        if (Mathf.Abs(playerRb.linearVelocity.x) < 0.05f &&
            Mathf.Abs(playerRb.linearVelocity.y) < 0.05f)
        {
            if (playerHealth == null)
                playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}