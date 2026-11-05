
using UnityEngine;
using System.Collections;

public class BoneAttack : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Spinning")]
    public float spinSpeed = 500f;

    [Header("Attack Timing")]
    public float waitBeforeAttack = 2f;

    [Header("Attack Movement")]
    public float attackSpeed = 12f;
    public float aimAngleOffset = 0f;

    private Vector2 attackDirection;
    private bool attacking;
    private bool hasStarted;

    private SpriteRenderer[] spriteRenderers;

    void Awake()
    {
        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);
    }

    void Start()
    {
        FindPlayer();
        StartCoroutine(PrepareAttack());
    }

    void Update()
    {
        // Keep spinning ALL the time,
        // including while moving and after hitting the player.
        transform.Rotate(
            0f,
            0f,
            spinSpeed * Time.deltaTime
        );

        // Keep moving forever after launching.
        if (attacking)
        {
            transform.position +=
                (Vector3)(attackDirection * attackSpeed * Time.deltaTime);
        }
    }

    IEnumerator PrepareAttack()
    {
        // Wait until the bone becomes visible.
        while (!IsVisible())
        {
            FindPlayer();
            yield return null;
        }

        // Spin while preparing.
        yield return new WaitForSeconds(waitBeforeAttack);

        FindPlayer();

        if (player == null)
            yield break;

        // Remember the player's position when the attack begins.
        attackDirection =
            ((Vector2)player.position -
             (Vector2)transform.position).normalized;

        if (attackDirection.sqrMagnitude < 0.001f)
            attackDirection = Vector2.right;

        // Adjust for the direction the sprite naturally faces.
        float angle =
            Mathf.Atan2(attackDirection.y, attackDirection.x) *
            Mathf.Rad2Deg + aimAngleOffset;

        // Face the player before launching.
        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        attacking = true;
        hasStarted = true;

        Debug.Log("Bone launched!");
    }

    bool IsVisible()
    {
        if (spriteRenderers == null ||
            spriteRenderers.Length == 0)
            return true;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null && sr.enabled && sr.gameObject.activeInHierarchy)
                return true;
        }

        return false;
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
