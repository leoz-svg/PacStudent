using UnityEngine;
// A shared clock keeps all loop variations in phase, including interrupted fades.
public class MusicManager : MonoBehaviour
{
 public AudioClip bgmIntro,bgmGhostNormal,bgmGhostScared,bgmGhostDead;
 const float FadeSeconds=.65f, MixVolume=.6f;
 AudioSource[] voices;
 float[] startGain=new float[2];
 int active; float fade; double musicStart; AudioClip requested;
 public bool IsTransitioning => fade < FadeSeconds;
 void Awake()
 {
  var binding=GetComponent<MusicVolumeBinding>(); if(binding) binding.enabled=false;
  var first=GetComponent<AudioSource>(); if(!first) first=gameObject.AddComponent<AudioSource>();
  first.Stop(); first.mute=false;
  voices=new[]{first,gameObject.AddComponent<AudioSource>()};
  foreach(var voice in voices) { voice.playOnAwake=false; voice.spatialBlend=0; voice.volume=0; }
  musicStart=AudioSettings.dspTime;
 }
 public void PlayIntro() { musicStart=AudioSettings.dspTime; Switch(bgmIntro,false); }
 public void SetMood(bool dead,bool scared) { Switch(dead?bgmGhostDead:scared?bgmGhostScared:bgmGhostNormal,true); }
 void Switch(AudioClip clip,bool loop)
 {
  if(!clip || requested==clip) return;
  requested=clip;
  int next=voices[0].clip==clip&&voices[0].isPlaying?0:voices[1].clip==clip&&voices[1].isPlaying?1:1-active;
  if(voices[next].clip!=clip || !voices[next].isPlaying)
  {
   if(voices[next].isPlaying) next=voices[0].volume<voices[1].volume?0:1;
   voices[next].Stop(); voices[next].clip=clip; voices[next].loop=loop; voices[next].volume=0;
   if(loop) voices[next].time=(float)((AudioSettings.dspTime-musicStart)%clip.length);
   voices[next].Play();
  }
  for(int i=0;i<2;i++) startGain[i]=voices[i].volume;
  active=next; fade=0;
 }
 void Update()
 {
  if(voices==null || fade>=FadeSeconds) return;
  fade=Mathf.Min(FadeSeconds,fade+Time.unscaledDeltaTime);
  float t=Mathf.SmoothStep(0,1,fade/FadeSeconds);
  for(int i=0;i<2;i++) voices[i].volume=Mathf.Lerp(startGain[i],i==active?MixVolume:0,t);
  if(fade>=FadeSeconds) voices[1-active].Stop();
 }
}
