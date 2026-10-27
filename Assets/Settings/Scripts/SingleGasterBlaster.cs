
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GasterBlasterSingle : MonoBehaviour
{
    [Header("Appear Timing")]
    public float appearTime = 5f;
    public float disappearTime = 30f;

    [Header("Blaster Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Firing Direction")]
    [Tooltip("Uses the GameObject's rotation. 0 = right, 90 = up, 180 = left, 270 = down.")]
    public float directionOffset = 0f;

    [Header("Appear Sound")]
    public AudioClip appearSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

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

    private SpriteRenderer spriteRenderer;
    private LineRenderer warningLine;
    private LineRenderer beamLine;

    private float sceneStartTime;
    private bool hasStarted;
    private bool hasFinished;
    private Coroutine attackCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        warningLine = CreateLine("Warning Laser", warningWidth);
        beamLine = CreateLine("Attack Beam", beamWidth);

        warningLine.enabled = false;
        beamLine.enabled = false;
        spriteRenderer.enabled = false;
    }

    void Start()
    {
        sceneStartTime = Time.timeSinceLevelLoad;
    }

    void Update()
    {
        if (hasFinished)
            return;

        float elapsed = Time.timeSinceLevelLoad - sceneStartTime;

        if (!hasStarted && elapsed >= appearTime)
        {
            hasStarted = true;
            spriteRenderer.enabled = true;

            if (closedSprite != null)
                spriteRenderer.sprite = closedSprite;

            attackCoroutine = StartCoroutine(AttackRoutine());
        }

        if (hasStarted && elapsed >= disappearTime)
        {
            hasFinished = true;

            if (attackCoroutine != null)
                StopCoroutine(attackCoroutine);

            HideBlaster();
        }
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
        line.numCapVertices = 4;
        line.sortingOrder = 100;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
            line.material = new Material(shader);

        line.startColor = beamColor;
        line.endColor = beamColor;

        return line;
    }

    IEnumerator AttackRoutine()
    {
       if (appearSound != null)
{
    Debug.Log("Gaster Blaster: Playing appear sound.");

    GameObject soundObject = new GameObject("Gaster Blaster Appear Sound");
    AudioSource audioSource = soundObject.AddComponent<AudioSource>();

    audioSource.clip = appearSound;
    audioSource.volume = soundVolume;
    audioSource.spatialBlend = 0f; // 2D sound, audible anywhere
    audioSource.playOnAwake = false;

    audioSource.Play();
    Destroy(soundObject, appearSound.length + 0.2f);
}
else
{
    Debug.LogWarning("Gaster Blaster: No appear sound assigned!");
}
        // Capture the direction ONCE using this object's rotation.
        // The player's position is never used for aiming.
        float angle = transform.eulerAngles.z + directionOffset;
        float radians = angle * Mathf.Deg2Rad;

        Vector2 direction = new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        ).normalized;

        // Lock the starting point and direction for the whole attack.
        Vector3 beamStart = transform.position;
        Vector3 warningEnd =
            beamStart + (Vector3)(direction * warningDistance);

        Color faintColor = new Color(
            beamColor.r, beamColor.g, beamColor.b, warningAlpha);

        warningLine.startColor = faintColor;
        warningLine.endColor = faintColor;
        warningLine.startWidth = warningWidth;
        warningLine.endWidth = warningWidth;
        warningLine.SetPosition(0, beamStart);
        warningLine.SetPosition(1, warningEnd);
        warningLine.enabled = true;

        yield return new WaitForSeconds(warningDuration);

        if (hasFinished)
            yield break;

        // Switch to the open sprite when firing.
        if (openSprite != null)
            spriteRenderer.sprite = openSprite;

        beamLine.startColor = beamColor;
        beamLine.endColor = beamColor;
        beamLine.startWidth = beamWidth;
        beamLine.endWidth = beamWidth;
        beamLine.SetPosition(0, beamStart);
        beamLine.SetPosition(
            1, beamStart + (Vector3)(direction * beamLength));
        beamLine.enabled = true;

        float elapsed = 0f;
        float nextDamageTime = 0f;

        while (elapsed < beamDuration)
        {
            if (hasFinished)
                yield break;

            if (elapsed >= nextDamageTime)
            {
                CheckPlayerHit(beamStart, direction);

                nextDamageTime =
                    elapsed + Mathf.Max(0.1f, damageInterval);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hide the blaster and both lasers after firing.
        hasFinished = true;
        HideBlaster();
        attackCoroutine = null;
    }

    void CheckPlayerHit(Vector3 beamStart, Vector2 direction)
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
            return;

        Collider2D playerCollider =
            playerObject.GetComponent<Collider2D>();

        if (playerCollider == null)
            playerCollider =
                playerObject.GetComponentInChildren<Collider2D>();

        Vector2 checkPosition = playerCollider != null
            ? playerCollider.ClosestPoint(beamStart)
            : (Vector2)playerObject.transform.position;

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
            playerObject.GetComponent<PlayerHealth>();

        if (health == null)
            health = playerObject.GetComponentInParent<PlayerHealth>();

        if (health == null)
            health = playerObject.GetComponentInChildren<PlayerHealth>();

        if (health != null)
            health.TakeDamage(damage);
    }

    void HideBlaster()
    {
        if (warningLine != null)
            warningLine.enabled = false;

        if (beamLine != null)
            beamLine.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }
}
