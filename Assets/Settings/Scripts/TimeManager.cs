
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameTimeTracker : MonoBehaviour
{
    public static GameTimeTracker Instance;

    [Header("Display")]
    public TMP_Text realTimeText;
    public TMP_Text playTimeText;
    public TMP_Text dateText;

    [Header("Display Settings")]
    public bool showRealTime = true;
    public bool showPlayTime = true;
    public bool showDate = true;
    public bool use24HourClock = false;

    [Header("Scene Timer Settings")]
    public string timerSceneName = "SansFight";
    public string destinationSceneName = "Ending";

    public float timeBeforeSceneChange = 300f;
    public bool enableSceneTimer = true;

    private float totalPlayTime;
    private float sceneTimer;
    private bool sceneTimerFinished;
    private bool wasInTimerScene;

    public float TotalPlayTime => totalPlayTime;
    public float SceneTimer => sceneTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        totalPlayTime += Time.unscaledDeltaTime;

        UpdateDisplays();

        if (!enableSceneTimer)
            return;

        bool isInTimerScene =
            SceneManager.GetActiveScene().name == timerSceneName;

        // Player has entered the timer scene.
        if (isInTimerScene && !wasInTimerScene)
        {
            sceneTimer = 0f;
            sceneTimerFinished = false;
        }

        // Player has left the timer scene.
        if (!isInTimerScene && wasInTimerScene)
        {
            sceneTimer = 0f;
            sceneTimerFinished = false;
        }

        wasInTimerScene = isInTimerScene;

        // Only count while inside the selected scene.
        if (isInTimerScene && !sceneTimerFinished)
        {
            sceneTimer += Time.unscaledDeltaTime;

            if (sceneTimer >= timeBeforeSceneChange)
            {
                sceneTimerFinished = true;

                SceneManager.LoadScene(destinationSceneName);
            }
        }
    }

    void UpdateDisplays()
    {
        DateTime now = DateTime.Now;

        if (realTimeText != null && showRealTime)
        {
            realTimeText.text = use24HourClock
                ? now.ToString("HH:mm:ss")
                : now.ToString("hh:mm:ss tt");
        }

        if (dateText != null && showDate)
        {
            dateText.text = now.ToString("MM/dd/yyyy");
        }

        if (playTimeText != null && showPlayTime)
        {
            playTimeText.text = FormatPlayTime(totalPlayTime);
        }
    }

    string FormatPlayTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);

        return string.Format(
            "{0:D2}:{1:D2}:{2:D2}",
            (int)time.TotalHours,
            time.Minutes,
            time.Seconds
        );
    }

    public void ResetPlayTime()
    {
        totalPlayTime = 0f;
    }

    public void ResetSceneTimer()
    {
        sceneTimer = 0f;
        sceneTimerFinished = false;
    }
}
