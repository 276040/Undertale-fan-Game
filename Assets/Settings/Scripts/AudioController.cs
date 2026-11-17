
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(AudioSource))]
public class PlaySoundWhenOnScreen : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip appearSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Playback Settings")]
    public float soundDuration = 1.5f;
    public bool use2DSound = true;

    private Renderer objectRenderer;
    private AudioSource audioSource;
    private float soundTimer;
    private bool soundStarted;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        audioSource.spatialBlend = use2DSound ? 0f : 1f;
    }

    private void Update()
    {
        if (appearSound == null)
            return;

        if (!objectRenderer.isVisible)
        {
            audioSource.Stop();
            soundTimer = 0f;
            soundStarted = false;
            return;
        }

        if (!soundStarted)
        {
            audioSource.clip = appearSound;
            audioSource.Play();

            soundTimer = 0f;
            soundStarted = true;
        }

        soundTimer += Time.deltaTime;

        if (soundTimer >= soundDuration)
        {
            audioSource.Stop();
            soundStarted = true;
        }
    }

    private void OnDisable()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}
