
using UnityEngine;
using System.Collections;

public class RandomWarningSpawner : MonoBehaviour
{
    [Header("Objects")]
    public GameObject warningObject;
    public GameObject mainObject;

    [Header("Spawn Locations")]
    public Transform location1;
    public Transform location2;
    public Transform location3;

    [Header("Timing")]
    public float startTime = 5f;
    public float stopTime = 30f;
    public float warningTime = 1f;
    public float spawnInterval = 3f;
    public float mainObjectLifetime = 3f;
    public float fadeDuration = 1f;

    private Coroutine spawnRoutine;

    void OnEnable()
    {
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    IEnumerator SpawnRoutine()
    {
        float elapsed = 0f;

        // Wait until the starting time.
        while (elapsed < startTime)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        // Spawn until the stopping time.
        while (elapsed < stopTime)
        {
            if (warningObject == null || mainObject == null)
            {
                Debug.LogError(
                    "RandomWarningSpawner: Warning Object or Main Object is missing!"
                );
                yield break;
            }

            Transform[] locations =
            {
                location1,
                location2,
                location3
            };

            Transform spawnPoint = locations[Random.Range(0, locations.Length)];

            if (spawnPoint == null)
            {
                Debug.LogError(
                    "RandomWarningSpawner: One of the spawn locations is missing!"
                );
                yield break;
            }

            // Create warning.
            GameObject warning = Instantiate(
                warningObject,
                spawnPoint.position,
                Quaternion.identity
            );

            // Wait for warning time while updating elapsed time.
            float warningTimer = 0f;

            while (warningTimer < warningTime)
            {
                yield return null;

                float delta = Time.deltaTime;
                warningTimer += delta;
                elapsed += delta;
            }

            if (warning != null)
                Destroy(warning);

            // Spawn main object only if still within the schedule.
            if (elapsed < stopTime)
            {
                GameObject spawnedObject = Instantiate(
                    mainObject,
                    spawnPoint.position,
                    Quaternion.identity
                );

                StartCoroutine(FadeAndDestroy(spawnedObject));
            }

            // Wait until the next spawn.
            float intervalTimer = 0f;

            while (intervalTimer < spawnInterval && elapsed < stopTime)
            {
                yield return null;

                float delta = Time.deltaTime;
                intervalTimer += delta;
                elapsed += delta;
            }
        }

        Debug.Log("RandomWarningSpawner: Finished spawning.");
    }

    IEnumerator FadeAndDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(mainObjectLifetime);

        if (obj == null)
            yield break;

        SpriteRenderer[] sprites =
            obj.GetComponentsInChildren<SpriteRenderer>(true);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            if (obj == null)
                yield break;

            timer += Time.deltaTime;

            float alpha = 1f -
                Mathf.Clamp01(timer / Mathf.Max(0.001f, fadeDuration));

            foreach (SpriteRenderer sprite in sprites)
            {
                if (sprite != null)
                {
                    Color color = sprite.color;
                    color.a = alpha;
                    sprite.color = color;
                }
            }

            yield return null;
        }

        if (obj != null)
            Destroy(obj);
    }
}
