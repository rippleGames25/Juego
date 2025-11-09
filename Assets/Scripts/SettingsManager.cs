using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class SettingsData
{
    public float master = 1f, music = 1f, sfx = 1f;
    public bool fullscreen = true;

    public float uiScale = 1f;              // 0.8–1.4
    public bool highContrast = false;

    public int cursorSizeIndex = 1;         // 0=pequeño,1=medio,2=grande
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioMixer mixer; // expone MasterVol/MusicVol/SFXVol

    [Header("UI Scaling Roots (asignar en cada escena opcionalmente)")]
    [SerializeField] private CanvasScaler[] canvasScalers; // HUD, PauseCanvas, MainMenu...

    [Header("High Contrast Theme")]
    [SerializeField] private Color normalText = Color.white;
    [SerializeField] private Color normalPanel = new Color(1, 1, 1, 0.15f);
    [SerializeField] private Color highText = Color.black;
    [SerializeField] private Color highPanel = new Color(1, 1, 1, 0.85f);

    [Header("Cursor (UI follower)")]
    [SerializeField] private RectTransform cursorUI; // Image que sigue al ratón
    [SerializeField] private Vector3[] cursorScales = { new(0.8f, 0.8f, 1f), Vector3.one, new(1.3f, 1.3f, 1f) };

    const string KEY = "SETTINGS_V1";
    SettingsData data = new SettingsData();
    public SettingsData Data => data;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        Load(); ApplyAll();
    }

    // -------- AUDIO --------
    public void SetMaster(float v) { data.master = v; SetMixer("MasterVol", v); }
    public void SetMusic(float v) { data.music = v; SetMixer("MusicVol", v); }
    public void SetSFX(float v) { data.sfx = v; SetMixer("SFXVol", v); }

    // -------- VIDEO --------
    public void SetFullscreen(bool fs) { data.fullscreen = fs; Screen.fullScreen = fs; }

    // -------- UI SCALE (fácil, sin rehacer nada) --------
    // Si usáis CanvasScaler = Scale With Screen Size, multiplicamos su "scaleFactor" a mayores.
    public void SetUiScale(float s)
    {
        data.uiScale = Mathf.Clamp(s, 0.8f, 1.4f);
        if (canvasScalers == null) return;
        foreach (var cs in canvasScalers)
            if (cs) cs.scaleFactor = data.uiScale;
    }

    // -------- ALTO CONTRASTE --------
    public void SetHighContrast(bool on)
    {
        data.highContrast = on;
        var texts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var t in texts) t.color = on ? highText : normalText;

        var images = FindObjectsOfType<Image>(true);
        foreach (var img in images)
        {
            if (img.raycastTarget == false) continue; // heurística: solo paneles visibles
            var c = img.color; c = on ? highPanel : normalPanel; img.color = new Color(c.r, c.g, c.b, img.color.a);
        }
    }

    // -------- CURSOR SIZE (con imagen UI seguidora) --------
    public void SetCursorSizeIndex(int idx)
    {
        data.cursorSizeIndex = Mathf.Clamp(idx, 0, cursorScales.Length - 1);
        if (cursorUI) cursorUI.localScale = cursorScales[data.cursorSizeIndex];
    }

    // Llamar desde un script que siga el ratón (Update en CursorFollower)
    public void RegisterCursorFollower(RectTransform rt) { cursorUI = rt; SetCursorSizeIndex(data.cursorSizeIndex); }

    // -------- Persistencia --------
    public void Save() { PlayerPrefs.SetString(KEY, JsonUtility.ToJson(data)); PlayerPrefs.Save(); }
    void Load() { if (PlayerPrefs.HasKey(KEY)) data = JsonUtility.FromJson<SettingsData>(PlayerPrefs.GetString(KEY)); }

    public void ApplyAll()
    {
        SetMixer("MasterVol", data.master);
        SetMixer("MusicVol", data.music);
        SetMixer("SFXVol", data.sfx);
        SetFullscreen(data.fullscreen);
        SetUiScale(data.uiScale);
        SetHighContrast(data.highContrast);
        SetCursorSizeIndex(data.cursorSizeIndex);
    }

    void SetMixer(string param, float v01)
    {
        float dB = Mathf.Log10(Mathf.Clamp(v01, 0.0001f, 1f)) * 20f;
        if (mixer) mixer.SetFloat(param, dB);
    }

    // Llamar desde cada escena (ej: en Awake de un SceneBinder) para registrar sus CanvasScalers
    public void RegisterCanvasScalers(CanvasScaler[] scalers)
    {
        canvasScalers = scalers;
        SetUiScale(data.uiScale);
    }
}
