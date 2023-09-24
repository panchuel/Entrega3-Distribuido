using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int score {  get; private set; }

    public void AddScore()
    {
        score += 1;
    }

    public void DeleteScore()
    {
        score = 0;
    }
}
