using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum LeoEmotion
{
    Normal,
    Happy,
    Scared
}

public class LeoDialogUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Panel raíz del diálogo de Leo (LeoDialogPanel)")]
    public GameObject panelRoot;              // LeoDialogPanel

    [Tooltip("Imagen del bocadillo de Leo (BubbleLeo)")]
    public Image bubbleLeoImage;              // BubbleLeo

    [Tooltip("Texto del diálogo (DialogueText)")]
    public TMP_Text dialogText;               // DialogueText

    [Tooltip("Botón de continuar (ContinueButton)")]
    public Button continueButton;             // ContinueButton

    [Header("Sprites de bocadillo")]
    public Sprite bubbleNormal;
    public Sprite bubbleHappy;
    public Sprite bubbleScared;

    private float defaultFontSize = 4f;
    private Action onContinueCallback;

    private void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;

        if (dialogText != null)
            defaultFontSize = dialogText.fontSize;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnClick_Continue);
        }

        // Lo dejamos oculto al empezar (en la escena ya está desactivado)
        panelRoot.SetActive(false);
    }

    /// <summary>
    /// Muestra el bocadillo con el texto indicado.
    /// </summary>
    public void Show(string text, LeoEmotion emotion, Action onContinue = null, float fontSize = -1f)
    {
        // Bloquear input general mientras hay diálogo
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInputLocked(true);
        }

        SetEmotion(emotion);

        if (dialogText != null)
        {
            dialogText.text = text;

            if (fontSize > 0f)
                dialogText.fontSize = fontSize;
            else
                dialogText.fontSize = defaultFontSize;
        }

        onContinueCallback = onContinue;

        // Activamos el panel (por si estaba oculto)
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    /// <summary>
    /// Oculta el panel y desbloquea el input.
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInputLocked(false);
        }
    }

    public void SetEmotion(LeoEmotion emotion)
    {
        if (bubbleLeoImage == null) return;

        switch (emotion)
        {
            case LeoEmotion.Happy:
                bubbleLeoImage.sprite = bubbleHappy;
                break;
            case LeoEmotion.Scared:
                bubbleLeoImage.sprite = bubbleScared;
                break;
            case LeoEmotion.Normal:
            default:
                bubbleLeoImage.sprite = bubbleNormal;
                break;
        }
    }

    /// <summary>
    /// Llamado por el botón "Continuar".
    /// </summary>
    public void OnClick_Continue()
    {
        SFXManager.Instance?.PlayClick();
        var cb = onContinueCallback;
        cb?.Invoke();
    }

}
