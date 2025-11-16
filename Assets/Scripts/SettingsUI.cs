using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;        
    [SerializeField] private GameObject settingsPanel;   
    public void Open()
    {
        SFXManager.Instance?.PlayClick();
        hudPanel.SetActive(false);
        settingsPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(true);
    }

    public void Close()
    {
        SFXManager.Instance?.PlayClick();
        settingsPanel.SetActive(false);
        hudPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(false);
    }
}
