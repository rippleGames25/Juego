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
    [Tooltip("Panel raíz del diálogo de Leo")]
    public GameObject panelRoot;

    [Tooltip("Imagen del bocadillo de Leo")]
    public Image bubbleLeoImage;

    [Tooltip("Texto del diálogo")]
    public TMP_Text dialogText;

    [Tooltip("Botón de continuar")]
    public Button continueButton;

    [Header("Sprites de bocadillo")]
    public Sprite bubbleNormal;
    public Sprite bubbleHappy;
    public Sprite bubbleScared;

    private float defaultFontSize = 4f;
    private Action onContinueCallback;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (dialogText != null)
            defaultFontSize = dialogText.fontSize;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnClick_Continue);
        }

        panelRoot.SetActive(false);
    }

    public void Show(string text, LeoEmotion emotion, Action onContinue = null, float fontSize = -1f)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetInputLocked(true);

        SetEmotion(emotion);

        if (dialogText != null)
        {
            dialogText.text = text;
            dialogText.fontSize = (fontSize > 0f) ? fontSize : defaultFontSize;
        }

        onContinueCallback = onContinue;

        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.SetInputLocked(false);
    }

    public void SetEmotion(LeoEmotion emotion)
    {
        if (bubbleLeoImage == null)
            return;

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

    public void OnClick_Continue()
    {
        SFXManager.Instance?.PlayClick();
        onContinueCallback?.Invoke();
    }
}
