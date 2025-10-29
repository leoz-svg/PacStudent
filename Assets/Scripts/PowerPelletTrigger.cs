using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapCollider2D), typeof(Tilemap))]
public class PowerPelletTrigger : MonoBehaviour
{
    Tilemap map;

    void Awake() => map = GetComponent<Tilemap>();

    void OnTriggerStay2D(Collider2D other)
    {
        
        if (!other.CompareTag("Player")) return;

        var cell = map.WorldToCell(other.transform.position);

        if (map.HasTile(cell))
        {
            map.SetTile(cell, null);            
            GameSystem.I?.AddScore(50);         
            GameSystem.I?.StartScared();        
            SfxManager.I?.PlayCherry();    
        }
    }
}

