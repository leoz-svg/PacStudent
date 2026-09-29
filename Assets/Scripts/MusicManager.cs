using UnityEngine;
public class MusicManager : MonoBehaviour
{
 public AudioClip bgmIntro,bgmGhostNormal,bgmGhostScared,bgmGhostDead;
 AudioSource source;
 void Awake() { source=GetComponent<AudioSource>(); if(!source) source=gameObject.AddComponent<AudioSource>(); source.playOnAwake=false; source.volume=.3f; }
 public void PlayIntro() { Play(bgmIntro,false); }
 public void SetMood(bool dead,bool scared) { Play(dead?bgmGhostDead:scared?bgmGhostScared:bgmGhostNormal,true); }
 void Play(AudioClip clip,bool loop) { if(!clip || source.clip==clip&&source.isPlaying) return; source.clip=clip; source.loop=loop; source.Play(); }
}
