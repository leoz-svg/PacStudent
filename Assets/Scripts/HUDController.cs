using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI scoreValue;
    public TextMeshProUGUI timeValue;
    public Transform livesContainer;   // LivesContainer
    public TextMeshProUGUI scaredValue;

    Image[] lifeIcons;

    void Awake()
    {
        if (livesContainer) lifeIcons = livesContainer.GetComponentsInChildren<Image>(true);
    }

    public void SetLevel(int level)
    {
        if (levelLabel) levelLabel.text = $"LEVEL {level}";
    }

    public void SetScore(int score)
    {
        if (scoreValue) scoreValue.text = score.ToString("D6"); 
    }

    public void SetTime(float seconds)
    {
        if (!timeValue) return;
        int h = Mathf.FloorToInt(seconds / 3600f);
        int m = Mathf.FloorToInt((seconds % 3600f) / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        timeValue.text = $"{h:00}:{m:00}:{s:00}";
    }

    public void SetLives(int lives)
    {
        if (lifeIcons == null) return;
        for (int i = 0; i < lifeIcons.Length; i++)
            lifeIcons[i].enabled = (i < lives);
    }

    public void SetScaredTime(float leftSeconds)
    {
        if (scaredValue) scaredValue.text = Mathf.CeilToInt(leftSeconds).ToString("00");
    }
}
