using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Localization;

public class GameOverUI : MonoBehaviour
{
    [Header("Textos de Estadísticas")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI daysSurvivedText;
    [SerializeField] private TextMeshProUGUI maxBiodiversityText;
    [SerializeField] private TextMeshProUGUI maxMaturePlantsText;

    [Header("Localizacion")]
    [SerializeField] private LocalizedString win;
    [SerializeField] private LocalizedString lose;
    [SerializeField] private LocalizedString days;
    [SerializeField] private LocalizedString biodiversity;
    [SerializeField] private LocalizedString species;

    void Start()
    {
        Time.timeScale = 1f;

        // Texto de derrota
        bool hasWon = false;

        // Rellenamos las estadísticas desde el Singleton
        if (GameSessionStats.Instance != null)
        {
            hasWon = GameSessionStats.Instance.didWinGame;

            daysSurvivedText.text = days.GetLocalizedString() + GameSessionStats.Instance.daysSurvived;
            maxBiodiversityText.text = biodiversity.GetLocalizedString() + GameSessionStats.Instance.maxBiodiversityAchieved;
            maxMaturePlantsText.text = species.GetLocalizedString() + GameSessionStats.Instance.maxMaturePlantsAchieved;
        }
        else
        {
            // Fallback por si testeamos desde la escena de GameOver
            daysSurvivedText.text = days.GetLocalizedString() + " N/A";
            maxBiodiversityText.text = biodiversity.GetLocalizedString() + " N/A";
            maxMaturePlantsText.text = species.GetLocalizedString() + " N/A";
        }

        if (hasWon)
        {
            titleText.text = win.GetLocalizedString();
        }
        else
        {
            titleText.text = lose.GetLocalizedString();
        }
    }

    public void RetryGame()
    {
        SFXManager.Instance?.PlayClick();

        // Resetea las stats para la nueva partida
        GameSessionStats.Instance?.ResetStats();
        SceneManager.LoadScene("GameScene");
    }

    public void ReturnMenu()
    {
        SFXManager.Instance?.PlayClick();

        // Resetea las stats
        GameSessionStats.Instance?.ResetStats();
        SceneManager.LoadScene("MainMenuScene");
    }
}