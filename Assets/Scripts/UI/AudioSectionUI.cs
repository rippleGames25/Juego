using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSectionUI : MonoBehaviour
{
    [Header("Master")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_Text masterValueText;

    [Header("Music")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicValueText;

    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValueText;

    void OnEnable()
    {
        var sm = SettingsManager.Instance;
        var d = sm.Data;

        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        masterSlider.SetValueWithoutNotify(d.master);
        musicSlider.SetValueWithoutNotify(d.music);
        sfxSlider.SetValueWithoutNotify(d.sfx);

        UpdateLabel(masterValueText, d.master);
        UpdateLabel(musicValueText, d.music);
        UpdateLabel(sfxValueText, d.sfx);

        // Suscribir eventos
        masterSlider.onValueChanged.AddListener(v => { sm.SetMaster(v); UpdateLabel(masterValueText, v); });
        musicSlider.onValueChanged.AddListener(v => { sm.SetMusic(v); UpdateLabel(musicValueText, v); });
        sfxSlider.onValueChanged.AddListener(v => { sm.SetSFX(v); UpdateLabel(sfxValueText, v); });
    }

    void OnDisable()
    {
        masterSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
    }

    private void UpdateLabel(TMP_Text label, float v01)
    {
        if (!label) return;
        int pct = Mathf.RoundToInt(v01 * 100f);
        label.text = pct + "%";
    }
}
