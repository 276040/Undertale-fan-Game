
using System.Collections.Generic;
using UnityEngine;

public class GasterBlasterSpawner : MonoBehaviour
{
    [Header("Blaster Prefab")]
    public GameObject blasterPrefab;

    [Header("Spawn Schedule")]
    public float startSpawningTime = 5f;
    public float endSpawningTime = 30f;

    [Header("Spawn Settings")]
    public float spawnDistance = 10f;
    public float spawnInterval = 1f;
    public bool repeatSpawning = true;

    [Header("Visibility")]
    public bool forceEnableSprites = true;
    public int spriteSortingOrder = 100;

    private readonly List<GameObject> blasters =
        new List<GameObject>();

    private Transform player;
    private float timer;
    private float nextSpawnTime;
    private bool finished;

    void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        if (blasterPrefab == null)
        {
            Debug.LogError(
                "Gaster Spawner: Assign the prefab in the Inspector.");
            enabled = false;
            return;
        }

        if (player == null)
        {
            Debug.LogError(
                "Gaster Spawner: Player not found. Check the Player tag.");
            enabled = false;
            return;
        }

        nextSpawnTime = startSpawningTime;
    }

    void Update()
    {
        if (finished)
            return;

        timer += Time.deltaTime;

        if (timer >= endSpawningTime)
        {
            finished = true;
            Debug.Log("Gaster Spawner: Finished spawning.");
            return;
        }

        if (timer < nextSpawnTime)
            return;

        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player == null)
        {
            nextSpawnTime = timer + 0.5f;
            return;
        }

        SpawnOrReuseBlaster();

        if (repeatSpawning)
            nextSpawnTime = timer + Mathf.Max(0.1f, spawnInterval);
        else
            finished = true;
    }

    void SpawnOrReuseBlaster()
    {
        GameObject blaster = null;

        // Find a hidden blaster we can reuse.
        foreach (GameObject existing in blasters)
        {
            if (existing != null && !existing.activeInHierarchy)
            {
                blaster = existing;
                break;
            }
        }

        Vector2 direction = Random.insideUnitCircle.normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector2.right;

        Vector3 position = player.position +
            (Vector3)(direction * spawnDistance);

        // Create one only if no hidden blaster is available.
        if (blaster == null)
        {
            blaster = Instantiate(
                blasterPrefab, position, Quaternion.identity);

            blaster.name = "Pooled Gaster Blaster";
            blasters.Add(blaster);
        }
        else
        {
            blaster.transform.SetPositionAndRotation(
                position, Quaternion.identity);
        }

        blaster.SetActive(true);

        SpriteRenderer[] renderers =
            blaster.GetComponentsInChildren<SpriteRenderer>(true);

        if (forceEnableSprites)
        {
            foreach (SpriteRenderer sr in renderers)
            {
                sr.gameObject.SetActive(true);
                sr.enabled = true;
                sr.sortingOrder = spriteSortingOrder;
            }
        }

        blaster.SendMessage(
            "SetTarget", player,
            SendMessageOptions.DontRequireReceiver);

        // Notify the attack script that this blaster is being reused.
        blaster.SendMessage(
            "ResetForReuse",
            SendMessageOptions.DontRequireReceiver);

        Debug.Log("Gaster Blaster shown or reused.");
    }
}
