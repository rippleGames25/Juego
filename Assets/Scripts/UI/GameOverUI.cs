using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    
    public void ReturnMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

}
