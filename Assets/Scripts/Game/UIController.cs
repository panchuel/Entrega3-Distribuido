using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Game UI")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] ScoreManager scoreManager;

    [Header("Menu")]
    [SerializeField] GameObject menuUI;

    private void Start()
    {
        Time.timeScale = 0;
        scoreManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreManager>();
    }

    public void UpdateScore()
    {
        scoreText.text = scoreManager.score.ToString();
    }

    public void PlayButton()
    {
        menuUI.SetActive(false);
        Time.timeScale = 1;
    }


}
