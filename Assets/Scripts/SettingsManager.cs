using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class SettingsData
{
    public float master = 1f, music = 1f, sfx = 1f;
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer; 
    SettingsData data = new SettingsData();
    public SettingsData Data => data;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        ApplyAll();
    }

    // Audio
    public void SetMaster(float v) { data.master = v; SetMixer("MasterVol", v); }
    public void SetMusic(float v) { data.music = v; SetMixer("MusicVol", v); }
    public void SetSFX(float v) { data.sfx = v; SetMixer("SFXVol", v); }


    public void ApplyAll()
    {
        SetMixer("MasterVol", data.master);
        SetMixer("MusicVol", data.music);
        SetMixer("SFXVol", data.sfx);
    }

    void SetMixer(string param, float v01)
    {
        float dB = Mathf.Log10(Mathf.Clamp(v01, 0.0001f, 1f)) * 20f;
        if (mixer) mixer.SetFloat(param, dB);
    }
}
