using UnityEngine;

public class HUDUI : MonoBehaviour
{

    [SerializeField] private GameObject PauseMenuPanel;
    [SerializeField] private GameObject HUDPanel;

    public void Pause()
    {
        HUDPanel.SetActive(false);
        PauseMenuPanel.SetActive(true);
    }


}
