using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement setting")]
    public float moveSpeed = 3f; // movement speed

    // four endpoint coordinates（clockwise）
    private Vector2[] waypoints;
    private int currentWaypoint = 0; // Current target point index
    private Animator animator;
    private AudioSource moveAudioSource;
    public AudioClip moveClip;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Initialize the four endpoint coordinates (in clockwise order: upper left → upper right → lower right → lower left)
        waypoints = new Vector2[4];
        waypoints[0] = new Vector2(-12.5f, 12.5f);  // Upper left endpoint
        waypoints[1] = new Vector2(-7.5f, 12.5f);   // Upper right endpoint
        waypoints[2] = new Vector2(-7.5f, 8.5f);      // lower right endpoint
        waypoints[3] = new Vector2(-12.5f, 8.5f);     // Lower left endpoint

        // Let the character's initial position be at the upper left endpoint
        transform.position = waypoints[0];

        moveAudioSource = gameObject.AddComponent<AudioSource>();
        moveAudioSource.clip = moveClip;
        moveAudioSource.loop = true;
    }

    void Update()
    {
        // Get the current target point
        Vector2 targetPosition = waypoints[currentWaypoint];

        // Move to target point (frame rate independent smooth movement)
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );


        // Calculate the movement direction (for animation switching)
        Vector2 moveDirection = (targetPosition - (Vector2)transform.position).normalized;
        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveY", moveDirection.y);

        if (!moveAudioSource.isPlaying)
        {
            moveAudioSource.Play();
        }

        // When approaching the target point (distance less than 0.01), switch to the next point
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
        {
            // Cycle to switch to the next point (0→1→2→3→0...)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            moveAudioSource.Stop(); // 碰撞能量豆时停止移动音效
            Destroy(other.gameObject);
        }
    }
}


