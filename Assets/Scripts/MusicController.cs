using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    public static MusicController Instance { get; private set; }

    [Header("Clips (opcional)")]
    public AudioClip mainMenuClip;
    public AudioClip gameClip;
    public bool useDifferentClips = false;

    AudioSource src;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();

        // 🔹 Si tienes un AudioMixer llamado GameMixer con grupo Music
        if (SettingsManager.Instance != null)
        {
            var mixer = SettingsManager.Instance.GetComponent<UnityEngine.Audio.AudioMixer>();
            if (mixer != null && src.outputAudioMixerGroup == null)
            {
                // Intentar asignar automáticamente el grupo "Music"
                var groups = mixer.FindMatchingGroups("Music");
                if (groups.Length > 0) src.outputAudioMixerGroup = groups[0];
            }
        }

        if (src.clip && !src.isPlaying) src.Play();

        SceneManager.activeSceneChanged += OnSceneChanged;
    }


    void OnDestroy() => SceneManager.activeSceneChanged -= OnSceneChanged;

    void OnSceneChanged(Scene oldSc, Scene newSc)
    {
        if (!useDifferentClips) return;

        AudioClip next = null;
        if (newSc.name.Contains("MainMenu")) next = mainMenuClip;
        else if (newSc.name.Contains("Game")) next = gameClip;

        if (next && src.clip != next) { src.clip = next; src.Play(); }
    }
}
