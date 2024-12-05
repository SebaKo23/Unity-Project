using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject Board;
    public static bool isGameOver = false;
    private void Start()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
        isGameOver = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        isGameOver = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        isGameOver = false;
    }
}
