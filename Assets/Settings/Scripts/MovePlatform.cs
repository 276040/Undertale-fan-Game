
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
    public TimedVisibility timedVisibility;

    private Vector3 startPosition;
    private Vector3 direction;

    void Start()
    {
        startPosition = transform.position;

        if (moveDirection.sqrMagnitude > 0f)
            direction = ((Vector3)moveDirection).normalized;
        else
            direction = Vector3.right;

        if (timedVisibility == null)
            timedVisibility = GetComponent<TimedVisibility>();

        if (timedVisibility == null)
            timedVisibility = GetComponentInParent<TimedVisibility>();
    }

    void Update()
    {
        // Do not move if the visibility controller is missing.
        if (timedVisibility == null)
            return;

        // Only move while the platform is visible.
        if (!timedVisibility.IsVisible)
            return;

        // Stop after reaching the optional movement limit.
        if (useMovementLimit &&
            Vector3.Distance(startPosition, transform.position) >= maxMoveDistance)
        {
            return;
        }

        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
