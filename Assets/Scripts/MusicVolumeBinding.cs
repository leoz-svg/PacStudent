using UnityEngine;

// Attach only to music sources, keeping gameplay sound effects independent.
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
        source.mute = !MusicSettings.Enabled;
        source.volume = .6f * MusicSettings.Volume;
    }
    void OnApplicationPause(bool paused) { if (paused) MusicSettings.Save(); }
    void OnApplicationQuit() { MusicSettings.Save(); }
}
