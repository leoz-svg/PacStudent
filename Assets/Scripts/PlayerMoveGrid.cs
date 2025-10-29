using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMoveGrid_HoldToMove : MonoBehaviour
{
    [Header("Grid & Tiles")]
    public Grid grid;
    public Tilemap walls;

    [Header("Movement")]
    public float cellsPerSecond = 6f;
    public Animator animator;

    
    [Header("Tunnel Wrap (Left-Right)")]
    [Tooltip("通道可站立的最左侧格 x（含）")]
    public int tunnelMinX = -26;
    [Tooltip("通道可站立的最右侧格 x（含）")]
    public int tunnelMaxX = 26;
    [Tooltip("通道所在的那一行格坐标 y")]
    public int tunnelY = 0;

    
    Vector3Int curCell;
    Vector3Int dir = Vector3Int.zero;    
    Vector3Int held = Vector3Int.zero;   
    Vector3 targetPos;

    void Start()
    {
        if (!grid) grid = GetComponentInParent<Grid>();
        if (!animator) animator = GetComponent<Animator>();

        curCell = grid.WorldToCell(transform.position);
        targetPos = grid.GetCellCenterWorld(curCell);
        transform.position = targetPos;

        ApplyAnim(Vector3Int.zero);

        
        SfxManager.I?.StopMoveLoop();
    }

    void Update()
    {
        
        held = ReadHeldDir();

        
        if ((transform.position - targetPos).sqrMagnitude < 0.0001f)
        {
            curCell = grid.WorldToCell(transform.position);

            
            if (curCell.y == tunnelY)
            {
                bool wrapped = false;

                if (curCell.x < tunnelMinX)                
                {
                    curCell = new Vector3Int(tunnelMaxX, tunnelY, 0);
                    transform.position = grid.GetCellCenterWorld(curCell);
                    wrapped = true;
                }
                else if (curCell.x > tunnelMaxX)           
                {
                    curCell = new Vector3Int(tunnelMinX, tunnelY, 0);
                    transform.position = grid.GetCellCenterWorld(curCell);
                    wrapped = true;
                }

                if (wrapped)
                {
                    
                    if (dir != Vector3Int.zero && CanGo(curCell, dir))
                        targetPos = grid.GetCellCenterWorld(curCell + dir);
                    else
                        targetPos = grid.GetCellCenterWorld(curCell);

                    float stepWrapped = cellsPerSecond * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, stepWrapped);

                    
                    UpdateMoveLoopSfx();
                    return;
                }
            }

            
            if (held == Vector3Int.zero)
            {
                
                if (dir != Vector3Int.zero && !CanGo(curCell, dir))
                {
                    SfxManager.I?.PlayHitWall();
                }

                dir = Vector3Int.zero;
                targetPos = grid.GetCellCenterWorld(curCell);
                ApplyAnim(Vector3Int.zero);

                
                UpdateMoveLoopSfx();
            }
            else
            {
                
                if (CanGo(curCell, held))
                {
                    dir = held;
                    ApplyAnim(dir);
                    targetPos = grid.GetCellCenterWorld(curCell + dir);

                    
                    UpdateMoveLoopSfx();
                }
                else
                {
                    
                    if (dir != Vector3Int.zero && CanGo(curCell, dir))
                    {
                        targetPos = grid.GetCellCenterWorld(curCell + dir);

                        
                        UpdateMoveLoopSfx();
                    }
                    else
                    {
                        
                        SfxManager.I?.PlayHitWall();

                        dir = Vector3Int.zero;
                        targetPos = grid.GetCellCenterWorld(curCell);
                        ApplyAnim(Vector3Int.zero);

                        
                        UpdateMoveLoopSfx();
                    }
                }
            }
        }

        
        float step = cellsPerSecond * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        
        UpdateMoveLoopSfx();
    }

    Vector3Int ReadHeldDir()
    {
        int x = 0, y = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y = -1;

        
        if (x != 0 && y != 0) y = 0;
        return new Vector3Int(x, y, 0);
    }

    bool CanGo(Vector3Int fromCell, Vector3Int d)
    {
        if (d == Vector3Int.zero) return false;
        return !walls.HasTile(fromCell + d);
    }

    void ApplyAnim(Vector3Int d)
    {
        if (!animator) return;

        Vector2 v = new Vector2(d.x, d.y);
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            v = new Vector2(Mathf.Sign(v.x), 0);
        else if (v != Vector2.zero)
            v = new Vector2(0, Mathf.Sign(v.y));

        animator.SetFloat("MoveX", v.x);
        animator.SetFloat("MoveY", v.y);
    }

    
    void UpdateMoveLoopSfx()
    {
        bool isMoving = (dir != Vector3Int.zero) &&
                        ((targetPos - transform.position).sqrMagnitude > 0.000001f);

        if (isMoving) SfxManager.I?.StartMoveLoop();
        else SfxManager.I?.StopMoveLoop();
    }
}



