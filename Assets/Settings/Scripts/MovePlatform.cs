
using UnityEngine;

public class MovingPlatformMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector2 moveDirection = Vector2.right;
    public float moveSpeed = 2f;

    [Header("Optional Movement Limit")]
    public bool useMovementLimit = false;
    public float maxMoveDistance = 5f;

    [Header("Visibility Settings")]
    public SpriteRenderer platformSprite;

    private Vector3 startPosition;
    private Vector3 direction;

    void Start()
    {
        startPosition = transform.position;

        if (moveDirection.sqrMagnitude > 0f)
            direction = ((Vector3)moveDirection).normalized;
        else
            direction = Vector3.right;

        // Automatically find the platform's Sprite Renderer.
        if (platformSprite == null)
            platformSprite = GetComponent<SpriteRenderer>();

        if (platformSprite == null)
            platformSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // Stop if there is no sprite or it isn't visible by any camera.
        if (platformSprite == null || !platformSprite.isVisible)
            return;

        // Stop after reaching the optional movement limit.
        if (useMovementLimit &&
            Vector3.Distance(startPosition, transform.position) >= maxMoveDistance)
        {
            return;
        }

        // Move only while the sprite is visible.
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
