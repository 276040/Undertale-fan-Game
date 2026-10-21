
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GasterBlasterAttack : MonoBehaviour
{
    [Header("Blaster Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Appear Sound")]
    public AudioClip appearSound;
    [Range(0f, 1f)] public float soundVolume = 1f;
    public float appearDelay = 0f;

    [Header("Warning Laser")]
    public float warningDistance = 10f;
    public float warningDuration = 1f;
    public float warningWidth = 0.15f;
    [Range(0f, 1f)] public float warningAlpha = 0.3f;

    [Header("Attack Beam")]
    public float beamLength = 15f;
    public float beamWidth = 0.8f;
    public float beamDuration = 0.7f;
    public Color beamColor = Color.cyan;

    [Header("Damage")]
    public int damage = 1;
    public float damageInterval = 0.5f;

    [Header("Cleanup")]
    public float destroyAfterAttackDelay = 0f;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private LineRenderer warningLine;
    private LineRenderer beamLine;
    private bool attackStarted;

    public void SetTarget(Transform target)
    {
        player = target;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        warningLine = CreateLine("Warning Laser", warningWidth);
        beamLine = CreateLine("Attack Beam", beamWidth);

        warningLine.enabled = false;
        beamLine.enabled = false;
    }

    LineRenderer CreateLine(string lineName, float width)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
            line.material = new Material(shader);

        line.startColor = beamColor;
        line.endColor = beamColor;
        line.sortingOrder = 100;
        line.numCapVertices = 4;

        return line;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null)
        {
            Debug.LogError(
                "Gaster Blaster: Player not found. Check the Player tag.");

            HideBlaster();
            return;
        }

        // Also works if the spawner already started the attack.
        StartAttack();
    }

    public void StartAttack()
    {
        if (attackStarted || !gameObject.activeInHierarchy)
            return;

        attackStarted = true;
        StartCoroutine(AttackRoutine());
    }

    // Called by the spawner whenever this hidden blaster is reused.
    public void ResetForReuse()
    {
        StopAllCoroutines();

        attackStarted = false;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        spriteRenderer.enabled = true;

        if (warningLine != null)
            warningLine.enabled = false;

        if (beamLine != null)
            beamLine.enabled = false;

        // Find the player again if needed.
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null)
        {
            Debug.LogError(
                "Gaster Blaster: Cannot reuse because Player was not found.");
            HideBlaster();
            return;
        }

        StartAttack();
    }

    IEnumerator AttackRoutine()
    {
        if (appearDelay > 0f)
            yield return new WaitForSeconds(appearDelay);

        if (player == null)
        {
            HideBlaster();
            yield break;
        }

        if (appearSound != null)
        {
            AudioSource.PlayClipAtPoint(
                appearSound, transform.position, soundVolume);
        }

        // Lock the aim direction when the attack starts.
        Vector2 direction =
            ((Vector2)player.position -
             (Vector2)transform.position).normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 beamStart = transform.position;

        // Show the warning laser.
        Vector3 warningEnd =
            beamStart + (Vector3)(direction * warningDistance);

        Color faintColor = new Color(
            beamColor.r,
            beamColor.g,
            beamColor.b,
            warningAlpha);

        warningLine.startColor = faintColor;
        warningLine.endColor = faintColor;
        warningLine.startWidth = warningWidth;
        warningLine.endWidth = warningWidth;
        warningLine.SetPosition(0, beamStart);
        warningLine.SetPosition(1, warningEnd);
        warningLine.enabled = true;

        yield return new WaitForSeconds(warningDuration);

        warningLine.enabled = false;

        if (openSprite != null)
            spriteRenderer.sprite = openSprite;

        // Fire the damaging beam.
        beamLine.startColor = beamColor;
        beamLine.endColor = beamColor;
        beamLine.startWidth = beamWidth;
        beamLine.endWidth = beamWidth;
        beamLine.SetPosition(0, beamStart);
        beamLine.SetPosition(
            1, beamStart + (Vector3)(direction * beamLength));

        beamLine.enabled = true;

        Debug.Log("Gaster Blaster fired its damaging beam.");

        float elapsed = 0f;
        float nextDamageTime = 0f;

        while (elapsed < beamDuration)
        {
            if (player != null && elapsed >= nextDamageTime)
            {
                CheckPlayerHit(beamStart, direction);

                nextDamageTime =
                    elapsed + Mathf.Max(0.1f, damageInterval);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        beamLine.enabled = false;

        if (destroyAfterAttackDelay > 0f)
            yield return new WaitForSeconds(destroyAfterAttackDelay);

        // Hide instead of destroying the object.
        HideBlaster();
    }

    void HideBlaster()
    {
        if (warningLine != null)
            warningLine.enabled = false;

        if (beamLine != null)
            beamLine.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        // The object still exists and can be reused by the spawner.
        gameObject.SetActive(false);
    }

    void CheckPlayerHit(Vector3 beamStart, Vector2 direction)
    {
        if (player == null)
            return;

        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        if (playerCollider == null)
            playerCollider =
                player.GetComponentInChildren<Collider2D>();

        Vector2 playerPosition = player.position;

        Vector2 checkPosition = playerCollider != null
            ? playerCollider.ClosestPoint(beamStart)
            : playerPosition;

        Vector2 offset = checkPosition - (Vector2)beamStart;

        float alongBeam = Vector2.Dot(offset, direction);

        float distanceFromBeam = Mathf.Abs(
            direction.x * offset.y -
            direction.y * offset.x);

        float extraReach = 0f;

        if (playerCollider != null)
        {
            Bounds bounds = playerCollider.bounds;
            extraReach = Mathf.Max(
                bounds.extents.x, bounds.extents.y);
        }

        bool hit =
            alongBeam >= -extraReach &&
            alongBeam <= beamLength + extraReach &&
            distanceFromBeam <= beamWidth * 0.5f + extraReach;

        if (!hit)
            return;

        PlayerHealth health =
            player.GetComponent<PlayerHealth>();

        if (health == null)
            health = player.GetComponentInParent<PlayerHealth>();

        if (health == null)
            health = player.GetComponentInChildren<PlayerHealth>();

        if (health != null)
        {
            Debug.Log(
                "Gaster Blaster hit the player! Damage: " + damage);

            health.TakeDamage(damage);
        }
        else
        {
            Debug.LogError(
                "Gaster Blaster detected a hit, but couldn't find " +
                "PlayerHealth on the player or its parents/children.");
        }
    }
}
