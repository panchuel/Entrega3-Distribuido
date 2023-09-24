using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LoseManager : MonoBehaviour
{
    int score;
    [HideInInspector] public int highScore;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] Transform ballInitialPosition;
    [SerializeField] GameObject menuUI;
    [SerializeField] TMP_Text highScoreText;
    [SerializeField] UIController scoreText;
    [SerializeField] AuthManager authManager;
    public UnityEvent updateAuth;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            score = scoreManager.score;
            if (score > highScore)
            {
                highScore = score;
                highScoreText.text = highScore.ToString();
                updateAuth.Invoke();
            }
            score = 0;
            ReloadScene();
        }
    }

    private void ReloadScene()
    {
        scoreManager.DeleteScore();
        scoreText.scoreText.text = "0";
        Jiggle.Instance.GetComponent<Transform>().position = ballInitialPosition.position;
        Jiggle.Instance.rb.gravityScale = 0f;
        Jiggle.Instance.rb.velocity = Vector2.zero;
        menuUI.SetActive(true);
    }

}
