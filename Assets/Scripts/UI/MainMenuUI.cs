using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

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

    public void NextLanguage()
    {
        var idiomasDisponibles = LocalizationSettings.AvailableLocales.Locales;
        var idiomaActual = LocalizationSettings.SelectedLocale;

        int indiceActual = 0;

        // Buscamos el índice del idioma actual en la lista
        for (int i = 0; i < idiomasDisponibles.Count; i++)
        {
            if (idiomasDisponibles[i] == idiomaActual)
            {
                indiceActual = i;
                break;
            }
        }

        int siguienteIndice = (indiceActual + 1) % idiomasDisponibles.Count; // el operador % hace que si llega al final, vuelva al 0

        LocalizationSettings.SelectedLocale = idiomasDisponibles[siguienteIndice];
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
