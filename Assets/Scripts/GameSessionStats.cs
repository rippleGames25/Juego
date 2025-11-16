using UnityEngine;

public class GameSessionStats : MonoBehaviour
{
    public static GameSessionStats Instance;

    public int daysSurvived = 0;
    public int maxBiodiversityAchieved = 0;
    public int maxMaturePlantsAchieved = 0;
    public bool didWinGame = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetStats()
    {
        daysSurvived = 0;
        maxBiodiversityAchieved = 0;
        maxMaturePlantsAchieved = 0;
        didWinGame = false;
}
}