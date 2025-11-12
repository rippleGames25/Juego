/*
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuPanel;
    [SerializeField] private GameObject SettingsPanel;
    [SerializeField] private GameObject CreditsPanel;
    [SerializeField] private GameObject SaveSlotsPanel;
    private const string TUTORIAL_SCENE_NAME = "TutorialScene";

    public void NewGame()
    {
        // SceneManager.LoadScene("GameScene");
        MainMenuPanel.SetActive(false);
        SaveSlotsPanel.SetActive(true);

    }

    public void Credits()
    {
        MainMenuPanel.SetActive(false);
        CreditsPanel.SetActive(true);
    }

    public void Settings()
    {
        MainMenuPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    public void Tutorial()
    {
        SceneManager.LoadScene(TUTORIAL_SCENE_NAME);
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
        SettingsPanel.SetActive(false);
        CreditsPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
}
*/