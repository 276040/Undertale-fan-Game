
using UnityEngine;
using TMPro;
using System;

public class GameTimeTracker : MonoBehaviour
{
    public static GameTimeTracker Instance;

    [Header("Display")]
    public TMP_Text realTimeText;
    public TMP_Text playTimeText;
    public TMP_Text dateText;

    [Header("Settings")]
    public bool showRealTime = true;
    public bool showPlayTime = true;
    public bool showDate = true;
    public bool use24HourClock = false;

    private float totalPlayTime;

    public float TotalPlayTime => totalPlayTime;

    void Awake()
    {
        // Prevent duplicate trackers when changing scenes.
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
        // Track how long the game has been running.
        totalPlayTime += Time.unscaledDeltaTime;

        UpdateDisplays();
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
}
