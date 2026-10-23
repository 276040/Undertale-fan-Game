
using UnityEngine;

public class MusicStartAtTime : MonoBehaviour
{
    [Header("Music")]
    public AudioSource musicSource;

    [Header("Song Position")]
    public float startSongAtSeconds = 30f;

    [Header("Playback")]
    public bool playOnSceneStart = true;
    public bool loopMusic = false;

    void Start()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
        {
            Debug.LogError(
                "MusicStartAtTime needs an AudioSource!"
            );
            return;
        }

        musicSource.playOnAwake = false;
        musicSource.loop = loopMusic;

        if (playOnSceneStart)
        {
            PlaySong();
        }
    }

    public void PlaySong()
    {
        if (musicSource == null ||
            musicSource.clip == null)
        {
            Debug.LogError(
                "Assign an AudioClip to the AudioSource!"
            );
            return;
        }

        musicSource.Stop();

        // Start at the chosen position in the song.
        musicSource.time = Mathf.Clamp(
            startSongAtSeconds,
            0f,
            musicSource.clip.length
        );

        musicSource.Play();
    }
}
