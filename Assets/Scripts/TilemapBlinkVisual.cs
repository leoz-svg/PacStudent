using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

[RequireComponent(typeof(TilemapRenderer))]
public class TilemapBlinkVisual : MonoBehaviour
{
    public float onDuration = 0.25f;
    public float offDuration = 0.25f;

    private TilemapRenderer rend;
    private Coroutine routine;

    private void Awake()
    {
        rend = GetComponent<TilemapRenderer>();
    }

    private void OnEnable()
    {
        routine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        if (rend != null) rend.enabled = true; // 
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            
            rend.enabled = true;
            yield return new WaitForSeconds(onDuration);

            
            rend.enabled = false;
            yield return new WaitForSeconds(offDuration);
        }
    }
}
