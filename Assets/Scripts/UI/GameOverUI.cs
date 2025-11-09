using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    
    public void ReturnMenu()
    {
        SFXManager.Instance?.PlayClick();
        SceneManager.LoadScene("MainMenuScene");
    }

}
