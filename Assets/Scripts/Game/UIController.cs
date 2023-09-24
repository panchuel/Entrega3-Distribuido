using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Game UI")]
    [SerializeField] GameObject ball;
    public TextMeshProUGUI scoreText;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GameObject gameUI;

    [Header("Menu")]
    [SerializeField] GameObject menuUI;

    [Header("Scoreboard")]
    [SerializeField] GameObject scoreboardUI;

    private void Start()
    {
        scoreManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreManager>();
    }

    public void UpdateScore()
    {
        scoreText.text = scoreManager.score.ToString();
    }

    public void PlayButton()
    {
        gameUI.SetActive(true);
        menuUI.SetActive(false);       
        Invoke("ActiveGravity", 0.5f);
    }

    public void ShowScoreboard()
    {
        scoreboardUI.SetActive(true);
        menuUI.SetActive(false);
        ball.SetActive(false);
    }

    public void BackToMenu()
    {
        menuUI.SetActive(true);
        ball.SetActive(true);
        scoreboardUI.SetActive(false);
    }

    void ActiveGravity()
    {
        Jiggle.Instance.rb.gravityScale = 1.0f;
    }


}
