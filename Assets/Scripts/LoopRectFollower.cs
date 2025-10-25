using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class LoopRectFollower : MonoBehaviour
{
    [Header("Path bounds ")]
    public float xl = -23.5f;   
    public float xr = 23.5f;   
    public float yt = 13.5f;   
    public float yb = -13.5f;   

    [Header("Movement")]
    public float tileSize = 1f;           
    public float speedTilesPerSec = 5f;   
    public int startOffsetTiles = 0;    
    public bool clockwise = true;        

    [Header("Animation")]
    public Animator animator;             

    private List<Vector2> pts;
    private int idx;                      

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        BuildLoopPoints();
    }

    void Start()
    {
        if (pts == null || pts.Count < 2) { enabled = false; return; }

        
        int start = Mod(startOffsetTiles, pts.Count);
        transform.position = pts[start];
        idx = (start + 1) % pts.Count;    
    }

    void Update()
    {
        Vector2 target = pts[idx];
        float speed = speedTilesPerSec * tileSize; 

        transform.position = Vector2.MoveTowards(
            transform.position, target, speed * Time.deltaTime);

        
        if (animator)
        {
            Vector2 raw = target - (Vector2)transform.position;
            Vector2 dir = raw.sqrMagnitude < 1e-8f ? Vector2.zero : raw.normalized;
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                dir = new Vector2(Mathf.Sign(dir.x), 0f);
            else
                dir = new Vector2(0f, Mathf.Sign(dir.y));

            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        
        if (((Vector2)transform.position - target).sqrMagnitude < 1e-6f)
            idx = (idx + 1) % pts.Count;
    }

    
    void BuildLoopPoints()
    {
        pts = new List<Vector2>();

        int w = Mathf.RoundToInt((xr - xl) / tileSize);
        int h = Mathf.RoundToInt((yt - yb) / tileSize);

        
        for (int i = 0; i <= w; i++)
            pts.Add(new Vector2(xl + i * tileSize, yt));

        
        for (int i = 1; i <= h; i++)
            pts.Add(new Vector2(xr, yt - i * tileSize));

        
        for (int i = 1; i <= w; i++)
            pts.Add(new Vector2(xr - i * tileSize, yb));

        
        for (int i = 1; i <= h - 1; i++)
            pts.Add(new Vector2(xl, yb + i * tileSize));

        if (!clockwise) pts.Reverse(); 
    }

    int Mod(int a, int m) { int r = a % m; return r < 0 ? r + m : r; }
}

