using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    
    public static GameSystem I { get; private set; }
    public static GameSystem Instance => I;

    [Header("Config")]
    public int levelIndex = 1;
    public int startLives = 3;
    public float scaredDuration = 8f;

    [Header("Runtime")]
    public int score;
    public int lives;
    public float runTime;

    
    [SerializeField] private float scaredLeft;   

    
    public float ScareTimer => scaredLeft;
    public bool IsScared => scaredLeft > 0f;

    [Header("UI")]
    public HUDController hud;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        
    }

    private void Start()
    {
        lives = startLives;

        hud?.SetLevel(levelIndex);
        hud?.SetScore(score);
        hud?.SetLives(lives);
        hud?.SetTime(0);
        hud?.SetScaredTime(0);
    }

    private void Update()
    {
        
        runTime += Time.deltaTime;
        hud?.SetTime(runTime);

        
        if (scaredLeft > 0f)
        {
            scaredLeft -= Time.deltaTime;
            if (scaredLeft < 0f) scaredLeft = 0f;
            hud?.SetScaredTime(scaredLeft);
        }
    }

    

    public void AddScore(int add)
    {
        score += add;
        hud?.SetScore(score);
    }

    public void LoseLife()
    {
        lives = Mathf.Max(0, lives - 1);
        hud?.SetLives(lives);
        
    }

    
    public void StartScared()
    {
        AddScare(scaredDuration);
    }

    
    public void AddScare(float seconds)
    {
        scaredLeft = Mathf.Max(scaredLeft, seconds);
        hud?.SetScaredTime(scaredLeft);
        
    }
}


