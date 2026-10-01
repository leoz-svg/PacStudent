using System;
using UnityEngine;

public static class MusicSettings
{
    public const string EnabledKey = "PacStudent.Music.Enabled";
    public const string VolumeKey = "PacStudent.Music.Volume";
    public static event Action Changed;
    public static bool Enabled => PlayerPrefs.GetInt(EnabledKey, 1) != 0;
    public static float Volume => Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, .5f));

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize() { ApplyMaster(); }
    static void ApplyMaster() { AudioListener.volume = Enabled ? Volume : 0f; }

    public static void SetEnabled(bool value)
    {
        PlayerPrefs.SetInt(EnabledKey, value ? 1 : 0);
        ApplyMaster();
        Changed?.Invoke();
        Save();
    }
    public static void SetVolume(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, Mathf.Clamp01(value));
        ApplyMaster();
        Changed?.Invoke();
    }
    public static void Save() { PlayerPrefs.Save(); }
}
