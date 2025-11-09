using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class TutorialUI : MonoBehaviour
{
    
    [SerializeField] private List<GameObject> tutorialPages;

    [SerializeField] private Button nextButton;

    [SerializeField] private TextMeshProUGUI buttonText;

    private int currentPageIndex = 0;
    private const string MAIN_MENU_SCENE_NAME = "MainMenuScene";

    void Start()
    {
        if (tutorialPages == null || tutorialPages.Count == 0)
        {
            CloseTutorial();
            return;
        }

        ShowPage(0);

        UpdateButtonUI();
    }

    private void ShowPage(int index)
    {
        foreach (GameObject page in tutorialPages)
        {
            page.SetActive(false);
        }

        if (index >= 0 && index < tutorialPages.Count)
        {
            tutorialPages[index].SetActive(true);
            currentPageIndex = index;
        }
    }

    private void UpdateButtonUI()
    {
        if (currentPageIndex == tutorialPages.Count - 1)
        {
            buttonText.text = "CERRAR";
        }
        else
        {
            buttonText.text = "SIGUIENTE";
        }
    }

    public void NextPage()
    {
        SFXManager.Instance?.PlayClick();
        if (currentPageIndex == tutorialPages.Count - 1)
        {
            CloseTutorial();
        }
        else
        {
            currentPageIndex++;
            ShowPage(currentPageIndex);
            UpdateButtonUI();
        }
    }

    public void CloseTutorial()
    {
        SFXManager.Instance?.PlayClick();
        SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
    }
}