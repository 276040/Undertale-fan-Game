
using UnityEngine;
using System.Collections;

public class GasterBlasterCircleSpawner : MonoBehaviour
{
    [Header("Blaster Settings")]
    public GameObject blasterPrefab;
    public Transform orbitCenter;

    [Header("Circle Settings")]
    [Range(8, 64)]
    public int numberOfBlasters = 32;

    public float orbitRadius = 10f;
    public float orbitSpeed = 45f;
    public bool rotateClockwise = true;

    [Header("Blaster Facing")]
    [Tooltip("Rotates every blaster to adjust which direction its mouth faces.")]
    [Range(-180f, 180f)]
    public float rotationOffset = 0f;

    [Header("Appearance Timing")]
    public float startTime = 5f;
    public float stopTime = 30f;
    public float timeBetweenSpawns = 0.08f;

    [Header("Firing Timing")]
    public float timeBetweenShots = 1f;
    public bool repeatAttack = true;
    public float timeBetweenRounds = 2f;

    [Header("Appearance Settings")]
    public bool forceEnableSprites = true;
    public int spriteSortingOrder = 100;

    private float fightStartTime;
    private GasterBlasterAttackCircle[] blasters;

    private void Start()
    {
        fightStartTime = Time.time;

        if (blasterPrefab == null || orbitCenter == null)
        {
            Debug.LogError("Gaster Circle: Assign the Blaster Prefab and Orbit Center!");
            enabled = false;
            return;
        }

        numberOfBlasters = Mathf.Clamp(numberOfBlasters, 8, 64);
        StartCoroutine(MainAttackRoutine());
    }

    private IEnumerator MainAttackRoutine()
    {
        yield return new WaitForSeconds(startTime);

        while (Time.time - fightStartTime < stopTime)
        {
            yield return StartCoroutine(SpawnCircle());

            if (Time.time - fightStartTime >= stopTime)
                break;

            yield return StartCoroutine(FireCircleSequence());

            if (!repeatAttack)
                break;

            yield return new WaitForSeconds(timeBetweenRounds);
        }

        HideAllBlasters();
    }

    private IEnumerator SpawnCircle()
    {
        blasters = new GasterBlasterAttackCircle[numberOfBlasters];

        for (int i = 0; i < numberOfBlasters; i++)
        {
            if (Time.time - fightStartTime >= stopTime)
                yield break;

            float angle = i * (360f / numberOfBlasters);

            Vector2 offset = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * orbitRadius;

            Vector3 spawnPosition = orbitCenter.position + (Vector3)offset;

            GameObject newBlaster = Instantiate(
                blasterPrefab,
                spawnPosition,
                Quaternion.identity
            );

            newBlaster.SetActive(true);

            GasterBlasterAttackCircle attack =
                newBlaster.GetComponent<GasterBlasterAttackCircle>();

            if (attack == null)
                attack = newBlaster.AddComponent<GasterBlasterAttackCircle>();

            SpriteRenderer[] renderers =
                newBlaster.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer sr in renderers)
            {
                if (forceEnableSprites)
                    sr.enabled = true;

                sr.sortingOrder = spriteSortingOrder;
            }

            attack.Setup(
                orbitCenter,
                orbitRadius,
                angle,
                orbitSpeed,
                rotateClockwise
            );

            // Apply the spawner's rotation offset to this blaster.
            attack.SetRotationOffset(rotationOffset);

            blasters[i] = attack;

            yield return new WaitForSeconds(
                Mathf.Max(0.01f, timeBetweenSpawns)
            );
        }
    }

    private IEnumerator FireCircleSequence()
    {
        for (int i = 0; i < blasters.Length; i++)
        {
            if (Time.time - fightStartTime >= stopTime)
                yield break;

            if (blasters[i] != null)
                blasters[i].Fire();

            yield return new WaitForSeconds(
                Mathf.Max(0.05f, timeBetweenShots)
            );
        }
    }

    private void HideAllBlasters()
    {
        if (blasters == null)
            return;

        foreach (GasterBlasterAttackCircle blaster in blasters)
        {
            if (blaster != null)
                blaster.HideBlaster();
        }
    }
}
