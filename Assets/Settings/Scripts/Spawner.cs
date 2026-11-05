
using System.Collections;
using UnityEngine;

public class TimedMovingSpawner : MonoBehaviour
{
    [Header("Object")]
    public GameObject objectToSpawn;
    public Transform spawnPoint;

    [Header("Spawn Timing")]
    public float startTime = 5f;
    public float stopTime = 20f;
    public float spawnInterval = 3f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public bool moveRight = true;

    [Header("Lifetime")]
    public float disappearAfter = 5f;

    private Coroutine spawningRoutine;

    void OnEnable()
    {
        spawningRoutine = StartCoroutine(SpawnObjects());
    }

    void OnDisable()
    {
        if (spawningRoutine != null)
        {
            StopCoroutine(spawningRoutine);
            spawningRoutine = null;
        }
    }

    IEnumerator SpawnObjects()
    {
        float elapsed = 0f;

        // Wait until the starting time.
        while (elapsed < startTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Spawn until the stopping time.
        while (elapsed < stopTime)
        {
            if (objectToSpawn == null || spawnPoint == null)
            {
                Debug.LogError(
                    "TimedMovingSpawner: Assign Object To Spawn and Spawn Point!"
                );
                yield break;
            }

            GameObject obj = Instantiate(
                objectToSpawn,
                spawnPoint.position,
                Quaternion.identity
            );

            StartCoroutine(MoveObject(obj));

            // Wait for the next spawn while tracking elapsed battle time.
            float waitTimer = 0f;

            while (waitTimer < spawnInterval && elapsed < stopTime)
            {
                yield return null;

                float delta = Time.deltaTime;
                waitTimer += delta;
                elapsed += delta;
            }
        }

        Debug.Log("TimedMovingSpawner: Finished spawning.");
    }

    IEnumerator MoveObject(GameObject obj)
    {
        float direction = moveRight ? 1f : -1f;
        float timer = 0f;

        while (timer < disappearAfter)
        {
            if (obj == null)
                yield break;

            obj.transform.position += Vector3.right *
                direction * moveSpeed * Time.deltaTime;

            timer += Time.deltaTime;
            yield return null;
        }

        if (obj != null)
            Destroy(obj);
    }
}
