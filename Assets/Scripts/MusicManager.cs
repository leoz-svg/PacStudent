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
        audioSource.loop = false; //  intro不循环，后续bgm循环

        // 先播放intro音乐
        audioSource.clip = bgmIntro;
        audioSource.Play();

        // 3秒后或intro结束时，切换到幽灵正常状态的bgm
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
