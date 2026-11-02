
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public Button retryButton;

    private bool gameOver = false;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(Retry);
    }

    public void ShowGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        else
            Debug.LogError("Game Over Panel is not assigned!");

        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
