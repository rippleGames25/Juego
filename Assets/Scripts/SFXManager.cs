using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Mixer Group (asigna GameMixer/SFX)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;      // efectos cortos 
    [SerializeField] private AudioSource ambientSource;  // ambiente/clima 

    [Header("Ambient Fades")]
    [SerializeField] private float ambientFadeTime = 0.6f;
    private Coroutine ambientFadeRoutine;

    [Header("Clips (acción)")]
    [SerializeField] private AudioClip abonar;
    [SerializeField] private AudioClip abono;
    [SerializeField] private AudioClip clickNormal;
    [SerializeField] private AudioClip comprar;
    [SerializeField] private AudioClip crece;
    [SerializeField] private AudioClip denegar;
    [SerializeField] private AudioClip desplantar;
    [SerializeField] private AudioClip infoPlanta;
    [SerializeField] private AudioClip marchita;
    [SerializeField] private AudioClip muerte;
    [SerializeField] private AudioClip pala;
    [SerializeField] private AudioClip pasarDia;
    [SerializeField] private AudioClip plantar;
    [SerializeField] private AudioClip regadera;
    [SerializeField] private AudioClip regar;

    [Header("Clips (clima/ambiente)")]
    [SerializeField] private AudioClip lluvia;
    [SerializeField] private AudioClip nieve;
    [SerializeField] private AudioClip soleado;
    [SerializeField] private AudioClip tormenta;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        // SFX corto 
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;

        // Ambiente/Clima
        if (!ambientSource) ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.spatialBlend = 0f;
        ambientSource.ignoreListenerPause = false; 

        if (sfxMixerGroup)
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            ambientSource.outputAudioMixerGroup = sfxMixerGroup;
        }
        else
        {
            Debug.LogWarning("[SFXManager] No se asignó AudioMixerGroup; los sliders podrían no afectar.");
        }
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldSc, Scene newSc)
    {
        if (!newSc.name.Contains("Game")) StopAmbient();
    }

    public void Play(AudioClip clip)
    {
        if (!clip || !sfxSource) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayAmbient(AudioClip clip)
    {
        if (!ambientSource) return;

        if (ambientFadeRoutine != null)
        {
            StopCoroutine(ambientFadeRoutine);
            ambientFadeRoutine = null;
        }

        if (ambientFadeTime <= 0f)
        {
            if (ambientSource.isPlaying) ambientSource.Stop();
            if (clip)
            {
                ambientSource.clip = clip;
                ambientSource.volume = 1f;
                ambientSource.Play();
            }
            return;
        }

        ambientFadeRoutine = StartCoroutine(CoPlayAmbientWithFade(clip, ambientFadeTime));
    }

    public void StopAmbient()
    {
        if (!ambientSource) return;

        if (ambientFadeRoutine != null)
        {
            StopCoroutine(ambientFadeRoutine);
            ambientFadeRoutine = null;
        }

        if (ambientFadeTime <= 0f)
        {
            ambientSource.Stop();
            return;
        }

        ambientFadeRoutine = StartCoroutine(CoFadeOutAmbient(ambientFadeTime));
    }

    // Pausar/Reanudar sólo el ambiente
    public void PauseAmbient(bool paused)
    {
        if (!ambientSource) return;
        if (paused)
        {
            if (ambientSource.isPlaying) ambientSource.Pause();
        }
        else
        {
            if (ambientSource.clip && !ambientSource.isPlaying) ambientSource.UnPause();
        }
    }

    private IEnumerator CoPlayAmbientWithFade(AudioClip next, float duration)
    {
        if (ambientSource.isPlaying && ambientSource.volume > 0f)
        {
            float t = 0f;
            float start = ambientSource.volume;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                ambientSource.volume = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }
            ambientSource.Stop();
        }

        if (next != null)
        {
            ambientSource.clip = next;
            ambientSource.volume = 0f;
            ambientSource.Play();

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                ambientSource.volume = Mathf.Lerp(0f, 1f, t / duration);
                yield return null;
            }
            ambientSource.volume = 1f;
        }

        ambientFadeRoutine = null;
    }

    private IEnumerator CoFadeOutAmbient(float duration)
    {
        if (ambientSource.isPlaying && ambientSource.volume > 0f)
        {
            float t = 0f;
            float start = ambientSource.volume;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                ambientSource.volume = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }
            ambientSource.Stop();
            ambientSource.volume = 1f; 
        }
        ambientFadeRoutine = null;
    }

  
    // Atajos
    public void PlayAbonar() => Play(abonar);
    public void PlayAbono() => Play(abono);
    public void PlayClick() => Play(clickNormal);
    public void PlayComprar() => Play(comprar);
    public void PlayCrece() => Play(crece);
    public void PlayDenegar() => Play(denegar);
    public void PlayDesplantar() => Play(desplantar);
    public void PlayInfoPlanta() => Play(infoPlanta);
    public void PlayMarchita() => Play(marchita);
    public void PlayMuerte() => Play(muerte);
    public void PlayPala() => Play(pala);
    public void PlayPasarDia() => Play(pasarDia);
    public void PlayPlantar() => Play(plantar);
    public void PlayRegadera() => Play(regadera);
    public void PlayRegar() => Play(regar);


    // Atajos: Clima
    public void PlayLluvia() => PlayAmbient(lluvia);
    public void PlayNieve() => PlayAmbient(nieve);
    public void PlaySoleado() => PlayAmbient(soleado);
    public void PlayTormenta() => PlayAmbient(tormenta);
}
