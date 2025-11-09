using UnityEngine;
using System.Collections;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;      // Efectos cortos (PlayOneShot)
    [SerializeField] private AudioSource ambientSource;  // Ambiente/clima (loop)

    [Header("Ambient Fades")]
    [SerializeField] private float ambientFadeTime = 0.6f; // segundos (0 = sin fade)
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
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // SFX corto
        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f; // 2D
        // IMPORTANTE: Asigna en el Inspector Output = GameMixer/SFX

        // Ambiente/Clima (loop)
        if (ambientSource == null)
            ambientSource = gameObject.AddComponent<AudioSource>();

        ambientSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.spatialBlend = 0f; // 2D
        // IMPORTANTE: Asigna en el Inspector Output = GameMixer/SFX
    }

    // ---------------------------
    // Core helpers
    // ---------------------------
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

        // Si no hay fade, cortar y reproducir
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

        // Con fade
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

    private IEnumerator CoPlayAmbientWithFade(AudioClip next, float duration)
    {
        // Fade out del actual
        if (ambientSource.isPlaying && ambientSource.volume > 0f)
        {
            float t = 0f;
            float start = ambientSource.volume;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime; // que funcione en pausa
                ambientSource.volume = Mathf.Lerp(start, 0f, t / duration);
                yield return null;
            }
            ambientSource.Stop();
        }

        // Arrancar el nuevo en 0 y subir a 1
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
            ambientSource.volume = 1f; // dejar preparado para el siguiente
        }
        ambientFadeRoutine = null;
    }

    // ---------------------------
    // Atajos: Acción (one shots)
    // ---------------------------
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

    // ---------------------------
    // Atajos: Clima (loop/ambiente)
    // ---------------------------
    public void PlayLluvia() => PlayAmbient(lluvia);
    public void PlayNieve() => PlayAmbient(nieve);
    public void PlaySoleado() => PlayAmbient(soleado);
    public void PlayTormenta() => PlayAmbient(tormenta);
}
