using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_Start : MonoBehaviour
{
    [Header("Movement setting")]
    public float moveSpeed = 3f;          
    public int startCornerIndex = 0;      
    public bool consumePelletsInThisScene = false; 

    
    private Vector2[] waypoints;
    private int currentWaypoint = 0;

    private Animator animator;
    private AudioSource moveAudioSource;
    public AudioClip moveClip;

    void Start()
    {
        animator = GetComponent<Animator>();

        
        waypoints = new Vector2[4];
        waypoints[0] = new Vector2(-23.5f, 13.5f); // Upper Left
        waypoints[1] = new Vector2(23.5f, 13.5f); // Upper Right
        waypoints[2] = new Vector2(23.5f, -13.5f); // Lower Right
        waypoints[3] = new Vector2(-23.5f, -13.5f); // Lower Left

        
        startCornerIndex = Mathf.Clamp(startCornerIndex, 0, 3);
        transform.position = waypoints[startCornerIndex];
        currentWaypoint = (startCornerIndex + 1) % waypoints.Length;

        
        if (moveClip)
        {
            moveAudioSource = gameObject.AddComponent<AudioSource>();
            moveAudioSource.clip = moveClip;
            moveAudioSource.loop = true;
        }
    }

    void Update()
    {
        
        Vector2 targetPosition = waypoints[currentWaypoint];

        
        transform.position = Vector2.MoveTowards(
            transform.position, targetPosition, moveSpeed * Time.deltaTime);

        
        Vector2 raw = (targetPosition - (Vector2)transform.position);
        Vector2 dir = raw.sqrMagnitude < 1e-8f ? Vector2.zero : raw.normalized;

        
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            dir = new Vector2(Mathf.Sign(dir.x), 0f);
        else
            dir = new Vector2(0f, Mathf.Sign(dir.y));

        if (animator)
        {
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        
        if (moveAudioSource && !moveAudioSource.isPlaying)
            moveAudioSource.Play();

        
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!consumePelletsInThisScene) return; 
        if (other.CompareTag("Pellet"))
        {
            if (moveAudioSource) moveAudioSource.Stop();
            Destroy(other.gameObject);
        }
    }
}
