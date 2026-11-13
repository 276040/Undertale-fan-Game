
using UnityEngine;

public class MovingPlatformCarryPlayer : MonoBehaviour
{
    private Rigidbody2D playerRigidbody;
    private Rigidbody2D platformRigidbody;

    private Vector2 previousPlatformPosition;
    private bool playerOnPlatform;

    void Awake()
    {
        platformRigidbody = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        playerRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();

        if (playerRigidbody == null)
            return;

        playerOnPlatform = true;

        if (platformRigidbody != null)
            previousPlatformPosition = platformRigidbody.position;
        else
            previousPlatformPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!playerOnPlatform || playerRigidbody == null)
            return;

        Vector2 currentPlatformPosition =
            platformRigidbody != null
                ? platformRigidbody.position
                : (Vector2)transform.position;

        Vector2 platformMovement =
            currentPlatformPosition - previousPlatformPosition;

        playerRigidbody.position += platformMovement;

        previousPlatformPosition = currentPlatformPosition;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;
            playerRigidbody = null;
        }
    }

    void OnDisable()
    {
        playerOnPlatform = false;
        playerRigidbody = null;
    }
}
