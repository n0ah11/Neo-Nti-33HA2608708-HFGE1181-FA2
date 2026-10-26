using System;
using TMPro;
using UnityEngine;

public class GameMNGR : MonoBehaviour
{
    public static GameMNGR Instance;
    private int score = 0;
   
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void IncrementScore(int value)
    {
        score += value;
        Debug.Log("Score: " + score);
    }

    public int GetScore()
    {
        return score;
    }
    
}
