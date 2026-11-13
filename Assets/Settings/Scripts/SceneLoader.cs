
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName = "Fight";

    public void LoadScene()
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Cannot load scene: '" + sceneName +
                "'. Check the exact scene name and Build Profiles.");
        }
    }
}
