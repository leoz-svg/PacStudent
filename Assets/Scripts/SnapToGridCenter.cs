using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]   
#endif
public class SnapToGridCenter : MonoBehaviour
{
    public Grid grid;  

    void LateUpdate()
    {
        if (grid == null) grid = GetComponentInParent<Grid>();
        if (grid == null) return;

        
        Vector3Int cell = grid.WorldToCell(transform.position);
        Vector3 center = grid.GetCellCenterWorld(cell);
        center.z = 0f;
        transform.position = center;
    }

    
    [ContextMenu("Snap Now")]
    void SnapNow()
    {
        LateUpdate();
    }
}

