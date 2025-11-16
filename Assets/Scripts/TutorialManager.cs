using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Paneles del tutorial (en orden)")]
    [SerializeField] private List<GameObject> panels = new List<GameObject>();
    // Panel 0  = Intro Leo (botón "Empezar tutorial")
    // Panel 1  = Primer panel "en el jardín"
    // ...
    // Panel 23 = Último panel con Leo + botón "Volver al menú"

    [Header("Fade")]
    [SerializeField] private Image fadeImage;     
    [SerializeField] private float fadeDuration = 0.5f;

    private int currentIndex = -1;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var p in panels)
        {
            if (p != null) p.SetActive(false);
        }

        if (fadeImage != null)
        {
            var c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    private IEnumerator Start()
    {
  
        ShowPanel(0);

        if (fadeImage != null)
        {
            yield return Fade(1f, 0f);
        }
    }


    public void OnNextButtonPressed()
    {
        SFXManager.Instance?.PlayClick();
        if (isTransitioning) return;

        int lastIndex = panels.Count - 1;
        if (currentIndex >= lastIndex) return; 

        int targetIndex = currentIndex + 1;


        if (currentIndex == 0 || currentIndex == lastIndex - 1)
        {
            StartCoroutine(SwitchWithFade(targetIndex));
        }
        else
        {
            ShowPanel(targetIndex);
        }
    }

    public void OnBackToMenuButtonPressed()
    {
        SFXManager.Instance?.PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }


    private void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Count)
        {
            Debug.LogError($"[TutorialManager] Índice de panel fuera de rango: {index}");
            return;
        }

        if (currentIndex >= 0 && currentIndex < panels.Count)
        {
            if (panels[currentIndex] != null)
                panels[currentIndex].SetActive(false);
        }

        if (panels[index] != null)
        {
            panels[index].SetActive(true);
        }

        currentIndex = index;
    }

    private IEnumerator SwitchWithFade(int targetIndex)
    {
        isTransitioning = true;

        
        if (fadeImage != null)
        {
            yield return Fade(0f, 1f);
        }

        
        ShowPanel(targetIndex);

        
        if (fadeImage != null)
        {
            yield return Fade(1f, 0f);
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);

        Color c = fadeImage.color;
        c.a = from;
        fadeImage.color = c;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            c.a = a;
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

       
        if (Mathf.Approximately(to, 0f))
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
}
