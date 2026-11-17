
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimedSceneChange : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;

    [Header("Scene Timer")]
    [Tooltip("Seconds after this scene loads before changing scenes.")]
    public float timeBeforeSceneChange = 30f;

    private bool sceneChanging;

    private void Update()
    {
        if (sceneChanging || string.IsNullOrWhiteSpace(sceneToLoad))
            return;

        // Time since the current scene was loaded.
        if (Time.timeSinceLevelLoad >= timeBeforeSceneChange)
        {
            sceneChanging = true;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
