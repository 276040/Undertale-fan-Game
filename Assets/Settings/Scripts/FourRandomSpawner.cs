
using UnityEngine;
using System.Collections;

public class FourDirectionSpawner : MonoBehaviour
{
    public enum SpawnDirection
    {
        Up,
        Down,
        Left,
        Right,
        Random
    }

    [Header("Prefab")]
    public GameObject prefabToSpawn;

    [Header("Player")]
    public Transform player;

    [Header("Spawn Settings")]
    public SpawnDirection spawnDirection = SpawnDirection.Random;
    public float spawnDistance = 10f;
    public float spawnInterval = 2f;
    public float startSpawningTime = 0f;
    public float stopSpawningTime = 30f;

    [Header("Options")]
    public bool repeatSpawning = true;
    public bool spawnAtPlayerHeight = true;
    public bool useRandomDirection = false;

    [Header("Spawn Rotation")]
    public bool rotateTowardPlayer = true;
    public float rotationOffset = 0f;

    private float gameTimer;
    private bool spawningStarted;
    private bool spawningFinished;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (prefabToSpawn == null)
        {
            Debug.LogError("FourDirectionSpawner: No prefab assigned!");
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError("FourDirectionSpawner: Player not found!");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(startSpawningTime);

        spawningStarted = true;

        while (!spawningFinished)
        {
            if (gameTimer >= stopSpawningTime)
                break;

            SpawnObject();

            if (!repeatSpawning)
                break;

            yield return new WaitForSeconds(spawnInterval);
        }

        spawningFinished = true;
    }

    void Update()
    {
        gameTimer += Time.deltaTime;
    }

    void SpawnObject()
    {
        Vector2 direction = GetSpawnDirection();

        Vector2 spawnPosition =
            (Vector2)player.position + direction * spawnDistance;

        if (spawnAtPlayerHeight)
            spawnPosition.y = player.position.y;

        float rotation = rotationOffset;

        if (rotateTowardPlayer)
        {
            Vector2 targetDirection =
                (Vector2)player.position - spawnPosition;

            rotation = Mathf.Atan2(
                targetDirection.y,
                targetDirection.x
            ) * Mathf.Rad2Deg + rotationOffset;
        }

        Instantiate(
            prefabToSpawn,
            spawnPosition,
            Quaternion.Euler(0f, 0f, rotation)
        );
    }

    Vector2 GetSpawnDirection()
    {
        SpawnDirection chosenDirection = spawnDirection;

        if (useRandomDirection ||
            chosenDirection == SpawnDirection.Random)
        {
            chosenDirection = (SpawnDirection)Random.Range(0, 4);
        }

        switch (chosenDirection)
        {
            case SpawnDirection.Up:
                return Vector2.up;

            case SpawnDirection.Down:
                return Vector2.down;

            case SpawnDirection.Left:
                return Vector2.left;

            case SpawnDirection.Right:
                return Vector2.right;
        }

        return Vector2.up;
    }
}
