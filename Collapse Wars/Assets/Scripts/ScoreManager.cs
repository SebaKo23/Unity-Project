using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public int score = 0;
    public Text scoreText;

    void Start()
    {
        UpdateScoreText();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    public void SaveBestScore(string difficulty)
    {
        int bestScore = PlayerPrefs.GetInt("BestScore_" + difficulty, 0);
        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore_" + difficulty, score);
        }
    }

    public int GetBestScore(string difficulty)
    {
        return PlayerPrefs.GetInt("BestScore_" + difficulty, 0);
    }
}
