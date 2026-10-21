
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

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // Wait until the start time
        yield return new WaitForSeconds(startTime);

        // Spawn until the stop time
        while (Time.time < stopTime)
        {
            int choice = Random.Range(0, 3);

            Transform spawnPoint = location1;

            if (choice == 1)
                spawnPoint = location2;
            else if (choice == 2)
                spawnPoint = location3;

            if (spawnPoint == null ||
                warningObject == null ||
                mainObject == null)
                yield break;

            // Show warning
            GameObject warning = Instantiate(
                warningObject,
                spawnPoint.position,
                Quaternion.identity
            );

            // Wait for warning
            yield return new WaitForSeconds(warningTime);

            if (warning != null)
                Destroy(warning);

            // Only spawn if still before the stop time
            if (Time.time < stopTime)
            {
                GameObject spawnedObject = Instantiate(
                    mainObject,
                    spawnPoint.position,
                    Quaternion.identity
                );

                StartCoroutine(FadeAndDestroy(spawnedObject));
            }

            // Wait before next spawn
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator FadeAndDestroy(GameObject obj)
    {
        yield return new WaitForSeconds(mainObjectLifetime);

        if (obj == null)
            yield break;

        SpriteRenderer[] sprites =
            obj.GetComponentsInChildren<SpriteRenderer>();

        float timer = 0f;

        while (timer < fadeDuration)
        {
            if (obj == null)
                yield break;

            timer += Time.deltaTime;
            float alpha = 1f -
                Mathf.Clamp01(timer / fadeDuration);

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