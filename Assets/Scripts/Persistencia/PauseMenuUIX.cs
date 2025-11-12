/*
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenuPanel;
    [SerializeField] private GameObject HUDPanel;

    public void Return()
    {
        SFXManager.Instance?.PlayClick();
        PauseMenuPanel.SetActive(false);
        HUDPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(false); 
    }

    public void Settings()
    {
        SFXManager.Instance?.PlayClick();
        PauseMenuPanel.SetActive(false);
        Debug.Log("Abriendo ajustes...");
    }

    public void Quit()
    {
        SFXManager.Instance?.PlayClick();
        SceneManager.LoadScene("GameOverScene");
    }


}
*/