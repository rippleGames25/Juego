using UnityEngine;
using UnityEngine.UI;

public class LeoIntroUI : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject leoIntroPanel;   // El propio panel
    public HUDUI hudUI;                // Referencia al HUDUI de la escena

    private void Awake()
    {
        // Si no lo asignas a mano, intenta autodescubrirlo
        if (hudUI == null)
        {
            hudUI = FindObjectOfType<HUDUI>();
        }
    }

    public void Show()
    {
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInputLocked(true);
        }

        leoIntroPanel.SetActive(true);
    }


    public void Hide()
    {
        leoIntroPanel.SetActive(false);
    }

    public void OnClick_GuideMe()
    {
        TutorialManager.Instance.StartDay1Tutorial();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInputLocked(false);
        }

        Hide();

        if (hudUI != null)
        {
            hudUI.StartGameAfterLeoIntro();
        }
    }
    public void OnClick_SkipHelp()
    {
        TutorialManager.Instance.SkipDay1Tutorial();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetInputLocked(false);
        }

        Hide();

        if (hudUI != null)
        {
            hudUI.StartGameAfterLeoIntro();
        }
    }


}
