using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject CreditsPanel;

    public void NewGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void Settings()
    {
        Debug.Log("Abriendo ajustes...");
    }

    public void Credits()
    {
        MainMenuPanel.SetActive(false);
        CreditsPanel.SetActive(true);
    }

    public void Quit()
    {
    
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ReturnToMenu()
    {
        CreditsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
}
