
using UnityEngine;
using System.Collections;

public class BoneSpinAttack : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Appear Settings")]
    public float appearTime = 3f;
    public bool startInvisible = true;

    [Header("Spinning")]
    public float spinSpeed = 500f;

    [Header("Attack Timing")]
    public float waitBeforeAttack = 2f;

    [Header("Attack Movement")]
    public float attackSpeed = 12f;
    public float aimAngleOffset = 0f;

    private Vector2 attackDirection;
    private bool attacking;

    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] colliders;

    void Awake()
    {
        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        colliders =
            GetComponentsInChildren<Collider2D>(true);

        if (startInvisible)
            SetVisible(false);
    }

    void Start()
    {
        FindPlayer();
        StartCoroutine(AppearAndAttack());
    }

    void Update()
    {
        // Keep spinning all the time.
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        // Keep moving forever after launching.
        if (attacking)
        {
            transform.position +=
                (Vector3)(attackDirection * attackSpeed * Time.deltaTime);
        }
    }

    IEnumerator AppearAndAttack()
    {
        // Wait before appearing.
        yield return new WaitForSeconds(appearTime);

        // Show the bone and enable its collider.
        SetVisible(true);

        // Wait before launching.
        yield return new WaitForSeconds(waitBeforeAttack);

        FindPlayer();

        if (player == null)
        {
            Debug.LogWarning("BoneSpinAttack: Player not found. Check the Player tag.");
            yield break;
        }

        // Aim at the player's position when the attack begins.
        attackDirection =
            ((Vector2)player.position -
             (Vector2)transform.position).normalized;

        if (attackDirection.sqrMagnitude < 0.001f)
            attackDirection = Vector2.right;

        float angle =
            Mathf.Atan2(attackDirection.y, attackDirection.x) *
            Mathf.Rad2Deg + aimAngleOffset;

        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        attacking = true;

        Debug.Log("Spinning bone launched!");
    }

    void SetVisible(bool visible)
    {
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
                sr.enabled = visible;
        }

        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = visible;
        }
    }

    void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }
}
