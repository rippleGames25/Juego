using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenuPanel;
    [SerializeField] private GameObject HUDPanel;

    public void Return()
    {
        SFXManager.Instance?.PlayClick();
        SFXManager.Instance?.PauseAmbient(false);
        PauseMenuPanel.SetActive(false);
        HUDPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(false); 
    }

    public void Settings()
    {
        SFXManager.Instance?.PlayClick();
        PauseMenuPanel.SetActive(false);
        Debug.Log("Abriendo ajustes...");
        GameManager.Instance.SetInputLocked(true);
    }

    public void Quit()
    {
        SFXManager.Instance?.PlayClick();
        SFXManager.Instance?.StopAmbient();
        SceneManager.LoadScene("GameOverScene");
    }
}
