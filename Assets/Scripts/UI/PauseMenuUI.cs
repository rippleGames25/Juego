using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenuPanel;
    [SerializeField] private GameObject HUDPanel;

    public void Return()
    {
        PauseMenuPanel.SetActive(false);
        HUDPanel.SetActive(true);

        // Reanudar el tiempo
    }

    public void Settings()
    {
        Debug.Log("Abriendo ajustes...");
    }

    public void Quit()
    {
        SceneManager.LoadScene("GameOverScene");
    }
}
