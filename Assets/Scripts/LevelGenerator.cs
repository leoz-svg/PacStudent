using UnityEngine;
using UnityEngine.Tilemaps;

// Exposes the existing hand-built tilemap as the common movement grid.
public class LevelGenerator : MonoBehaviour
{
    public Grid grid;
    public Tilemap walls, gates, pellets, powerPellets;
    public int[,] levelMap;
    public BoundsInt bounds;
    public static readonly Vector3Int[] Directions = { Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left };
    void Awake() { RefreshMap(); }
    public void RefreshMap()
    {
        walls.CompressBounds(); bounds = walls.cellBounds;
        levelMap = new int[bounds.size.y, bounds.size.x];
        foreach (var c in bounds.allPositionsWithin)
            levelMap[c.y - bounds.yMin, c.x - bounds.xMin] = walls.HasTile(c) ? 1 : gates.HasTile(c) ? 2 : powerPellets.HasTile(c) ? 4 : pellets.HasTile(c) ? 3 : 0;
    }
    public Vector3 Center(Vector3Int cell) => grid.GetCellCenterWorld(cell);
    public Vector3Int Cell(Vector3 position) => grid.WorldToCell(position);
    public bool InHouse(Vector3Int c) => c.x >= -3 && c.x <= 2 && c.y >= -1 && c.y <= 0;
    public bool CanStep(Vector3Int from, Vector3Int direction, bool ghost = false, bool leaving = false)
    {
        if (direction == Vector3Int.zero) return false;
        var next = from + direction;
        if (!bounds.Contains(next)) return !ghost && from.y == 0 && direction.y == 0;
        if (walls.HasTile(next)) return false;
        if (gates.HasTile(next)) return ghost && leaving;
        if (ghost && !leaving && InHouse(next)) return false;
        // Only the middle row connects to the two outside tunnels.
        if (ghost && next.y == 0 && (next.x <= bounds.xMin + 1 || next.x >= bounds.xMax - 2)) return false;
        return true;
    }
    public int CountFood()
    {
        int count = 0;
        foreach (var c in bounds.allPositionsWithin)
            if (pellets.HasTile(c) || powerPellets.HasTile(c)) count++;
        return count;
    }
}
