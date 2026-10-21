
using UnityEngine;
using System.Collections;

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

    void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    IEnumerator SpawnObjects()
    {
        // Wait until spawning starts
        yield return new WaitForSeconds(startTime);

        // Keep spawning until the stop time
        while (Time.time < stopTime)
        {
            if (objectToSpawn == null || spawnPoint == null)
                yield break;

            GameObject obj = Instantiate(
                objectToSpawn,
                spawnPoint.position,
                Quaternion.identity
            );

            StartCoroutine(MoveObject(obj));

            // Wait before spawning the next object
            yield return new WaitForSeconds(spawnInterval);
        }
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