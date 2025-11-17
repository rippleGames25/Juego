using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject CreditsPanel;
    private const string TUTORIAL_SCENE_NAME = "TutorialScene";

    public void NewGame()
    {
        SFXManager.Instance?.PlayClick();
        SceneManager.LoadScene("GameScene");
    }

    public void Credits()
    {
        SFXManager.Instance?.PlayClick();
        MainMenuPanel.SetActive(false);
        CreditsPanel.SetActive(true);
    }

    public void Settings()
    {
        SFXManager.Instance?.PlayClick();
        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void Tutorial()
    {
        SFXManager.Instance?.PlayClick();
        SceneManager.LoadScene("TutorialScene");
    }

    public void Quit()
    {
        SFXManager.Instance?.PlayClick();
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ReturnToMenu()
    {
        SFXManager.Instance?.PlayClick();
        SettingsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
}
