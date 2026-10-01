using System.Collections.Generic;
using UnityEngine;

// Hand-designed corridor graph. The Editor bakes it into the second scene's tilemaps.
public static class EnergyHuntMap
{
    public static HashSet<Vector3Int> Corridors()
    {
        var cells=new HashSet<Vector3Int>();
        Line(cells,-13,12,12,12); Line(cells,-13,-13,12,-13);
        Line(cells,-13,-13,-13,12); Line(cells,12,-13,12,12);
        Line(cells,-8,7,7,7); Line(cells,-8,-8,7,-8);
        Line(cells,-8,-8,-8,7); Line(cells,7,-8,7,7);
        // Four links allow switching between the outer escape ring and inner loop.
        Line(cells,-8,7,-8,12); Line(cells,7,-13,7,-8);
        Line(cells,-13,-8,-8,-8); Line(cells,7,7,12,7);
        // Shorter central crossings run immediately above/below the ghost exits.
        Line(cells,-13,2,12,2); Line(cells,-13,-3,12,-3);
        Line(cells,-1,2,-1,7); Line(cells,0,-8,0,-3);
        // Mid-row tunnels connect to the two rings, while preserving the house walls.
        Line(cells,-14,0,-8,0); Line(cells,7,0,13,0);
        return cells;
    }
    static void Line(HashSet<Vector3Int> cells,int x1,int y1,int x2,int y2)
    {
        int x=x1,y=y1; cells.Add(new Vector3Int(x,y,0));
        while(x!=x2||y!=y2) { x+=System.Math.Sign(x2-x); y+=System.Math.Sign(y2-y); cells.Add(new Vector3Int(x,y,0)); }
    }
    public static readonly Vector3Int[] PowerCells={new Vector3Int(-13,-8,0),new Vector3Int(12,7,0),new Vector3Int(-1,3,0),new Vector3Int(0,-4,0)};
}
