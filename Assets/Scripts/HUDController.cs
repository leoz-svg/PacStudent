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

    public static string FormatTime(float seconds) { int centis = Mathf.FloorToInt(seconds * 100); return $"{centis / 6000:00}:{centis / 100 % 60:00}:{centis % 100:00}"; }

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
        timeValue.text = FormatTime(seconds);
    }

    public void SetLives(int lives)
    {
        if (lifeIcons == null) return;
        for (int i = 0; i < lifeIcons.Length; i++)
            lifeIcons[i].enabled = (i < lives);
    }

    public void SetScaredTime(float leftSeconds)
    {
        if (scaredValue) { scaredValue.gameObject.SetActive(leftSeconds > 0); scaredValue.text = Mathf.CeilToInt(leftSeconds).ToString(); }
    }
}
