
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class GasterBlasterAttackCircle : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Sound")]
    public AudioClip appearSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("Warning Laser")]
    public float warningDuration = 1f;
    public float warningWidth = 0.15f;
    public Color warningColor = new Color(1f, 0f, 0f, 0.35f);

    [Header("Attack Beam")]
    public float beamDuration = 0.7f;
    public float beamWidth = 0.8f;
    public Color beamColor = Color.cyan;

    [Header("Damage")]
    public int damage = 1;
    public float damageInterval = 0.5f;

    [Header("Blaster Facing")]
    public float rotationOffset = 0f;

    private Vector3 fixedCenterPosition;
    private float orbitRadius;
    private float orbitAngle;
    private float orbitSpeed;
    private bool clockwise;
    private bool setupComplete;

    private Quaternion fixedRotation;

    private SpriteRenderer spriteRenderer;
    private LineRenderer warningLine;
    private LineRenderer beamLine;

    private bool hasFired;
    private bool hidden;
    private float nextDamageTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        warningLine = CreateLine(
            "Warning Laser", warningColor, warningWidth
        );

        beamLine = CreateLine(
            "Attack Beam", beamColor, beamWidth
        );

        warningLine.enabled = false;
        beamLine.enabled = false;
    }

    public void Setup(
        Transform center,
        float radius,
        float startingAngle,
        float speed,
        bool rotateClockwise)
    {
        if (center == null)
        {
            Debug.LogError("Gaster Blaster: Orbit Center is missing!");
            return;
        }

        fixedCenterPosition = center.position;

        orbitRadius = radius;
        orbitAngle = startingAngle;
        orbitSpeed = speed;
        clockwise = rotateClockwise;

        hasFired = false;
        hidden = false;
        setupComplete = true;

        spriteRenderer.enabled = true;

        if (closedSprite != null)
            spriteRenderer.sprite = closedSprite;

        warningLine.enabled = false;
        beamLine.enabled = false;

        // Calculate the inward-facing rotation once.
        Vector2 inward = (
            (Vector2)fixedCenterPosition -
            (Vector2)transform.position
        ).normalized;

        float angle = Mathf.Atan2(inward.y, inward.x) * Mathf.Rad2Deg;

        fixedRotation = Quaternion.Euler(
            0f, 0f, angle + rotationOffset
        );

        transform.rotation = fixedRotation;

        UpdateOrbitPosition();
        UpdateLaserPositions();
    }

    public void SetRotationOffset(float offset)
    {
        rotationOffset = offset;

        if (!setupComplete)
            return;

        Vector2 inward = (
            (Vector2)fixedCenterPosition -
            (Vector2)transform.position
        ).normalized;

        float angle = Mathf.Atan2(inward.y, inward.x) * Mathf.Rad2Deg;

        fixedRotation = Quaternion.Euler(
            0f, 0f, angle + rotationOffset
        );

        transform.rotation = fixedRotation;
    }

    private void Update()
    {
        if (!setupComplete || hidden)
            return;

        float direction = clockwise ? -1f : 1f;

        // Move around the circle without changing rotation.
        orbitAngle += direction * orbitSpeed * Time.deltaTime;

        UpdateOrbitPosition();
        UpdateLaserPositions();
    }

    private void UpdateOrbitPosition()
    {
        Vector2 offset = new Vector2(
            Mathf.Cos(orbitAngle * Mathf.Deg2Rad),
            Mathf.Sin(orbitAngle * Mathf.Deg2Rad)
        ) * orbitRadius;

        transform.position = fixedCenterPosition + (Vector3)offset;

        // Keep the rotation fixed.
        transform.rotation = fixedRotation;
    }

    public void Fire()
    {
        if (!setupComplete || hasFired || hidden ||
            !gameObject.activeInHierarchy)
            return;

        hasFired = true;
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        if (appearSound != null)
        {
            AudioSource.PlayClipAtPoint(
                appearSound,
                transform.position,
                soundVolume
            );
        }

        if (openSprite != null)
            spriteRenderer.sprite = openSprite;

        warningLine.enabled = true;

        float timer = 0f;

        while (timer < warningDuration)
        {
            UpdateLaserPositions();
            timer += Time.deltaTime;
            yield return null;
        }

        warningLine.enabled = false;
        beamLine.enabled = true;

        timer = 0f;
        nextDamageTime = 0f;

        while (timer < beamDuration)
        {
            UpdateLaserPositions();

            if (Time.time >= nextDamageTime)
            {
                DamagePlayerOnBeam();

                nextDamageTime =
                    Time.time + Mathf.Max(0.05f, damageInterval);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        beamLine.enabled = false;
        HideBlaster();
    }

    private Vector3 GetInwardDirection()
    {
        return (
            fixedCenterPosition - transform.position
        ).normalized;
    }

    private void UpdateLaserPositions()
    {
        if (!setupComplete)
            return;

        Vector3 start = transform.position;

        // Always aim at the fixed center, never at the player.
        Vector3 direction = GetInwardDirection();

        Vector3 end = fixedCenterPosition;

        warningLine.SetPosition(0, start);
        warningLine.SetPosition(1, end);

        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, end);
    }

    private void DamagePlayerOnBeam()
    {
        if (!setupComplete)
            return;

        Vector3 start = transform.position;
        Vector3 direction = GetInwardDirection();

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            start,
            direction,
            orbitRadius
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            PlayerHealth health =
                hit.collider.GetComponent<PlayerHealth>();

            if (health == null)
                health = hit.collider.GetComponentInParent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
                return;
            }
        }
    }

    public void HideBlaster()
    {
        hidden = true;

        spriteRenderer.enabled = false;
        warningLine.enabled = false;
        beamLine.enabled = false;
    }

    private LineRenderer CreateLine(
        string objectName,
        Color color,
        float width)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 101;

        return line;
    }
}
