using UnityEngine;

[DisallowMultipleComponent]
public class SfxManager : MonoBehaviour
{
    public static SfxManager I { get; private set; }

    [Header("Loop SFX")]
    public AudioClip moveLoop;             // SFX-PacMove

    [Header("One-shot SFX")]
    public AudioClip hitWall;              // SFX-WallImpact
    public AudioClip pelletEat;            // SFX-PelletEat
    public AudioClip cherryEat;            // SFX-CherryEat
    public AudioClip pacDeath;             // SFX-PacDeath

    [Header("Volumes")]
    [Range(0, 1)] public float moveVolume = 0.35f;
    [Range(0, 1)] public float hitVolume = 0.8f;
    [Range(0, 1)] public float eatVolume = 0.7f;
    [Range(0, 1)] public float deathVolume = 0.9f;

    AudioSource loopSource;    
    AudioSource pelletSource;
    AudioSource oneShot;       

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        

        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.spatialBlend = 0f; // 2D
        loopSource.playOnAwake = false;

        pelletSource = gameObject.AddComponent<AudioSource>();
        pelletSource.playOnAwake = false;
        pelletSource.spatialBlend = 0f;
        oneShot = gameObject.AddComponent<AudioSource>();
        oneShot.loop = false;
        oneShot.spatialBlend = 0f; // 2D
        oneShot.playOnAwake = false;
    }

    
    public void StartMoveLoop(bool eating = false)
    {
        var clip = eating ? pelletEat : moveLoop;
        if (!clip) return;
        if (loopSource.isPlaying && loopSource.clip == clip) return;
        loopSource.clip = clip;
        loopSource.volume = moveVolume;
        loopSource.Play();
    }
    public void StopMoveLoop()
    {
        if (loopSource.isPlaying) loopSource.Stop();
    }

    
    public void PlayHitWall()
    {
        if (!hitWall) return;
        oneShot.pitch = Random.Range(0.98f, 1.02f);
        oneShot.PlayOneShot(hitWall, hitVolume);
    }
    public void PlayPellet()
    {
        if (!pelletEat) return;
        // Retrigger one dedicated voice; fast collection never stacks many copies.
        pelletSource.clip = pelletEat;
        pelletSource.volume = eatVolume * .45f;
        pelletSource.Play();
    }


    public void PlayCherry()
    {
        if (!cherryEat) return;
        oneShot.PlayOneShot(cherryEat, eatVolume);
    }
    public void PlayPacDeath()
    {
        if (!pacDeath) return;
        oneShot.PlayOneShot(pacDeath, deathVolume);
    }
}
