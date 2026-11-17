
using UnityEngine;
using System.Collections;

public class GasterBlasterLaserController : MonoBehaviour
{
    [Header("Warning Laser")]
    public float warningDistance = 10f;
    public float warningDuration = 1f;
    public float warningWidth = 0.15f;
    public Color warningColor = new Color(1f, 0f, 0f, 0.3f);

    [Header("Attack Beam")]
    public float beamLength = 15f;
    public float beamWidth = 0.8f;
    public float beamDuration = 0.7f;
    public Color beamColor = Color.cyan;

    [Header("Damage")]
    public int damage = 1;
    public float damageInterval = 0.5f;

    private LineRenderer warningLine;
    private LineRenderer beamLine;
    private Vector3 fixedCenter;
    private bool initialized;

    private void Awake()
    {
        warningLine = CreateLine(
            "Warning Laser", warningColor, warningWidth
        );

        beamLine = CreateLine(
            "Attack Beam", beamColor, beamWidth
        );

        warningLine.enabled = false;
        beamLine.enabled = false;
    }

    public void Setup(Vector3 center)
    {
        fixedCenter = center;
        initialized = true;
        HideLasers();
    }

    public IEnumerator Fire()
    {
        if (!initialized)
            yield break;

        UpdateLines();

        warningLine.enabled = true;
        yield return new WaitForSeconds(warningDuration);

        warningLine.enabled = false;
        beamLine.enabled = true;

        float elapsed = 0f;
        float nextDamage = 0f;

        while (elapsed < beamDuration)
        {
            UpdateLines();

            if (Time.time >= nextDamage)
            {
                DamagePlayer();
                nextDamage = Time.time +
                    Mathf.Max(0.05f, damageInterval);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        HideLasers();
    }

    private void UpdateLines()
    {
        Vector3 start = transform.position;

        Vector2 direction =
            ((Vector2)fixedCenter - (Vector2)start).normalized;

        float centerDistance = Vector2.Distance(
            start, fixedCenter
        );

        Vector3 warningEnd = start +
            (Vector3)(direction * Mathf.Min(
                warningDistance, centerDistance
            ));

        Vector3 beamEnd = start +
            (Vector3)(direction * Mathf.Min(
                beamLength, centerDistance
            ));

        warningLine.SetPosition(0, start);
        warningLine.SetPosition(1, warningEnd);

        beamLine.SetPosition(0, start);
        beamLine.SetPosition(1, beamEnd);
    }

    private void DamagePlayer()
    {
        Vector2 direction =
            ((Vector2)fixedCenter -
             (Vector2)transform.position).normalized;

        float distance = Vector2.Distance(
            transform.position, fixedCenter
        );

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            transform.position,
            direction,
            Mathf.Min(beamLength, distance)
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null)
                continue;

            PlayerHealth health =
                hit.collider.GetComponent<PlayerHealth>();

            if (health == null)
            {
                health =
                    hit.collider.GetComponentInParent<PlayerHealth>();
            }

            if (health != null)
            {
                health.TakeDamage(damage);
                return;
            }
        }
    }

    public void HideLasers()
    {
        if (warningLine != null)
            warningLine.enabled = false;

        if (beamLine != null)
            beamLine.enabled = false;
    }

    private LineRenderer CreateLine(
        string objectName,
        Color color,
        float width)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.material = new Material(
            Shader.Find("Sprites/Default")
        );
        line.sortingOrder = 101;

        return line;
    }
}
