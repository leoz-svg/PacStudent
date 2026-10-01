using UnityEngine;

// Music mix trim only. MusicSettings controls the master listener for every sound.
[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class MusicVolumeBinding : MonoBehaviour
{
    AudioSource source;
    void Awake() { source = GetComponent<AudioSource>(); Apply(); }
    void OnEnable() { MusicSettings.Changed += Apply; Apply(); }
    void OnDisable() { MusicSettings.Changed -= Apply; }
    void Apply()
    {
        if (!source) source = GetComponent<AudioSource>();
        source.mute = false;
        source.volume = .6f;
    }
    void OnApplicationPause(bool paused) { if (paused) MusicSettings.Save(); }
    void OnApplicationQuit() { MusicSettings.Save(); }
}
