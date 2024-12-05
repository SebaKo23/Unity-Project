using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject difficultyPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject highScorePanel;
    public TextMeshProUGUI easyBestScoreText;
    public TextMeshProUGUI mediumBestScoreText;
    public TextMeshProUGUI hardBestScoreText;

    private void Start()
    {
        UpdateBestScores();
    }

    public void UpdateBestScores()
    {
        easyBestScoreText.text = "Easy: " + PlayerPrefs.GetInt("BestScore_Easy", 0).ToString();
        mediumBestScoreText.text = "Medium: " + PlayerPrefs.GetInt("BestScore_Medium", 0).ToString();
        hardBestScoreText.text = "Hard: " + PlayerPrefs.GetInt("BestScore_Hard", 0).ToString();
    }

    public void StartGameEasy()
    {
        SceneManager.LoadScene("Easy"); 
    }

    public void StartGameMedium()
    {
        SceneManager.LoadScene("Medium"); 
    }

    public void StartGameHard()
    {
        SceneManager.LoadScene("Hard"); 
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        highScorePanel.SetActive(false);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        creditsPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        highScorePanel.SetActive(false);
    }

    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(true);
        difficultyPanel.SetActive(false);
        highScorePanel.SetActive(false);
    }
    
    public void ShowDifficulty()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        difficultyPanel.SetActive(true);
        highScorePanel.SetActive(false);
    }

    public void ShowHighScores()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        highScorePanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
