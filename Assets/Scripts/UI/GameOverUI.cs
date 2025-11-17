using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Textos de Estadísticas")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI daysSurvivedText;
    [SerializeField] private TextMeshProUGUI maxBiodiversityText;
    [SerializeField] private TextMeshProUGUI maxMaturePlantsText;

    void Start()
    {
        Time.timeScale = 1f;

        // Texto de derrota
        bool hasWon = false;

        // Rellenamos las estadísticas desde el Singleton
        if (GameSessionStats.Instance != null)
        {
            hasWon = GameSessionStats.Instance.didWinGame;

            daysSurvivedText.text = $"Días de Resistencia: {GameSessionStats.Instance.daysSurvived}";
            maxBiodiversityText.text = $"Biodiversidad Máxima: {GameSessionStats.Instance.maxBiodiversityAchieved}";
            maxMaturePlantsText.text = $"Máx. Especies Maduras: {GameSessionStats.Instance.maxMaturePlantsAchieved}";
        }
        else
        {
            // Fallback por si testeamos desde la escena de GameOver
            daysSurvivedText.text = "Días de Resistencia: N/A";
            maxBiodiversityText.text = "Biodiversidad Máxima: N/A";
            maxMaturePlantsText.text = "Máx. Especies Maduras: N/A";
        }

        if (hasWon)
        {
            titleText.text = "¡El Santuario ha triunfado!";
        }
        else
        {
            titleText.text = "Has perdido el Santuario";
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