using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class GhostControllerGrid : MonoBehaviour
{
    [Header("Refs")]
    public Grid grid;
    public Tilemap walls;              
    public Tilemap gates;              
    public Transform player;           
    public Animator animator;          

    [Header("Speed (cells/sec)")]
    public float normalSpeed = 6f;     
    public float scaredSpeed = 3.5f;   

    [Header("Behaviour")]
    public bool chaseWhenCalm = true;  
    public bool avoidBacktrack = true; 
    public float releaseDelay = 0f;    

    [Header("Tunnel (optional)")]
    public bool forbidTunnels = true;                 
    public Vector3Int leftGateCell = new Vector3Int(-13, 0, 0);
    public Vector3Int rightGateCell = new Vector3Int(12, 0, 0);

    
    Vector3Int curCell;
    Vector3Int dir = Vector3Int.left;
    Vector3 targetPos;
    float waitLeft;

    void Start()
    {
        if (!grid) grid = GetComponentInParent<Grid>();
        if (!animator) animator = GetComponent<Animator>();

        curCell = grid.WorldToCell(transform.position);
        targetPos = grid.GetCellCenterWorld(curCell);
        transform.position = targetPos;

        waitLeft = releaseDelay;
        ApplyAnim(Vector3Int.zero);
    }

    void Update()
    {
        if (waitLeft > 0f)
        {
            waitLeft -= Time.deltaTime;
            return;
        }

        float cellsPerSecond =
            (GameSystem.Instance != null && GameSystem.Instance.IsScared)
            ? scaredSpeed : normalSpeed;

        if ((transform.position - targetPos).sqrMagnitude < 0.0001f)
        {
            curCell = grid.WorldToCell(transform.position);

            Vector3Int nextDir = ChooseDir(curCell, dir);
            dir = nextDir; 
            ApplyAnim(dir);

            targetPos = grid.GetCellCenterWorld(curCell + dir);
        }

        float step = cellsPerSecond * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
    }

    
    bool AdjacentToGate(Vector3Int cell)
    {
        if (!gates) return false;
        return gates.HasTile(cell + Vector3Int.up) || gates.HasTile(cell + Vector3Int.down);
    }

    Vector3Int ChooseDir(Vector3Int cell, Vector3Int currentDir)
    {
        Vector3Int[] all = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };
        List<Vector3Int> options = new List<Vector3Int>(4);

        foreach (var d in all)
        {
            if (!CanGo(cell, d)) continue;

            if (avoidBacktrack && d == -currentDir && HasOtherWay(cell, currentDir))
                continue; 

            options.Add(d);
        }

        
        if (AdjacentToGate(cell))
            options.RemoveAll(d => d.x != 0);

        if (options.Count == 0) return Vector3Int.zero;
        if (options.Count == 1) return options[0];

        if (!player || (!chaseWhenCalm && !IsScared()))
            return options[Random.Range(0, options.Count)];

        Vector3Int playerCell = grid.WorldToCell(player.position);
        bool flee = IsScared();
        float bestScore = flee ? float.NegativeInfinity : float.PositiveInfinity;
        Vector3Int best = options[0];

        foreach (var d in options)
        {
            Vector3Int dest = cell + d;
            float dist2 = (playerCell - dest).sqrMagnitude;

            if (flee)
            {
                if (dist2 > bestScore) { bestScore = dist2; best = d; }
            }
            else
            {
                if (dist2 < bestScore) { bestScore = dist2; best = d; }
            }
        }
        return best;
    }

    bool CanGo(Vector3Int from, Vector3Int d)
    {
        if (d == Vector3Int.zero) return false;

        Vector3Int next = from + d;

        if (forbidTunnels)
        {
            if ((from == leftGateCell && d == Vector3Int.left) ||
                (from == rightGateCell && d == Vector3Int.right))
                return false;
        }

        if (walls && walls.HasTile(next))
            return false;

        
        if (gates && gates.HasTile(next))
            return (d == Vector3Int.up || d == Vector3Int.down);

        return true;
    }

    bool HasOtherWay(Vector3Int cell, Vector3Int currentDir)
    {
        Vector3Int[] all = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };
        foreach (var d in all)
        {
            if (d == -currentDir) continue;
            if (CanGo(cell, d)) return true;
        }
        return false;
    }

    void ApplyAnim(Vector3Int d)
    {
        if (!animator) return;
        Vector2 v = new Vector2(d.x, d.y);
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y)) v = new Vector2(Mathf.Sign(v.x), 0);
        else if (v != Vector2.zero) v = new Vector2(0, Mathf.Sign(v.y));
        animator.SetFloat("MoveX", v.x);
        animator.SetFloat("MoveY", v.y);
    }

    bool IsScared()
    {
        return GameSystem.Instance != null && GameSystem.Instance.IsScared;
    }
}





