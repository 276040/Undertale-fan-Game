
using UnityEngine;
using System.Collections;

public class EnemyMove : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float disappearAfter = 5f;
    public bool moveRight = true;

    private Rigidbody2D rb;
    private float timer;
    private bool isMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        timer = 0f;
        isMoving = true;
    }

    void FixedUpdate()
    {
        if (!isMoving || rb == null)
            return;

        float direction = moveRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );

        timer += Time.fixedDeltaTime;

        if (timer >= disappearAfter)
        {
            isMoving = false;
            rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }
}