
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class GasterBlasterAttackCircle : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;

    [Header("Appearance Sound")]
    public AudioClip appearSound;

    [Range(0f, 1f)]
    public float soundVolume = 1f;

    public float appearDelay = 0f;

    [Tooltip("Minimum time between appearance sounds from all blasters.")]
    [Min(0f)]
    public float globalSoundCooldown = 0.12f;

    [Header("Blaster Facing")]
    public float rotationOffset = 0f;

    [Header("References")]
    public GasterBlasterLaserController laserController;

    private SpriteRenderer spriteRenderer;
    private Vector3 fixedCenter;
    private bool initialized;
    private bool finished;

    private static AudioSource sharedSoundSource;
    private static float nextGlobalSoundTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (laserController == null)
        {
            laserController =
                GetComponent<GasterBlasterLaserController>();
        }

        if (laserController == null)
        {
            Debug.LogError(
                "Gaster Blaster: Add GasterBlasterLaserController " +
                "to the prefab."
            );
        }

        CreateSharedSoundSource();
    }

    private static void CreateSharedSoundSource()
    {
        if (sharedSoundSource != null)
            return;

        AudioListener listener = FindObjectOfType<AudioListener>();

        GameObject soundObject;

        if (listener != null)
        {
            soundObject = listener.gameObject;
        }
        else
        {
            soundObject = new GameObject(
                "Gaster Blaster Shared Sound"
            );

            Debug.LogWarning(
                "No AudioListener found. Created a separate sound object."
            );
        }

        sharedSoundSource =
            soundObject.GetComponent<AudioSource>();

        if (sharedSoundSource == null)
        {
            sharedSoundSource =
                soundObject.AddComponent<AudioSource>();
        }

        sharedSoundSource.playOnAwake = false;
        sharedSoundSource.loop = false;
        sharedSoundSource.spatialBlend = 0f;
        sharedSoundSource.priority = 32;
    }

    public void Setup(Transform center, float angle, float radius)
    {
        if (center == null)
            return;

        fixedCenter = center.position;

        Vector2 offset = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * radius;

        transform.position = fixedCenter + (Vector3)offset;

        Vector2 inward =
            ((Vector2)fixedCenter -
             (Vector2)transform.position).normalized;

        float facingAngle =
            Mathf.Atan2(inward.y, inward.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f, 0f, facingAngle + rotationOffset
        );

        if (laserController != null)
            laserController.Setup(fixedCenter);

        initialized = true;
        finished = false;

        spriteRenderer.enabled = true;

        if (closedSprite != null)
            spriteRenderer.sprite = closedSprite;
    }

    public IEnumerator RunAttackRounds(
        int rounds,
        float delayAfterSpawn,
        float timeBetweenRounds)
    {
        if (!initialized || laserController == null)
        {
            finished = true;
            yield break;
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, delayAfterSpawn)
        );

        for (int i = 0; i < rounds; i++)
        {
            if (i > 0)
            {
                spriteRenderer.enabled = true;

                if (closedSprite != null)
                    spriteRenderer.sprite = closedSprite;

                yield return new WaitForSeconds(
                    Mathf.Max(0f, timeBetweenRounds)
                );
            }

            if (appearDelay > 0f)
                yield return new WaitForSeconds(appearDelay);

            PlayAppearanceSound();

            if (openSprite != null)
                spriteRenderer.sprite = openSprite;

            yield return StartCoroutine(laserController.Fire());
        }

        HideBlaster();
        finished = true;
    }

    private void PlayAppearanceSound()
    {
        if (appearSound == null)
            return;

        if (sharedSoundSource == null)
            CreateSharedSoundSource();

        if (Time.time < nextGlobalSoundTime)
            return;

        sharedSoundSource.PlayOneShot(
            appearSound,
            soundVolume
        );

        nextGlobalSoundTime =
            Time.time + Mathf.Max(0f, globalSoundCooldown);
    }

    public bool IsFinished()
    {
        return finished;
    }

    public void HideBlaster()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (laserController != null)
            laserController.HideLasers();
    }
}
