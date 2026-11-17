
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasterBlasterCircleSpawner : MonoBehaviour
{
    [Header("Prefab and Circle")]
    public GameObject blasterPrefab;
    public Transform orbitCenter;

    [Min(1)]
    public int maxBlasters = 100;

    [Min(0.1f)]
    public float orbitRadius = 8f;

    public bool keepInsideCamera = true;

    [Min(1f)]
    public float spacingMultiplier = 1.8f;

    [Header("Scene Start Timing")]
    [Tooltip("Seconds after the scene loads before the circle starts appearing.")]
    public float appearTime = 15f;

    [Tooltip("Scene time when spawning new blasters stops.")]
    public float stopSpawningTime = 30f;

    [Header("Spawn Settings")]
    [Min(0.01f)]
    public float spawnInterval = 0.5f;

    public float rotationOffset = 0f;

    [Header("Each Blaster's Attack")]
    public float delayAfterSpawn = 1f;

    [Range(1, 20)]
    public int roundsPerBlaster = 4;

    public float timeBetweenRounds = 0.5f;

    [Header("Appearance")]
    public bool forceEnableSprites = true;
    public int spriteSortingOrder = 100;

    private readonly List<GasterBlasterAttackCircle> activeBlasters =
        new List<GasterBlasterAttackCircle>();

    private Coroutine mainRoutine;

    private void Start()
    {
        if (blasterPrefab == null)
        {
            Debug.LogError("Circle Spawner: Assign the Blaster Prefab.");
            return;
        }

        if (orbitCenter == null)
        {
            Debug.LogError("Circle Spawner: Assign the Orbit Center.");
            return;
        }

        if (stopSpawningTime <= appearTime)
        {
            Debug.LogError(
                "Circle Spawner: Stop Spawning Time must be greater than Appear Time."
            );
            return;
        }

        mainRoutine = StartCoroutine(MainRoutine());
    }

    private IEnumerator MainRoutine()
    {
        // Timer starts from when the scene loaded.
        // It does not restart just because this object was enabled.
        while (Time.timeSinceLevelLoad < appearTime)
        {
            yield return null;
        }

        Debug.Log(
            "Gaster Blaster Circle: Appearance time reached at scene time " +
            Time.timeSinceLevelLoad
        );

        float safeRadius = CalculateSafeRadius();
        float angleStep = CalculateAngleStep(safeRadius);

        float currentAngle = rotationOffset;
        float nextSpawnTime = Time.timeSinceLevelLoad;

        int spawnedCount = 0;

        // Keep spawning until the scene reaches Stop Spawning Time.
        while (Time.timeSinceLevelLoad < stopSpawningTime &&
               spawnedCount < maxBlasters)
        {
            if (Time.timeSinceLevelLoad >= nextSpawnTime)
            {
                SpawnOneBlaster(currentAngle, safeRadius);

                currentAngle += angleStep;
                spawnedCount++;

                nextSpawnTime =
                    Time.timeSinceLevelLoad + spawnInterval;
            }

            yield return null;
        }

        Debug.Log(
            "Gaster Blaster Circle: Spawning stopped. " +
            "Waiting for existing attacks to finish."
        );

        // Let every already-spawned blaster finish its attacks.
        while (AnyBlastersStillAttacking())
        {
            yield return null;
        }

        ClearCircle();

        Debug.Log("Gaster Blaster Circle: All attacks finished.");

        mainRoutine = null;
    }

    private void SpawnOneBlaster(float angle, float radius)
    {
        GameObject newBlaster = Instantiate(
            blasterPrefab,
            orbitCenter.position,
            Quaternion.identity
        );

        GasterBlasterAttackCircle attack =
            newBlaster.GetComponent<GasterBlasterAttackCircle>();

        if (attack == null)
        {
            Debug.LogError(
                "Spawned prefab is missing GasterBlasterAttackCircle."
            );

            Destroy(newBlaster);
            return;
        }

        attack.Setup(orbitCenter, angle, radius);

        SpriteRenderer[] renderers =
            newBlaster.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer sr in renderers)
        {
            if (forceEnableSprites)
                sr.enabled = true;

            sr.sortingOrder = spriteSortingOrder;
        }

        activeBlasters.Add(attack);

        // Each blaster gets its own attack timer.
        attack.StartCoroutine(
            attack.RunAttackRounds(
                roundsPerBlaster,
                delayAfterSpawn,
                timeBetweenRounds
            )
        );
    }

    private bool AnyBlastersStillAttacking()
    {
        for (int i = activeBlasters.Count - 1; i >= 0; i--)
        {
            if (activeBlasters[i] == null)
            {
                activeBlasters.RemoveAt(i);
                continue;
            }

            if (!activeBlasters[i].IsFinished())
                return true;
        }

        return false;
    }

    private float CalculateSafeRadius()
    {
        float result = orbitRadius;

        if (!keepInsideCamera)
            return result;

        Camera cam = Camera.main;

        if (cam == null || !cam.orthographic)
            return result;

        Vector3 center = orbitCenter.position;

        float left = cam.ViewportToWorldPoint(
            new Vector3(0f, 0.5f, 0f)
        ).x;

        float right = cam.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, 0f)
        ).x;

        float bottom = cam.ViewportToWorldPoint(
            new Vector3(0.5f, 0f, 0f)
        ).y;

        float top = cam.ViewportToWorldPoint(
            new Vector3(0.5f, 1f, 0f)
        ).y;

        float blasterRadius = GetPrefabHalfDiagonal();

        float availableRadius = Mathf.Min(
            center.x - left,
            right - center.x,
            center.y - bottom,
            top - center.y
        ) - blasterRadius;

        if (availableRadius <= 0f)
        {
            Debug.LogWarning(
                "Circle Spawner: Not enough camera space for the circle."
            );

            return 0.1f;
        }

        result = Mathf.Min(orbitRadius, availableRadius);

        return Mathf.Max(0.1f, result);
    }

    private float GetPrefabHalfDiagonal()
    {
        SpriteRenderer sr =
            blasterPrefab.GetComponentInChildren<SpriteRenderer>();

        if (sr == null || sr.sprite == null)
            return 0.5f;

        Vector3 scale = blasterPrefab.transform.lossyScale;
        Vector2 size = sr.sprite.bounds.size;

        float width = size.x * Mathf.Abs(scale.x);
        float height = size.y * Mathf.Abs(scale.y);

        return Mathf.Sqrt(
            width * width + height * height
        ) * 0.5f;
    }

    private float CalculateAngleStep(float radius)
    {
        float halfDiagonal = GetPrefabHalfDiagonal();

        float minimumSpacing =
            halfDiagonal * 2f * spacingMultiplier;

        float ratio = minimumSpacing / (2f * radius);
        ratio = Mathf.Clamp(ratio, 0f, 1f);

        float angle =
            2f * Mathf.Asin(ratio) * Mathf.Rad2Deg;

        return Mathf.Max(1f, angle);
    }

    private void ClearCircle()
    {
        foreach (GasterBlasterAttackCircle blaster in activeBlasters)
        {
            if (blaster != null)
                Destroy(blaster.gameObject);
        }

        activeBlasters.Clear();
    }

    private void OnDisable()
    {
        if (mainRoutine != null)
        {
            StopCoroutine(mainRoutine);
            mainRoutine = null;
        }

        ClearCircle();
    }
}
