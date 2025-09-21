using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip bgmIntro;    // BGM-GameIntro
    public AudioClip bgmGhostNormal; // BGM-GhostNormal
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = false; //  Intro does not loop, subsequent bgm loops

        // Play the intro music first
        audioSource.clip = bgmIntro;
        audioSource.Play();

        // After 3 seconds or when the intro ends, switch to the ghost's normal bgm
        float delay = Mathf.Min(bgmIntro.length, 3f);
        Invoke("PlayGhostNormalBGM", delay);
    }

    void PlayGhostNormalBGM()
    {
        audioSource.clip = bgmGhostNormal;
        audioSource.loop = true;
        audioSource.Play();
    }
}
